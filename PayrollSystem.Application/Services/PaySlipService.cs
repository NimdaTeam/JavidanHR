using Microsoft.Extensions.Logging;
using PayrollSystem.Application.DTOs;
using PayrollSystem.Application.Interfaces;
using PayrollSystem.Domain.Entities.CalculationLog;
using PayrollSystem.Domain.Entities.PayItem;
using PayrollSystem.Domain.Entities.PaySlip;
using PayrollSystem.Domain.Interfaces.CalculationLog;
using PayrollSystem.Domain.Interfaces.Contract;
using PayrollSystem.Domain.Interfaces.PayItem;
using PayrollSystem.Domain.Interfaces.PaySlip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace PayrollSystem.Application.Services
{
    /// <summary>
    /// Application service for generating and managing pay slips.
    /// </summary>
    public class PaySlipService : IPaySlipService
    {
        private readonly IPaySlipRepository _paySlipRepository;
        private readonly IContractRepository _contractRepository;
        private readonly IPayItemRepository _payItemRepository;
        private readonly ICalculationLogRepository _calculationLogRepository;
        private readonly IPayItemService _payItemAppService; // For formula evaluation
        private readonly ILogger<PaySlipService> _logger;

        public PaySlipService(
            IPaySlipRepository paySlipRepository,
            IContractRepository contractRepository,
            IPayItemRepository payItemRepository,
            ICalculationLogRepository calculationLogRepository,
            IPayItemService payItemAppService,
            ILogger<PaySlipService> logger)
        {
            _paySlipRepository = paySlipRepository ?? throw new ArgumentNullException(nameof(paySlipRepository));
            _contractRepository = contractRepository ?? throw new ArgumentNullException(nameof(contractRepository));
            _payItemRepository = payItemRepository ?? throw new ArgumentNullException(nameof(payItemRepository));
            _calculationLogRepository = calculationLogRepository ?? throw new ArgumentNullException(nameof(calculationLogRepository));
            _payItemAppService = payItemAppService ?? throw new ArgumentNullException(nameof(payItemAppService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<PaySlipDto> GeneratePaySlipAsync(GeneratePaySlipCommand command, CancellationToken cancellationToken = default)
        {
            // 1. Check if pay slip already exists for the same period
            var existing = await _paySlipRepository.GetByEmployeeAndDateAsync(command.EmployeeId, command.Year, command.Month, cancellationToken);
            if (existing != null)
                throw new InvalidOperationException($"Pay slip for {command.Year}/{command.Month} already exists.");

            // 2. Get active contract on the first day of the month
            var firstDayOfMonth = new DateTime(command.Year, command.Month, 1);
            var contract = await _contractRepository.GetActiveContractByEmployeeAsync(command.EmployeeId, firstDayOfMonth, cancellationToken);
            if (contract == null)
                throw new InvalidOperationException("No active contract found for the employee in the given period.");

            // 3. Fetch all active pay items
            var allPayItems = await _payItemRepository.GetActivePayItemsAsync(cancellationToken);
            if (allPayItems.Count == 0)
                throw new InvalidOperationException("No active pay items defined.");

            // 4. Get employee payroll data from HR subsystem
            var employeeContract =
                await _contractRepository.GetActiveContractByEmployeeAsync(command.EmployeeId, firstDayOfMonth,
                    cancellationToken);
            if (employeeContract is null)
                throw new InvalidOperationException("No active contract found.");

            var employeeData =
                await _contractRepository.GetWithPayItemsAsync(employeeContract.Id, cancellationToken);
            if (employeeData is null)
                throw new InvalidOperationException("No active contract found.");

            // 5. Build calculation context (variables for formulas)
            var context = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in employeeData.PayItems)
            {
                var payItem = await _payItemRepository.GetAsNoTrackingAsync(kv.PayItemId, cancellationToken);
                if (payItem is null)
                    continue;

                context[payItem.SystemCode] = kv.Value ?? 0;
            }


            // Add contract-specific values (from ContractPayItem)
            var contractWithItems = await _contractRepository.GetWithPayItemsAsync(contract.Id, cancellationToken);
            if (contractWithItems != null)
            {
                foreach (var contractItem in contractWithItems.PayItems)
                {
                    var payItem = allPayItems.FirstOrDefault(p => p.Id == contractItem.PayItemId);
                    if (payItem != null && contractItem.Value.HasValue)
                        context[payItem.SystemCode] = contractItem.Value.Value;
                }
            }

            // 6. Create new PaySlip aggregate
            var paySlip = new PaySlip(command.EmployeeId, contract.Id, command.Year, command.Month, command.IssueDate);

            // 7. Calculate each pay item and add to pay slip
            foreach (var payItem in allPayItems)
            {
                decimal calculatedValue = 0;
                string? formulaUsed = null;
                string? inputJson = null;

                switch (payItem.DataType)
                {
                    case PayItem.PayItemDataType.Formula:
                        var activeFormula = await _payItemRepository.GetActiveFormulaAsync(payItem.Id, firstDayOfMonth, cancellationToken);
                        if (activeFormula != null)
                        {
                            formulaUsed = activeFormula.Formula;
                            inputJson = JsonSerializer.Serialize(context);
                            calculatedValue = await _payItemAppService.EvaluateFormulaAsync(payItem.Id, activeFormula.Formula, context, cancellationToken);
                        }
                        break;

                    case PayItem.PayItemDataType.Decimal:
                        // Fixed value from context (e.g., base salary from HR)
                        calculatedValue = context.GetValueOrDefault(payItem.SystemCode, 0);
                        break;

                    case PayItem.PayItemDataType.Boolean:
                        // Boolean values become 0 or 1
                        calculatedValue = context.GetValueOrDefault(payItem.SystemCode, 0);
                        break;

                    case PayItem.PayItemDataType.UserInput:
                        // Skip – value will be provided later via manual override
                        continue;
                }

                paySlip.AddItem(payItem.Id, calculatedValue);

                // Log calculation for formula-based items
                if (payItem.DataType == PayItem.PayItemDataType.Formula && formulaUsed != null)
                {
                    var log = new CalculationLog(paySlip.Id, payItem.Id, inputJson ?? "{}", formulaUsed, calculatedValue);
                    await _calculationLogRepository.AddAsync(log, cancellationToken);
                }
            }

            // 8. Recalculate totals using pay item types (Earning/Deduction)
            paySlip.RecalculateTotals(payItemId =>
            {
                var pi = allPayItems.First(p => p.Id == payItemId);
                return pi.Type;
            });

            // 9. Save pay slip
            await _paySlipRepository.AddAsync(paySlip, cancellationToken);
            await _paySlipRepository.SaveChangesAsync(cancellationToken);

            // 10. Return DTO
            return await MapToDtoAsync(paySlip, allPayItems, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<PaySlipDto?> GetPaySlipAsync(long id, CancellationToken cancellationToken = default)
        {
            var paySlip = await _paySlipRepository.GetWithItemsAsync(id, cancellationToken);
            if (paySlip == null)
                return null;

            var allPayItems = await _payItemRepository.GetActivePayItemsAsync(cancellationToken);
            return await MapToDtoAsync(paySlip, allPayItems, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<List<PaySlipDto>> GetPaySlipsByEmployeeAsync(long employeeId, CancellationToken cancellationToken = default)
        {
            // First get all contracts for the employee, then pay slips via contract.
            var contracts = await _contractRepository.GetContractsByEmployeeAsync(employeeId, cancellationToken);
            var contractIds = contracts.Select(c => c.Id).ToList();

            if (contractIds.Count == 0)
                return new List<PaySlipDto>();

            var paySlips = new List<PaySlip>();
            foreach (var contractId in contractIds)
            {
                var slips = await _paySlipRepository.GetPaySlipsByContractAsync(contractId, cancellationToken);
                paySlips.AddRange(slips);
            }

            var allPayItems = await _payItemRepository.GetActivePayItemsAsync(cancellationToken);
            var result = new List<PaySlipDto>();
            foreach (var ps in paySlips)
                result.Add(await MapToDtoAsync(ps, allPayItems, cancellationToken));

            return result;
        }

        /// <inheritdoc />
        public async Task<bool> OverridePaySlipItemAsync(OverridePaySlipItemCommand command, CancellationToken cancellationToken = default)
        {
            var paySlip = await _paySlipRepository.GetWithItemsAsync(command.PaySlipId, cancellationToken);
            if (paySlip == null)
                return false;

            if (paySlip.Status == PaySlipStatus.Finalized)
                throw new InvalidOperationException("Cannot override item on a finalized pay slip.");

            paySlip.OverrideItemValue(command.PayItemId, command.NewValue);

            // Recalculate totals after override
            var allPayItems = await _payItemRepository.GetActivePayItemsAsync(cancellationToken);
            paySlip.RecalculateTotals(pid => allPayItems.First(p => p.Id == pid).Type);

            await _paySlipRepository.UpdateAsync(paySlip, cancellationToken);
            await _paySlipRepository.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> FinalizePaySlipAsync(long paySlipId, CancellationToken cancellationToken = default)
        {
            var paySlip = await _paySlipRepository.GetAsync(paySlipId, cancellationToken);
            if (paySlip == null)
                return false;

            paySlip.Finalize();
            await _paySlipRepository.UpdateAsync(paySlip, cancellationToken);
            await _paySlipRepository.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <inheritdoc />
        public async Task<PaySlipDto> RecalculatePaySlipAsync(long paySlipId, CancellationToken cancellationToken = default)
        {
            // Fetch existing pay slip with its items
            var paySlip = await _paySlipRepository.GetWithItemsAsync(paySlipId, cancellationToken);
            if (paySlip == null)
                throw new ArgumentException($"Pay slip with id {paySlipId} not found.");

            if (paySlip.Status == PaySlipStatus.Finalized)
                throw new InvalidOperationException("Cannot recalculate a finalized pay slip. Revert to draft first.");

            // Clear existing calculation logs for this pay slip (optional)
            await _calculationLogRepository.DeleteByPaySlipIdAsync(paySlipId, cancellationToken);
            await _calculationLogRepository.SaveChangesAsync(cancellationToken);

            // Re‑generate calculation context (same as in GeneratePaySlipAsync)
            var firstDayOfMonth = new DateTime(paySlip.Year, paySlip.Month, 1);
            var contract = await _contractRepository.GetAsync(paySlip.ContractId, cancellationToken);
            if (contract == null)
                throw new InvalidOperationException("Contract not found.");

            var allPayItems = await _payItemRepository.GetActivePayItemsAsync(cancellationToken);

            // 4. Get employee payroll data from HR subsystem
            var employeeContract =
                await _contractRepository.GetActiveContractByEmployeeAsync(paySlip.EmployeeId, firstDayOfMonth,
                    cancellationToken);
            if (employeeContract is null)
                throw new InvalidOperationException("No active contract found.");

            var employeeData =
                await _contractRepository.GetWithPayItemsAsync(employeeContract.Id, cancellationToken);
            if (employeeData is null)
                throw new InvalidOperationException("No active contract found.");

            // 5. Build calculation context (variables for formulas)
            var context = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in employeeData.PayItems)
            {
                var payItem = await _payItemRepository.GetAsNoTrackingAsync(kv.PayItemId, cancellationToken);
                if (payItem is null)
                    continue;

                context[payItem.SystemCode] = kv.Value ?? 0;
            }

            var contractWithItems = await _contractRepository.GetWithPayItemsAsync(contract.Id, cancellationToken);
            if (contractWithItems != null)
            {
                foreach (var contractItem in contractWithItems.PayItems)
                {
                    var payItem = allPayItems.FirstOrDefault(p => p.Id == contractItem.PayItemId);
                    if (payItem != null && contractItem.Value.HasValue)
                        context[payItem.SystemCode] = contractItem.Value.Value;
                }
            }

            // Remove all existing items and re‑add them with recalculated values
            // (We can't modify the collection directly, so we create a new list and replace)
            var existingItems = paySlip.Items.ToList();
            foreach (var item in existingItems)
                paySlip.RemoveItem(item.PayItemId);  // custom method in PaySlip

            // Re‑add each pay item
            foreach (var payItem in allPayItems)
            {
                decimal calculatedValue = 0;
                string? formulaUsed = null;
                string? inputJson = null;

                switch (payItem.DataType)
                {
                    case PayItem.PayItemDataType.Formula:
                        var activeFormula = await _payItemRepository.GetActiveFormulaAsync(payItem.Id, firstDayOfMonth, cancellationToken);
                        if (activeFormula != null)
                        {
                            formulaUsed = activeFormula.Formula;
                            inputJson = JsonSerializer.Serialize(context);
                            calculatedValue = await _payItemAppService.EvaluateFormulaAsync(payItem.Id, activeFormula.Formula, context, cancellationToken);
                        }
                        break;
                    case PayItem.PayItemDataType.Decimal:
                        calculatedValue = context.GetValueOrDefault(payItem.SystemCode, 0);
                        break;
                    case PayItem.PayItemDataType.Boolean:
                        calculatedValue = context.GetValueOrDefault(payItem.SystemCode, 0);
                        break;
                    case PayItem.PayItemDataType.UserInput:
                        // Preserve previous override if exists
                        var existingItem = existingItems.FirstOrDefault(i => i.PayItemId == payItem.Id);
                        var manualValue = existingItem?.ManualOverrideValue;
                        calculatedValue = manualValue ?? 0;
                        break;
                }

                paySlip.AddItem(payItem.Id, calculatedValue);

                if (payItem.DataType == PayItem.PayItemDataType.Formula && formulaUsed != null)
                {
                    var log = new CalculationLog(paySlip.Id, payItem.Id, inputJson ?? "{}", formulaUsed, calculatedValue);
                    await _calculationLogRepository.AddAsync(log, cancellationToken);
                }
            }

            // Recalculate totals
            paySlip.RecalculateTotals(pid => allPayItems.First(p => p.Id == pid).Type);

            // Update and save
            await _paySlipRepository.UpdateAsync(paySlip, cancellationToken);
            await _paySlipRepository.SaveChangesAsync(cancellationToken);

            return await MapToDtoAsync(paySlip, allPayItems, cancellationToken);
        }

        #region Private Helpers

        private async Task<PaySlipDto> MapToDtoAsync(PaySlip paySlip, List<PayItem> allPayItems, CancellationToken cancellationToken)
        {
            // If pay slip items are not already loaded, fetch them
            if (!paySlip.Items.Any())
            {
                var fullPaySlip = await _paySlipRepository.GetWithItemsAsync(paySlip.Id, cancellationToken);
                if (fullPaySlip != null)
                    paySlip = fullPaySlip;
            }

            var dto = new PaySlipDto
            {
                Id = paySlip.Id,
                EmployeeId = paySlip.EmployeeId,
                ContractId = paySlip.ContractId,
                Year = paySlip.Year,
                Month = paySlip.Month,
                Status = paySlip.Status.ToString(),
                IssueDate = paySlip.IssueDate,
                TotalEarnings = paySlip.TotalEarnings,
                TotalDeductions = paySlip.TotalDeductions,
                NetPay = paySlip.NetPay,
                Items = paySlip.Items.Select(item =>
                {
                    var payItem = allPayItems.FirstOrDefault(p => p.Id == item.PayItemId);
                    return new PaySlipItemDto
                    {
                        PayItemId = item.PayItemId,
                        PayItemName = payItem?.Name ?? "Unknown",
                        CalculatedValue = item.CalculatedValue,
                        ManualOverrideValue = item.ManualOverrideValue,
                        FinalValue = item.FinalValue
                    };
                }).ToList()
            };
            return dto;
        }

        #endregion
    }
}
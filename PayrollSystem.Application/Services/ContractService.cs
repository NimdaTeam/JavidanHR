using DNTPersianUtils.Core;
using HrSystem.Application.Interfaces;
using Microsoft.Extensions.Logging;
using PayrollSystem.Application.DTOs;
using PayrollSystem.Application.Interfaces;
using PayrollSystem.Domain.Common;
using PayrollSystem.Domain.Entities.Contract;
using PayrollSystem.Domain.Interfaces.Contract;
using PayrollSystem.Domain.Interfaces.PayItem;
using PayrollSystem.Domain.Interfaces.Workshop;

namespace PayrollSystem.Application.Services;

public class ContractService : IContractService
{
    private readonly IContractRepository _contractRepo;
    private readonly IWorkshopRepository _workshopRepo;
    private readonly IPayItemRepository _payItemRepo;
    private readonly ILogger<ContractService> _logger;
    private readonly IEmployeeService _employeeService;
    private readonly IPayItemService _payItemService;

    public ContractService(IContractRepository contractRepo, IWorkshopRepository workshopRepo, IPayItemRepository payItemRepo, ILogger<ContractService> logger, IEmployeeService employeeService, IPayItemService payItemService)
    {
        _contractRepo = contractRepo;
        _workshopRepo = workshopRepo;
        _payItemRepo = payItemRepo;
        _logger = logger;
        _employeeService = employeeService;
        _payItemService = payItemService;
    }

    public async Task<List<ContractDto>> GetAllContractsWithDtoAsync(CancellationToken cancellationToken = default)
    {
        var contracts = await _contractRepo.GetAllWithIncludesAsync(null, null, cancellationToken, x => x.PayItems);

        var list = new List<ContractDto>();
        foreach (var ct in contracts)
        {
            var mapped = await MapToDto(ct, cancellationToken);
            var employee = await _employeeService.GetById(ct.EmployeeId);
            var workshop = await _workshopRepo.GetAsync(ct.WorkshopId, cancellationToken);
            if(employee is null || workshop is null)
                continue;

            mapped.EmployeeName = employee.GetFullName();
            mapped.WorkshopName = workshop.Name;

            list.Add(mapped);
        }

        return list;
    }


	public async Task<ContractDto?> CreateContractAsync(CreateContractDto dto, CancellationToken cancellationToken = default)
	{
		// ─────────────────────────────────────────────────────────────────
		// Validate workshop exists
		// ─────────────────────────────────────────────────────────────────
		var workshop = await _workshopRepo.GetAsync(dto.WorkshopId, cancellationToken);
		if (workshop == null)
			throw new ArgumentException("کارگاه پیدا نشد");

		// ─────────────────────────────────────────────────────────────────
		// Convert Jalali dates to Gregorian
		// ─────────────────────────────────────────────────────────────────
		var validFromDate = dto.ValidFromDateJalali.ToGregorianDateTime();
		var validToDate = dto.ValidToDateJalali.ToGregorianDateTime();

		// ─────────────────────────────────────────────────────────────────
		// Check for overlapping contracts
		// ─────────────────────────────────────────────────────────────────
		var overlap = await _contractRepo.HasOverlapAsync(
			dto.EmployeeId,
			(DateTime)validFromDate!,
			validToDate,
			null,
			cancellationToken);

		if (overlap)
			throw new InvalidOperationException("تاریخ قرارداد با قرارداد های دیگر کارمند تداخل دارد.");

		// ─────────────────────────────────────────────────────────────────
		// Create contract entity (outside transaction)
		// ─────────────────────────────────────────────────────────────────
		var contract = new Contract(
			dto.EmployeeId,
			dto.WorkshopId,
			(DateTime)validFromDate,
			validToDate);

		// ─────────────────────────────────────────────────────────────────
		// Execute in transaction (reuse the same contract object)
		// ─────────────────────────────────────────────────────────────────
		var result = await _contractRepo.ExecuteInTransactionAsync(async () =>
		{
			// Step 1: Add contract
			await _contractRepo.AddAsync(contract, cancellationToken);

			// Step 2: Save to get contract.Id
			var saved = await _contractRepo.SaveChangesAsync(cancellationToken);
			if (!saved)
				return false;

			// Step 3: Now add pay items (contract.Id is valid now)
			foreach (var payItemDto in dto.PayItems)
			{
				if (payItemDto.IsSystemItem)
				{
					if (!PayItemConstants.IsCodeExist(payItemDto.SystemCode!))
						throw new InvalidOperationException($"عامل سیستمی با کد {payItemDto.SystemCode} یافت نشد");

					contract.AddSystemPayItem(payItemDto.SystemCode!, payItemDto.Value);
				}
				else
				{
					if (!payItemDto.PayItemId.HasValue)
						throw new ArgumentException("شناسه عامل حقوقی الزامی است");

					var payItem = await _payItemService.GetPayItemByIdAsync(
						payItemDto.PayItemId.Value,
						cancellationToken);

					if (payItem == null)
						throw new ArgumentException($"عامل حقوقی با شناسه {payItemDto.PayItemId} یافت نشد");

					contract.AddPayItem(payItemDto.PayItemId.Value, payItemDto.Value);
				}
			}

			// Step 4: Save pay items
			var payItemsSaved = await _contractRepo.SaveChangesAsync(cancellationToken);
			return true;

		}, cancellationToken);

		if (!result)
			return null;

		// Now contract has Id and pay items loaded
		return await MapToDto(contract, cancellationToken);
	}


	public async Task<ContractDto?> GetContractByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var contract = await _contractRepo.GetWithPayItemsAsync(id, cancellationToken);
        if (contract == null) return null;
        return await MapToDto(contract, cancellationToken);
    }

    public async Task<ContractDto?> GetActiveContractByEmployeeAsync(long employeeId, DateTime? atDate = null, CancellationToken cancellationToken = default)
    {
        var contract = await _contractRepo.GetActiveContractByEmployeeAsync(employeeId, atDate, cancellationToken);
        if (contract == null) return null;
        return await MapToDto(contract, cancellationToken);
    }

    public async Task<List<ContractDto>> GetContractsByEmployeeAsync(long employeeId, CancellationToken cancellationToken = default)
    {
        var contracts = await _contractRepo.GetContractsByEmployeeAsync(employeeId, cancellationToken);
        var result = new List<ContractDto>();
        foreach (var c in contracts)
            result.Add(await MapToDto(c, cancellationToken));
        return result;
    }

    public async Task<bool> AssignPayItemToContractAsync(AssignPayItemToContractDto dto, CancellationToken cancellationToken = default)
    {
        var contract = await _contractRepo.GetAsync(dto.ContractId, cancellationToken);
        if (contract == null) return false;

        var payItem = await _payItemRepo.GetAsync(dto.PayItemId, cancellationToken);
        if (payItem == null) throw new ArgumentException("Pay item not found");

        contract.AddPayItem(dto.PayItemId, dto.Value);
        await _contractRepo.UpdateAsync(contract, cancellationToken);
        await _contractRepo.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemovePayItemFromContractAsync(long contractId, long payItemId, CancellationToken cancellationToken = default)
    {
        var contract = await _contractRepo.GetWithPayItemsAsync(contractId, cancellationToken);
        if (contract == null) return false;

        contract.RemovePayItem(payItemId);
        await _contractRepo.UpdateAsync(contract, cancellationToken);
        await _contractRepo.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> TerminateContractAsync(long contractId, DateTime terminationDate, CancellationToken cancellationToken = default)
    {
        var contract = await _contractRepo.GetAsync(contractId, cancellationToken);
        if (contract == null) return false;

        contract.Terminate();
        await _contractRepo.UpdateAsync(contract, cancellationToken);
        await _contractRepo.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<List<Contract>> GetAllContractsAsync(CancellationToken cancellationToken = default)
    {
        return await _contractRepo.GetAllWithIncludesAsync(null, null, cancellationToken, x => x.PayItems);
    }

	public async Task<ContractDto?> GetContractForEditAsync(int id, CancellationToken cancellationToken = default)
	{
		var contract = await _contractRepo.GetWithIncludesAsync(id, cancellationToken,x=>x.PayItems);
		if (contract == null)
			return null;

		return await MapToDto(contract, cancellationToken);
	}

	public async Task<bool> UpdateContractAsync(EditContractDto dto, CancellationToken cancellationToken = default)
	{
		// ─────────────────────────────────────────────────────────────────
		// Fetch existing contract with its pay items
		// ─────────────────────────────────────────────────────────────────
		var contract = await _contractRepo.GetWithIncludesAsync(dto.Id, cancellationToken, x => x.PayItems);
		if (contract == null)
			throw new ArgumentException("قرارداد یافت نشد");

		// ─────────────────────────────────────────────────────────────────
		// Convert Jalali dates
		// ─────────────────────────────────────────────────────────────────
		var validFromDate = dto.ValidFromDateJalali.ToGregorianDateTime();
		var validToDate = dto.ValidToDateJalali.ToGregorianDateTime();

		// ─────────────────────────────────────────────────────────────────
		// Check for overlap (excluding current contract)
		// ─────────────────────────────────────────────────────────────────
		var overlap = await _contractRepo.HasOverlapAsync(
			contract.EmployeeId,
			(DateTime)validFromDate!,
			validToDate,
			contract.Id, // exclude current contract
			cancellationToken);

		if (overlap)
			throw new InvalidOperationException("تاریخ قرارداد با قرارداد فعال دیگری تداخل دارد.");

		// ─────────────────────────────────────────────────────────────────
		// Update contract basic info
		// ─────────────────────────────────────────────────────────────────
		contract.UpdateValidFromDate((DateTime)validFromDate);
		contract.UpdateValidToDate(validToDate);

		// ─────────────────────────────────────────────────────────────────
		// Update pay items in transaction
		// ─────────────────────────────────────────────────────────────────
		var result = await _contractRepo.ExecuteInTransactionAsync(async () =>
		{
			// ─────────────────────────────────────────────────────────────
			// Step 1: Build lookup sets for comparison
			// ─────────────────────────────────────────────────────────────

			// Current items in database (both types)
			var currentDbItemIds = contract.PayItems
				.Where(pi => pi.PayItemId.HasValue)
				.Select(pi => pi.Id)
				.ToHashSet();

			var currentSystemCodes = contract.PayItems
				.Where(pi => !string.IsNullOrEmpty(pi.SystemCode))
				.Select(pi => pi.SystemCode!)
				.ToHashSet();

			// Updated items from DTO (not deleted)
			var updatedDbItemIds = dto.PayItems
				.Where(pi => pi.Id.HasValue && !pi.IsDeleted && !pi.IsSystemItem)
				.Select(pi => pi.Id!.Value)
				.ToHashSet();

			var updatedSystemCodes = dto.PayItems
				.Where(pi => !pi.IsDeleted && pi.IsSystemItem)
				.Select(pi => pi.SystemCode!)
				.ToHashSet();

			// ─────────────────────────────────────────────────────────────
			// Step 2: Remove items that are deleted or not in updated list
			// ─────────────────────────────────────────────────────────────

			// Remove database pay items
			var dbItemsToRemove = contract.PayItems
				.Where(pi => pi.PayItemId.HasValue && !updatedDbItemIds.Contains(pi.Id))
				.ToList();

			foreach (var payItem in dbItemsToRemove)
			{
				contract.RemovePayItem(payItem.PayItemId!.Value);
			}

			// Remove system pay items
			var systemItemsToRemove = contract.PayItems
				.Where(pi => !string.IsNullOrEmpty(pi.SystemCode) && !updatedSystemCodes.Contains(pi.SystemCode!))
				.ToList();

			foreach (var payItem in systemItemsToRemove)
			{
				contract.RemoveSystemPayItem(payItem.SystemCode!);
			}

			// ─────────────────────────────────────────────────────────────
			// Step 3: Add or update pay items
			// ─────────────────────────────────────────────────────────────
			foreach (var itemDto in dto.PayItems)
			{
				if (itemDto.IsDeleted)
					continue;

				if (itemDto.IsSystemItem)
				{
					// ─────────────────────────────────────────────────────
					// Handle system pay item
					// ─────────────────────────────────────────────────────
					if (!PayItemConstants.IsCodeExist(itemDto.SystemCode!))
						throw new InvalidOperationException($"عامل سیستمی با کد {itemDto.SystemCode} یافت نشد");

					var existingSystemItem = contract.PayItems
						.FirstOrDefault(pi => pi.SystemCode == itemDto.SystemCode);

					if (existingSystemItem != null)
					{
						// Update existing system item
						existingSystemItem.UpdateValue(itemDto.Value);
					}
					else
					{
						// Add new system item
						contract.AddSystemPayItem(itemDto.SystemCode!, itemDto.Value);
					}
				}
				else
				{
					// ─────────────────────────────────────────────────────
					// Handle database pay item
					// ─────────────────────────────────────────────────────
					if (!itemDto.PayItemId.HasValue)
						throw new ArgumentException("شناسه عامل حقوقی الزامی است");

					if (itemDto.Id.HasValue)
					{
						// Update existing database pay item
						var existing = contract.PayItems.FirstOrDefault(pi => pi.Id == itemDto.Id.Value);
						if (existing != null)
						{
							existing.UpdateValue(itemDto.Value);
						}
					}
					else
					{
						// Add new database pay item
						var payItem = await _payItemService.GetPayItemByIdAsync(
							itemDto.PayItemId.Value,
							cancellationToken);

						if (payItem == null)
							throw new ArgumentException($"عامل حقوقی با شناسه {itemDto.PayItemId} یافت نشد");

						contract.AddPayItem(itemDto.PayItemId.Value, itemDto.Value);
					}
				}
			}

			// ─────────────────────────────────────────────────────────────
			// Step 4: Save all changes
			// ─────────────────────────────────────────────────────────────
			return await _contractRepo.SaveChangesAsync(cancellationToken);

		}, cancellationToken);

		return result;
	}


	/// <summary>
	/// Activate contract
	/// </summary>
	public async Task<bool> ActivateContractAsync(long contractId, CancellationToken cancellationToken = default)
    {
        try
        {
            var contract = await _contractRepo.GetAsync(contractId, cancellationToken);
            if (contract == null)
            {
                return false;
            }

            contract.Activate();
            await _contractRepo.UpdateAsync(contract, cancellationToken);
            await _contractRepo.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating contract {ContractId}", contractId);
            return false;
        }
    }

    /// <summary>
    /// Deactivate contract
    /// </summary>
    public async Task<bool> DeactivateContractAsync(long contractId, CancellationToken cancellationToken = default)
    {
        try
        {
            var contract = await _contractRepo.GetAsync(contractId, cancellationToken);
            if (contract == null)
            {
                return false;
            }

            contract.Deactivate();
            await _contractRepo.UpdateAsync(contract, cancellationToken);
            await _contractRepo.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating contract {ContractId}", contractId);
            return false;
        }
    }

    /// <summary>
    /// Delete contract (soft delete)
    /// </summary>
    public async Task<bool> DeleteContractAsync(long contractId, CancellationToken cancellationToken = default)
    {
        try
        {
            var contract = await _contractRepo.GetAsync(contractId, cancellationToken);
            if (contract == null)
            {
                return false;
            }

            // Todo: check for existing payslips

            // Check if contract has any pay slips
            // var hasPaySlips = await _paySlipRepo.AnyAsync(
            //    ps => ps.ContractId == contractId,
            //   cancellationToken);

            // if (hasPaySlips)
            // {
            //    throw new InvalidOperationException("Cannot delete contract with existing pay slips");
            // }

            await _contractRepo.DeleteAsync(contract, cancellationToken);
            await _contractRepo.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting contract {ContractId}", contractId);
            throw;
        }
    }

    /// <summary>
    /// Maps Contract entity to ContractDto
    /// Handles both database pay items and system pay items
    /// </summary>
    private async Task<ContractDto> MapToDto(Contract contract, CancellationToken ct = default)
    {
        // Get all database pay item IDs (exclude system items)
        var payItemIds = contract.PayItems
            .Where(pi => pi.IsDatabaseItem)
            .Select(pi => pi.PayItemId!.Value)
            .Distinct()
            .ToList();

        // Fetch database pay items
        Dictionary<long, string?> payItemDict = new();
        if (payItemIds.Any())
        {
            var payItems = await _payItemRepo.GetAllByConditionAsync(
                p => payItemIds.Contains(p.Id),
                ct
            );
            payItemDict = payItems.ToDictionary(p => p.Id, p => p.Name);
        }

        return new ContractDto
        {
            Id = contract.Id,
            EmployeeId = contract.EmployeeId,
            WorkshopId = contract.WorkshopId,
            ValidFromDate = contract.ValidFromDate,
            ValidToDate = contract.ValidToDate,
            IsActive = contract.IsActive,
            Status = contract.Status,
            PayItems = contract.PayItems.Select(pi => new ContractPayItemDto
            {
                PayItemId = pi.PayItemId,
                SystemCode = pi.SystemCode,
                IsSystemItem = pi.IsSystemItem,

                // Get name from database or system constants
                PayItemName = pi.IsSystemItem
                    ? PayItemConstants.GetName(pi.SystemCode!)
                    : payItemDict.GetValueOrDefault(pi.PayItemId!.Value, "نامشخص"),

                Value = pi.Value
            }).ToList()
        };
    }

}
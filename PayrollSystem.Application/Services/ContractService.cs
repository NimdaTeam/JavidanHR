using HrSystem.Application.Interfaces;
using Microsoft.Extensions.Logging;
using PayrollSystem.Application.DTOs;
using PayrollSystem.Application.Interfaces;
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

    public ContractService(IContractRepository contractRepo, IWorkshopRepository workshopRepo, IPayItemRepository payItemRepo, ILogger<ContractService> logger, IEmployeeService employeeService)
    {
        _contractRepo = contractRepo;
        _workshopRepo = workshopRepo;
        _payItemRepo = payItemRepo;
        _logger = logger;
        _employeeService = employeeService;
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
        // Check workshop exists
        var workshop = await _workshopRepo.GetAsync(dto.WorkshopId, cancellationToken);
        if (workshop == null) throw new ArgumentException("Workshop not found");

        // Check overlap
        var overlap = await _contractRepo.HasOverlapAsync(dto.EmployeeId, dto.ValidFromDate, dto.ValidToDate, null, cancellationToken);
        if (overlap) throw new InvalidOperationException("Contract dates overlap with existing contract.");

        var contract = new Contract(dto.EmployeeId, dto.WorkshopId, dto.ValidFromDate, dto.ValidToDate);

        var result = await _contractRepo.ExecuteInTransactionAsync(async () =>
        {
            await _contractRepo.AddAsync(contract, cancellationToken);
            return await _contractRepo.SaveChangesAsync(cancellationToken);
        }, cancellationToken);

        if (!result)
            return null;

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

    /// <summary>
    /// Update contract basic information
    /// </summary>
    public async Task<bool> UpdateContractAsync(
        UpdateContractDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var contract = await _contractRepo.GetAsync(dto.Id, cancellationToken);
            if (contract == null)
            {
                return false;
            }

            // Update dates
            contract.UpdateValidFromDate(dto.ValidFromDate);

            if (dto.ValidToDate.HasValue)
            {
                contract.UpdateValidToDate(dto.ValidToDate);
            }

            await _contractRepo.UpdateAsync(contract, cancellationToken);
            await _contractRepo.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contract {ContractId}", dto.Id);
            return false;
        }
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

    private async Task<ContractDto> MapToDto(Contract contract, CancellationToken ct)
    {
        var payItemIds = contract.PayItems.Select(pi => pi.PayItemId).Distinct().ToList();
        var payItems = await _payItemRepo.GetAllByConditionAsync(p => payItemIds.Contains(p.Id), ct);
        var payItemDict = payItems.ToDictionary(p => p.Id, p => p.Name);

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
                PayItemName = payItemDict.GetValueOrDefault(pi.PayItemId, "Unknown"),
                Value = pi.Value
            }).ToList()
        };
    }
}
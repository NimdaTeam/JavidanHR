// IContractAppService.cs
using PayrollSystem.Application.DTOs;
using PayrollSystem.Domain.Entities.Contract;

namespace PayrollSystem.Application.Interfaces
{
    /// <summary>
    /// Application service for contract operations.
    /// </summary>
    public interface IContractService
    {
        Task<List<ContractDto>> GetAllContractsWithDtoAsync(CancellationToken cancellationToken = default);
        Task<ContractDto?> CreateContractAsync(CreateContractDto dto, CancellationToken cancellationToken = default);
        Task<ContractDto?> GetContractByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<ContractDto?> GetActiveContractByEmployeeAsync(long employeeId, DateTime? atDate = null, CancellationToken cancellationToken = default);
        Task<List<ContractDto>> GetContractsByEmployeeAsync(long employeeId, CancellationToken cancellationToken = default);
        Task<bool> AssignPayItemToContractAsync(AssignPayItemToContractDto dto, CancellationToken cancellationToken = default);
        Task<bool> RemovePayItemFromContractAsync(long contractId, long payItemId, CancellationToken cancellationToken = default);
        Task<bool> TerminateContractAsync(long contractId, DateTime terminationDate, CancellationToken cancellationToken = default);
        Task<List<Contract>> GetAllContractsAsync(CancellationToken cancellationToken = default);

        Task<bool> UpdateContractAsync(UpdateContractDto dto, CancellationToken cancellationToken = default);
        Task<bool> ActivateContractAsync(long contractId, CancellationToken cancellationToken = default);
        Task<bool> DeactivateContractAsync(long contractId, CancellationToken cancellationToken = default);
        Task<bool> DeleteContractAsync(long contractId, CancellationToken cancellationToken = default);
    }
}
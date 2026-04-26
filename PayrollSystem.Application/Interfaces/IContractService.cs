// IContractAppService.cs
using PayrollSystem.Application.DTOs;

namespace PayrollSystem.Application.Interfaces
{
    /// <summary>
    /// Application service for contract operations.
    /// </summary>
    public interface IContractService
    {
        Task<ContractDto> CreateContractAsync(CreateContractDto dto, CancellationToken cancellationToken = default);
        Task<ContractDto?> GetContractByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<ContractDto?> GetActiveContractByEmployeeAsync(long employeeId, DateTime? atDate = null, CancellationToken cancellationToken = default);
        Task<List<ContractDto>> GetContractsByEmployeeAsync(long employeeId, CancellationToken cancellationToken = default);
        Task<bool> AssignPayItemToContractAsync(AssignPayItemToContractDto dto, CancellationToken cancellationToken = default);
        Task<bool> RemovePayItemFromContractAsync(long contractId, long payItemId, CancellationToken cancellationToken = default);
        Task<bool> TerminateContractAsync(long contractId, DateTime terminationDate, CancellationToken cancellationToken = default);
    }
}
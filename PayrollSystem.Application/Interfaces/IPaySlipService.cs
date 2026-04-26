// IPaySlipAppService.cs
using PayrollSystem.Application.DTOs;

namespace PayrollSystem.Application.Interfaces
{
    /// <summary>
    /// Application service for pay slip generation and management.
    /// </summary>
    public interface IPaySlipService
    {
        Task<PaySlipDto> GeneratePaySlipAsync(GeneratePaySlipCommand command, CancellationToken cancellationToken = default);
        Task<PaySlipDto?> GetPaySlipAsync(long id, CancellationToken cancellationToken = default);
        Task<List<PaySlipDto>> GetPaySlipsByEmployeeAsync(long employeeId, CancellationToken cancellationToken = default);
        Task<bool> OverridePaySlipItemAsync(OverridePaySlipItemCommand command, CancellationToken cancellationToken = default);
        Task<bool> FinalizePaySlipAsync(long paySlipId, CancellationToken cancellationToken = default);
        Task<PaySlipDto> RecalculatePaySlipAsync(long paySlipId, CancellationToken cancellationToken = default);
    }
}
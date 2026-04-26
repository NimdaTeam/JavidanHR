// ICalculationLogAppService.cs
using PayrollSystem.Application.DTOs;

namespace PayrollSystem.Application.Interfaces
{
    /// <summary>
    /// Application service for reading calculation logs.
    /// </summary>
    public interface ICalculationLogService
    {
        Task<List<CalculationLogDto>> GetLogsByPaySlipAsync(long paySlipId, CancellationToken cancellationToken = default);
        Task<List<CalculationLogDto>> GetLogsByPayItemAsync(long payItemId, CancellationToken cancellationToken = default);
        Task<bool> DeleteLogsForPaySlipAsync(long paySlipId, CancellationToken cancellationToken = default);
    }
}
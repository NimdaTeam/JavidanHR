using PayrollSystem.Application.DTOs;
using PayrollSystem.Application.Interfaces;
using PayrollSystem.Domain.Interfaces.CalculationLog;

namespace PayrollSystem.Application.Services;

public class CalculationLogService : ICalculationLogService
{
    private readonly ICalculationLogRepository _logRepo;
    public CalculationLogService(ICalculationLogRepository logRepo) => _logRepo = logRepo;

    public async Task<List<CalculationLogDto>> GetLogsByPaySlipAsync(long paySlipId, CancellationToken cancellationToken = default)
    {
        var logs = await _logRepo.GetLogsByPaySlipAsync(paySlipId, cancellationToken);
        return logs.Select(l => new CalculationLogDto
        {
            Id = l.Id,
            PaySlipId = l.PaySlipId,
            PayItemId = l.PayItemId,
            InputValuesJson = l.InputValuesJson,
            FormulaUsed = l.FormulaUsed,
            ResultValue = l.ResultValue,
            CalculatedAt = l.CalculatedAt
        }).ToList();
    }

    public async Task<List<CalculationLogDto>> GetLogsByPayItemAsync(long payItemId, CancellationToken cancellationToken = default)
    {
        var logs = await _logRepo.GetLogsByPayItemAsync(payItemId, cancellationToken);
        return logs.Select(l => new CalculationLogDto
        {
            Id = l.Id,
            PaySlipId = l.PaySlipId,
            PayItemId = l.PayItemId,
            InputValuesJson = l.InputValuesJson,
            FormulaUsed = l.FormulaUsed,
            ResultValue = l.ResultValue,
            CalculatedAt = l.CalculatedAt
        }).ToList();
    }

    public async Task<bool> DeleteLogsForPaySlipAsync(long paySlipId, CancellationToken cancellationToken = default)
    {
        await _logRepo.DeleteByPaySlipIdAsync(paySlipId, cancellationToken);
        await _logRepo.SaveChangesAsync(cancellationToken);
        return true;
    }
}
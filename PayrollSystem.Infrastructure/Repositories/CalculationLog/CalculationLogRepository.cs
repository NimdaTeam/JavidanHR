using _0_Framework.GenericRepositoy.Service;
using Microsoft.EntityFrameworkCore;
using PayrollSystem.Domain.Interfaces.CalculationLog;
using PayrollSystem.Infrastructure.Persistence.Context;

namespace PayrollSystem.Infrastructure.Repositories.CalculationLog
{
    public class CalculationLogRepository : RepositoryService<long, Domain.Entities.CalculationLog.CalculationLog>, ICalculationLogRepository
    {
        private readonly PayrollSystemContext _context;

        public CalculationLogRepository(PayrollSystemContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Domain.Entities.CalculationLog.CalculationLog>> GetLogsByPaySlipAsync(long paySlipId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Domain.Entities.CalculationLog.CalculationLog>()
                .AsNoTracking()
                .Where(log => log.PaySlipId == paySlipId)
                .OrderBy(log => log.CalculatedAt)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<Domain.Entities.CalculationLog.CalculationLog>> GetLogsByPayItemAsync(long payItemId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Domain.Entities.CalculationLog.CalculationLog>()
                .AsNoTracking()
                .Where(log => log.PayItemId == payItemId)
                .OrderByDescending(log => log.CalculatedAt)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task DeleteByPaySlipIdAsync(long paySlipId, CancellationToken cancellationToken = default)
        {
            var logs = await _context.Set<Domain.Entities.CalculationLog.CalculationLog>()
                .Where(log => log.PaySlipId == paySlipId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (logs.Any())
            {
                _context.Set<Domain.Entities.CalculationLog.CalculationLog>().RemoveRange(logs);
                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
using _0_Framework.GenericRepositoy.Service;
using Microsoft.EntityFrameworkCore;
using PayrollSystem.Domain.Interfaces.PaySlip;
using PayrollSystem.Infrastructure.Persistence.Context;

namespace PayrollSystem.Infrastructure.Repositories.PaySlip
{
    public class PaySlipRepository : RepositoryService<long, Domain.Entities.PaySlip.PaySlip>, IPaySlipRepository
    {
        private readonly PayrollSystemContext _context;

        public PaySlipRepository(PayrollSystemContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Domain.Entities.PaySlip.PaySlip?> GetByEmployeeAndDateAsync(long employeeId, int year, int month, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Domain.Entities.PaySlip.PaySlip>()
                .AsNoTracking()
                .FirstOrDefaultAsync(ps => ps.EmployeeId == employeeId && ps.Year == year && ps.Month == month, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<Domain.Entities.PaySlip.PaySlip>> GetPaySlipsByContractAsync(long contractId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Domain.Entities.PaySlip.PaySlip>()
                .AsNoTracking()
                .Where(ps => ps.ContractId == contractId)
                .OrderByDescending(ps => ps.Year).ThenByDescending(ps => ps.Month)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<bool> IsPaySlipExistForPeriodAsync(long contractId, int year, int month, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Domain.Entities.PaySlip.PaySlip>()
                .AnyAsync(ps => ps.ContractId == contractId && ps.Year == year && ps.Month == month, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<Domain.Entities.PaySlip.PaySlip?> GetWithItemsAsync(long paySlipId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Domain.Entities.PaySlip.PaySlip>()
                .Include(ps => ps.Items)
                .FirstOrDefaultAsync(ps => ps.Id == paySlipId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<Domain.Entities.PaySlip.PaySlip>> GetPaySlipsByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            // از ترکیب Year و Month برای فیلتر بازه استفاده می‌کنیم
            return await _context.Set<Domain.Entities.PaySlip.PaySlip>()
                .AsNoTracking()
                .Where(ps => ps.IssueDate >= fromDate && ps.IssueDate <= toDate)
                .OrderBy(ps => ps.IssueDate)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
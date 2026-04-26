using _0_Framework.GenericRepositoy.Service;
using Microsoft.EntityFrameworkCore;
using PayrollSystem.Domain.Interfaces.Contract;
using PayrollSystem.Infrastructure.Persistence.Context;

namespace PayrollSystem.Infrastructure.Repositories.Contract
{
    public class ContractRepository : RepositoryService<long, Domain.Entities.Contract.Contract>, IContractRepository
    {
        private readonly PayrollSystemContext _context;

        public ContractRepository(PayrollSystemContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Domain.Entities.Contract.Contract?> GetActiveContractByEmployeeAsync(long employeeId, DateTime? atDate = null, CancellationToken cancellationToken = default)
        {
            var targetDate = atDate ?? DateTime.Now;
            return await _context.Set<Domain.Entities.Contract.Contract>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.EmployeeId == employeeId &&
                                          c.IsActive &&
                                          c.ValidFromDate <= targetDate &&
                                          (!c.ValidToDate.HasValue || c.ValidToDate >= targetDate),
                                          cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<Domain.Entities.Contract.Contract>> GetContractsByEmployeeAsync(long employeeId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Domain.Entities.Contract.Contract>()
                .AsNoTracking()
                .Where(c => c.EmployeeId == employeeId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<bool> HasOverlapAsync(long employeeId, DateTime fromDate, DateTime? toDate, long? excludeContractId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Set<Domain.Entities.Contract.Contract>()
                .Where(c => c.EmployeeId == employeeId &&
                            c.ValidFromDate <= (toDate ?? DateTime.MaxValue) &&
                            (c.ValidToDate == null || c.ValidToDate >= fromDate));

            if (excludeContractId.HasValue)
                query = query.Where(c => c.Id != excludeContractId.Value);

            return await query.AnyAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<Domain.Entities.Contract.Contract?> GetWithPayItemsAsync(long contractId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Domain.Entities.Contract.Contract>()
                .Include(c => c.PayItems)
                .FirstOrDefaultAsync(c => c.Id == contractId, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
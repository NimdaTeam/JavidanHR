using _0_Framework.GenericRepositoy.Service;
using Microsoft.EntityFrameworkCore;
using PayrollSystem.Domain.Interfaces.Workshop;
using PayrollSystem.Infrastructure.Persistence.Context;

namespace PayrollSystem.Infrastructure.Repositories.Workshop
{
    public class WorkshopRepository : RepositoryService<long,Domain.Entities.Workshop.Workshop>, IWorkshopRepository
    {
        private readonly PayrollSystemContext _context;

        public WorkshopRepository(PayrollSystemContext context) : base(context)
        {
            _context = context;
        }


        public async Task<Domain.Entities.Workshop.Workshop?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Domain.Entities.Workshop.Workshop>()
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Code == code, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<bool> IsCodeUniqueAsync(string code, long? excludeId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Set<Domain.Entities.Workshop.Workshop>().AsNoTracking().Where(w => w.Code == code);
            if (excludeId.HasValue)
                query = query.Where(w => w.Id != excludeId.Value);
            return !await query.AnyAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
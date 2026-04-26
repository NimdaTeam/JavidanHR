using _0_Framework.GenericRepositoy.Interface;
using PayrollSystem.Domain.Entities.Workshop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayrollSystem.Domain.Interfaces.Workshop
{
    public interface IWorkshopRepository : IRepository<long, Entities.Workshop.Workshop>
    {
        Task<Entities.Workshop.Workshop?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
        Task<bool> IsCodeUniqueAsync(string code, long? excludeId = null, CancellationToken cancellationToken = default);
    }
}

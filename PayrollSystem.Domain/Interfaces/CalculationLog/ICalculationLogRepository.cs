using _0_Framework.GenericRepositoy.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayrollSystem.Domain.Interfaces.CalculationLog
{
    public interface ICalculationLogRepository : IRepository<long, Entities.CalculationLog.CalculationLog>
    {
        Task<List<Entities.CalculationLog.CalculationLog>> GetLogsByPaySlipAsync(long paySlipId, CancellationToken cancellationToken = default);
        Task<List<Entities.CalculationLog.CalculationLog>> GetLogsByPayItemAsync(long payItemId, CancellationToken cancellationToken = default);

        /// <summary>
        /// حذف لاگ‌های یک فیش (برای بازمحاسبه)
        /// </summary>
        Task DeleteByPaySlipIdAsync(long paySlipId, CancellationToken cancellationToken = default);
    }
}

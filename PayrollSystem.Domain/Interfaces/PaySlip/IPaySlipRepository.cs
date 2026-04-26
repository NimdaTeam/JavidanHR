using _0_Framework.GenericRepositoy.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayrollSystem.Domain.Interfaces.PaySlip
{
    public interface IPaySlipRepository : IRepository<long, Entities.PaySlip.PaySlip>
    {
        Task<Entities.PaySlip.PaySlip?> GetByEmployeeAndDateAsync(long employeeId, int year, int month, CancellationToken cancellationToken = default);
        Task<List<Entities.PaySlip.PaySlip>> GetPaySlipsByContractAsync(long contractId, CancellationToken cancellationToken = default);
        Task<bool> IsPaySlipExistForPeriodAsync(long contractId, int year, int month, CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت فیش به همراه آیتم‌های آن (برای محاسبات)
        /// </summary>
        Task<Entities.PaySlip.PaySlip?> GetWithItemsAsync(long paySlipId, CancellationToken cancellationToken = default);

        Task<List<Entities.PaySlip.PaySlip>> GetPaySlipsByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    }
}

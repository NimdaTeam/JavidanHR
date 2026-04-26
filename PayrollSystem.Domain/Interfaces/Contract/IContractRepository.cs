using _0_Framework.GenericRepositoy.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayrollSystem.Domain.Interfaces.Contract
{
    public interface IContractRepository : IRepository<long, Entities.Contract.Contract>
    {
        /// <summary>
        /// دریافت قرارداد فعال کارمند در تاریخ مشخص (پیش‌فرض زمان حال)
        /// </summary>
        Task<Entities.Contract.Contract?> GetActiveContractByEmployeeAsync(long employeeId, DateTime? atDate = null, CancellationToken cancellationToken = default);

        Task<List<Entities.Contract.Contract>> GetContractsByEmployeeAsync(long employeeId, CancellationToken cancellationToken = default);

        /// <summary>
        /// بررسی تداخل تاریخ قرارداد با سایر قراردادهای کارمند
        /// </summary>
        Task<bool> HasOverlapAsync(long employeeId, DateTime fromDate, DateTime? toDate, long? excludeContractId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت قرارداد به همراه آیتم‌های آن
        /// </summary>
        Task<Entities.Contract.Contract?> GetWithPayItemsAsync(long contractId, CancellationToken cancellationToken = default);
    }
}

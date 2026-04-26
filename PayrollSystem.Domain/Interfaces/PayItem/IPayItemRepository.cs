using _0_Framework.GenericRepositoy.Interface;
using PayrollSystem.Domain.Entities.PayItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayrollSystem.Domain.Interfaces.PayItem
{
    public interface IPayItemRepository : IRepository<long, Entities.PayItem.PayItem>
    {
        Task<Entities.PayItem.PayItem?> GetBySystemCodeAsync(string systemCode, CancellationToken cancellationToken = default);
        Task<List<Entities.PayItem.PayItem>> GetActivePayItemsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت فرمول فعال برای یک آیتم در تاریخ مشخص
        /// </summary>
        Task<PayItemFormula?> GetActiveFormulaAsync(long payItemId, DateTime atDate, CancellationToken cancellationToken = default);

        Task<List<Entities.PayItem.PayItem>> GetPayItemsByTypeAsync(Entities.PayItem.PayItem.PayItemType type, CancellationToken cancellationToken = default);

        Task RefreshCacheAsync();

        Task<bool> IsValidCodeAsync(string code);

        Task<PayItemFormula?> GetActiveFormulaAsync(long payItemId, CancellationToken cancellationToken =default);
    }
}

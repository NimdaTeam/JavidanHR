// IPayItemAppService.cs

using PayrollSystem.Application.DTOs;
using PayrollSystem.Domain.Common;
using PayrollSystem.Domain.Entities.PayItem;
using System.Linq.Expressions;

namespace PayrollSystem.Application.Interfaces
{
    /// <summary>
    /// Application service for pay item (earning/deduction) management.
    /// </summary>
    public interface IPayItemService
    {
        Task<PayItemConstants.PayItemDto?> CreatePayItemAsync(CreatePayItemDto dto, CancellationToken cancellationToken = default);
        Task<PayItemConstants.PayItemDto?> UpdatePayItemAsync(UpdatePayItemDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeletePayItemAsync(long id, CancellationToken cancellationToken = default);
        Task<PayItemConstants.PayItemDto?> GetPayItemByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<PayItemConstants.PayItemDto?> GetPayItemBySystemCodeAsync(string systemCode, CancellationToken cancellationToken = default);
        Task<List<PayItemConstants.PayItemDto>> GetActivePayItemsAsync(CancellationToken cancellationToken = default);
        Task<List<PayItemConstants.PayItemDto>> GetPayItemsByTypeAsync(string type, CancellationToken cancellationToken = default);
        Task<List<PayItemConstants.PayItemDto>> GetAllPayItemsAsync(CancellationToken cancellationToken = default);
        Task<List<PayItemConstants.PayItemDto>> GetAllPayItemsByConditionAsync(Expression<Func<PayItem, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a new versioned formula to a pay item.
        /// </summary>
        Task<bool> AddFormulaToPayItemAsync(AddPayItemFormulaDto dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Activates a specific formula version and deactivates others.
        /// </summary>
        Task<bool> ActivateFormulaAsync(long formulaId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Evaluates a formula using the given context values.
        /// </summary>
        Task<decimal> EvaluateFormulaAsync(long payItemId, string formula, Dictionary<string, decimal> contextValues, CancellationToken cancellationToken = default);


        Task<List<PayItemConstants.PayItemDto>> GetAllValidCodesAsync();
        Task<bool> IsValidCodeAsync(string code);
        Task<string?> GetNameAsync(string code);
        Task<List<PayItemConstants.PayItemDto>> GetAllCustomCodesAsync();
        Task RefreshCacheAsync();

        Task<PayItemConstants.PayItemFormulaDto?> GetActiveFormulaAsync(long payItemId, DateTime atDate,
            CancellationToken cancellationToken = default);

        Task<PayItemConstants.PayItemFormulaDto?> GetActiveFormulaAsync(long payItemId, CancellationToken cancellationToken = default);

        // ── عوامل سیستمی ─────────────────────────────────────────────
        /// <summary>
        /// ویرایش محدود یک عامل سیستمی (فقط IsInsured، IsTaxable، IsActive، SortOrder و فرمول)
        /// اگر override در DB وجود نداشته باشد، یک رکورد جدید با IsCustom=false ایجاد می‌کند.
        /// </summary>
        Task<PayItemConstants.PayItemDto?> UpdateSystemPayItemAsync(UpdateSystemPayItemDto dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// بازگشت فرمول یک عامل سیستمی به مقدار پیش‌فرض از PayItemConstants
        /// </summary>
        Task ResetSystemPayItemFormulaAsync(string systemCode, CancellationToken cancellationToken = default);

        /// <summary>
        /// آیا این کد سیستمی یک عامل سیستمی است؟
        /// </summary>
        bool IsSystemItem(string systemCode);

        /// <summary>
        /// فرمول پیش‌فرض یک عامل سیستمی (از PayItemConstants)
        /// </summary>
        string? GetDefaultFormula(string systemCode);
    }
}
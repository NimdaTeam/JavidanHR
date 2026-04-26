using PayrollSystem.Application.DTOs;
using PayrollSystem.Application.Interfaces;
using PayrollSystem.Application.Utilities;
using PayrollSystem.Domain.Common;
using PayrollSystem.Domain.Entities.PayItem;
using PayrollSystem.Domain.Interfaces.PayItem;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using _0_Framework.Utilities.Security;

namespace PayrollSystem.Application.Services
{
    /// <summary>
    /// Application service for managing pay items (earnings/deductions) and their formulas.
    /// </summary>
    public class PayItemService : IPayItemService
    {
        private readonly IPayItemRepository _payItemRepository;

        public PayItemService(IPayItemRepository payItemRepository)
        {
            _payItemRepository = payItemRepository ?? throw new ArgumentNullException(nameof(payItemRepository));
        }

        /// <inheritdoc />
        public async Task<PayItemConstants.PayItemDto?> CreatePayItemAsync(CreatePayItemDto dto, CancellationToken cancellationToken = default)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("نام عامل حقوقی الزامی است");
            if (string.IsNullOrWhiteSpace(dto.SystemCode))
                throw new ArgumentException("کد سیستم الزامی است");

            // Check uniqueness of SystemCode
            await RefreshCacheAsync();
            if (PayItemConstants.IsCodeExist(dto.SystemCode.SanitizeString()))
                throw new InvalidOperationException($"کد سیستمی '{dto.SystemCode}' قبلاً ثبت شده است");

            var payItem = new PayItem(
                name: dto.Name.SanitizeString(),
                systemCode: dto.SystemCode.SanitizeString(),
                type: dto.Type,
                dataType: dto.DataType,
                isInsured: dto.IsInsured,
                isTaxable: dto.IsTaxable,
                isEditable: dto.IsEditable,
                isActive: dto.IsActive,
                sortOrder: dto.SortOrder
            );

            var result = await _payItemRepository.ExecuteInTransactionAsync(async () =>
            {
                await _payItemRepository.AddAsync(payItem, cancellationToken);
                return await _payItemRepository.SaveChangesAsync(cancellationToken);
            }, cancellationToken);


            if (!result)
                throw new InvalidOperationException("خطا در ساخت عامل حقوقی");


            if (dto.DataType == PayItem.PayItemDataType.Formula
                && !string.IsNullOrWhiteSpace(dto.Formula)
                && dto.FormulaValidFromDate.HasValue)
            {
                payItem.AddFormula(dto.Formula, dto.FormulaValidFromDate.Value);

                var formulaResult = await _payItemRepository.ExecuteInTransactionAsync(async () =>
                {
                    await _payItemRepository.UpdateAsync(payItem, cancellationToken);
                    return await _payItemRepository.SaveChangesAsync(cancellationToken);
                }, cancellationToken);

                if (!formulaResult)
                    throw new InvalidOperationException("خطا در ذخیره سازی فرمول");
            }

            await RefreshCacheAsync();

            return MapToDto(payItem);
        }

        /// <inheritdoc />
        public async Task<PayItemConstants.PayItemDto?> UpdatePayItemAsync(UpdatePayItemDto dto, CancellationToken cancellationToken = default)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("نام عامل حقوقی الزامی است");
            if (string.IsNullOrWhiteSpace(dto.SystemCode))
                throw new ArgumentException("کد سیستم الزامی است");


            var payItem = await _payItemRepository.GetWithIncludesAsync(
                dto.Id!,
                cancellationToken,
                p => p.Formulas
                );
            if (payItem == null)
                return null;

            if (payItem.SystemCode != dto.SystemCode)
            {
                // Check uniqueness of SystemCode
                await RefreshCacheAsync();
                if (!PayItemConstants.IsCodeExist(dto.SystemCode.SanitizeString(), payItem.Id))
                    throw new InvalidOperationException($"کد سیستمی '{dto.SystemCode}' قبلاً ثبت شده است");
            }


            // Update properties
            payItem.UpdateName(dto.Name.SanitizeString());
            payItem.UpdateSystemCode(dto.SystemCode.SanitizeString());
            payItem.UpdateType(dto.Type);
            payItem.UpdateDataType(dto.DataType);
            payItem.UpdateIsInsured(dto.IsInsured);
            payItem.UpdateIsTaxable(dto.IsTaxable);
            payItem.UpdateIsEditable(dto.IsEditable);
            payItem.UpdateIsActive(dto.IsActive);
            payItem.UpdateSortOrder(dto.SortOrder);

            // اگر DataType فرمول بود و فرمول وارد شده، اضافه کن
            if (dto.DataType == PayItem.PayItemDataType.Formula
                && !string.IsNullOrWhiteSpace(dto.Formula)
                && dto.FormulaValidFromDate.HasValue)
            {
                payItem.ReplaceFormula(dto.Formula, DateTime.Now);
            }

            var result = await _payItemRepository.ExecuteInTransactionAsync(async () =>
            {
                //await _payItemRepository.UpdateAsync(payItem, cancellationToken);
                return await _payItemRepository.SaveChangesAsync(cancellationToken);
            }, cancellationToken);

            if (!result)
                throw new InvalidOperationException("خطا در ویرایش عامل حقوقی");

            await RefreshCacheAsync();

            return MapToDto(payItem);
        }

        /// <inheritdoc />
        public async Task<bool> DeletePayItemAsync(long id, CancellationToken cancellationToken = default)
        {
            var success = await _payItemRepository.DeleteAsync(id, cancellationToken);
            if (success)
                await _payItemRepository.SaveChangesAsync(cancellationToken);
            return success;
        }

        /// <inheritdoc />
        public async Task<PayItemConstants.PayItemDto?> GetPayItemByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            var payItem = await _payItemRepository.GetWithIncludesAsync(id, cancellationToken);
            return payItem != null ? MapToDto(payItem) : null;
        }

        /// <inheritdoc cref="" />
        public async Task<PayItemConstants.PayItemDto?> GetPayItemBySystemCodeAsync(string systemCode, CancellationToken cancellationToken = default)
        {
            // اول DB را بررسی کن (override)
            var entity = await _payItemRepository.GetBySystemCodeAsync(systemCode,cancellationToken);
            if (entity == null)
                return PayItemConstants.GetAllValidItems()
                    .FirstOrDefault(x => x.SystemCode.Equals(systemCode, StringComparison.OrdinalIgnoreCase));

            var dto = MapToDto(entity);
            var def = GetDefaultItem(systemCode);
            return def != null ? MergeWithDefault(dto, def) : dto;

        }

        /// <inheritdoc />
        public async Task<List<PayItemConstants.PayItemDto>> GetActivePayItemsAsync(CancellationToken cancellationToken = default)
        {
            var items = await _payItemRepository.GetActivePayItemsAsync(cancellationToken);
            return items.Select(MapToDto).ToList();
        }

        /// <inheritdoc />
        public async Task<List<PayItemConstants.PayItemDto>> GetPayItemsByTypeAsync(string type, CancellationToken cancellationToken = default)
        {
            if (!Enum.TryParse<PayItem.PayItemType>(type, true, out var payItemType))
                throw new ArgumentException($"Invalid type: {type}. Allowed: Earning, Deduction, Info.");

            var items = await _payItemRepository.GetPayItemsByTypeAsync(payItemType, cancellationToken);
            return items.Select(MapToDto).ToList();
        }

        public async Task<List<PayItemConstants.PayItemDto>> GetAllPayItemsAsync(CancellationToken cancellationToken = default)
        {
            var items = await _payItemRepository.GetAllAsync(cancellationToken);
            return items.Select(MapToDto).ToList();
        }

        public async Task<List<PayItemConstants.PayItemDto>> GetAllPayItemsByConditionAsync(Expression<Func<PayItem, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var items = await _payItemRepository.GetAllByConditionAsync(predicate, cancellationToken);
            return items.Select(MapToDto).ToList();
        }

        /// <inheritdoc />
        public async Task<bool> AddFormulaToPayItemAsync(AddPayItemFormulaDto dto, CancellationToken cancellationToken = default)
        {
            var payItem = await _payItemRepository.GetAsync(dto.PayItemId, cancellationToken);
            if (payItem == null)
                return false;

            if (payItem.DataType != PayItem.PayItemDataType.Formula)
                throw new InvalidOperationException("Formula can only be added to pay items with DataType = Formula.");

            payItem.AddFormula(dto.Formula, dto.ValidFromDate, dto.ValidToDate);
            await _payItemRepository.UpdateAsync(payItem, cancellationToken);
            await _payItemRepository.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> ActivateFormulaAsync(long formulaId, CancellationToken cancellationToken = default)
        {
            // Find the pay item that contains this formula
            var allPayItems = await _payItemRepository.GetAllWithIncludesAsync(
                filter: null,
                orderBy: null,
                cancellationToken: cancellationToken,
                includes: p => p.Formulas
            );
            var payItem = allPayItems.FirstOrDefault(p => p.Formulas.Any(f => f.Id == formulaId));
            if (payItem == null)
                return false;

            payItem.ActivateFormula(formulaId);
            await _payItemRepository.UpdateAsync(payItem, cancellationToken);
            await _payItemRepository.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <inheritdoc />
        public async Task<decimal> EvaluateFormulaAsync(long payItemId, string formula, Dictionary<string, decimal> contextValues, CancellationToken cancellationToken = default)
        {
            // This method does not require repository access; it's a pure evaluation.
            // It is used by PaySlipAppService and can be called directly.
            try
            {
                var parameter = Expression.Parameter(typeof(IDictionary<string, object>), "variables");
                var transformedFormula = formula;
                foreach (var varName in contextValues.Keys)
                {
                    transformedFormula = Regex.Replace(transformedFormula,
                        $@"\b{Regex.Escape(varName)}\b",
                        $"variables[\"{varName}\"]",
                        RegexOptions.IgnoreCase);
                }

                var parsedExpression = DynamicExpressionParser.ParseLambda(
                    new[] { parameter },
                    typeof(decimal),
                    transformedFormula);

                var compiled = parsedExpression.Compile();
                var dict = contextValues.ToDictionary(k => k.Key, v => (object)v.Value);
                var result = compiled.DynamicInvoke(dict);

                return result != null ? Convert.ToDecimal(result) : 0;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Formula evaluation failed: {ex.Message}", ex);
            }
        }


        public async Task<List<PayItemConstants.PayItemDto>> GetAllValidCodesAsync()
        {
            // همه رکوردهای DB (هم custom هم override های سیستمی)
            var dbItems = await _payItemRepository.GetAllWithIncludesAsync(null,null,default,x=>x.Formulas);
            var dbByCode = dbItems.ToDictionary(x => x.SystemCode, StringComparer.OrdinalIgnoreCase);

            var result = new List<PayItemConstants.PayItemDto>();

            // ۱. عوامل کارکردی — فقط از constants (قابل override نیستند)
            result.AddRange(PayItemConstants.AttendanceItems);

            // ۲. عوامل سیستمی — اگر override در DB دارند، از DB بخوان
            foreach (var def in PayItemConstants.DefaultPayItems)
            {
                if (dbByCode.TryGetValue(def.SystemCode, out var dbItem))
                    result.Add(MergeWithDefault(MapToDto(dbItem), def));
                else
                    result.Add(def);
            }

            // ۳. عوامل سفارشی خالص
            var customInDb = dbItems.Where(x => x.IsCustom).ToList();
            result.AddRange(customInDb.Select(MapToDto));

            return result.OrderBy(x => x.SortOrder).ToList();
        }


        public async Task<bool> IsValidCodeAsync(string code)
        {
            return await _payItemRepository.IsValidCodeAsync(code.SanitizeString());
        }

        public async Task<string?> GetNameAsync(string code)
        {
            var item = await _payItemRepository.GetBySystemCodeAsync(code.SanitizeString());
            return item?.Name ?? "-";
        }

        public async Task<List<PayItemConstants.PayItemDto>> GetAllCustomCodesAsync()
        {
            var allCodes = await GetAllValidCodesAsync();
            return allCodes.Where(x => x.IsCustom).ToList();
        }

        public async Task RefreshCacheAsync()
        {
            await _payItemRepository.RefreshCacheAsync();
        }

        public async Task<PayItemConstants.PayItemFormulaDto?> GetActiveFormulaAsync(long payItemId, DateTime atDate,
            CancellationToken cancellationToken = default)
        {
            var entity = await _payItemRepository.GetWithIncludesAsync(payItemId, cancellationToken, x => x.Formulas);
            if (entity == null) return null;

            var activeFormula = entity.Formulas.FirstOrDefault(f => f.IsActive && f.ValidFromDate >= atDate && (f.ValidToDate is null || f.ValidToDate > atDate));
            if (activeFormula != null)
                return new PayItemConstants.PayItemFormulaDto
                {
                    Id = activeFormula.Id,
                    Formula = activeFormula.Formula,
                    Version = activeFormula.Version,
                    ValidFromDate = activeFormula.ValidFromDate,
                    ValidToDate = activeFormula.ValidToDate,
                    IsActive = activeFormula.IsActive
                };

            // اگر فرمولی نداشت و سیستمی است، فرمول پیش‌فرض برگردان
            var defaultFormula = GetDefaultFormula(entity.SystemCode);
            if (defaultFormula != null)
                return new PayItemConstants.PayItemFormulaDto
                {
                    Formula = defaultFormula,
                    Version = 1,
                    ValidFromDate = DateTime.MinValue,
                    IsActive = true
                };

            return null;
        }

        public async Task<PayItemConstants.PayItemFormulaDto?> GetActiveFormulaAsync(long payItemId, CancellationToken cancellationToken = default)
        {
            var entity = await _payItemRepository.GetWithIncludesAsync(payItemId, cancellationToken, x => x.Formulas);
            if (entity == null) return null;

            var activeFormula = entity.Formulas.FirstOrDefault(f => f.IsActive);
            if (activeFormula != null)
                return new PayItemConstants.PayItemFormulaDto
                {
                    Id = activeFormula.Id,
                    Formula = activeFormula.Formula,
                    Version = activeFormula.Version,
                    ValidFromDate = activeFormula.ValidFromDate,
                    ValidToDate = activeFormula.ValidToDate,
                    IsActive = activeFormula.IsActive
                };

            // اگر فرمولی نداشت و سیستمی است، فرمول پیش‌فرض برگردان
            var defaultFormula = GetDefaultFormula(entity.SystemCode);
            if (defaultFormula != null)
                return new PayItemConstants.PayItemFormulaDto
                {
                    Formula = defaultFormula,
                    Version = 1,
                    ValidFromDate = DateTime.MinValue,
                    IsActive = true
                };

            return null;
        }


        // ══════════════════════════════════════════════════════════════
        // عوامل سیستمی
        // ══════════════════════════════════════════════════════════════

        public async Task<PayItemConstants.PayItemDto?> UpdateSystemPayItemAsync(UpdateSystemPayItemDto dto, CancellationToken cancellationToken = default)
        {
            var defaultItem = GetDefaultItem(dto.SystemCode)
                ?? throw new ArgumentException($"عامل سیستمی با کد '{dto.SystemCode}' یافت نشد");

            // پیدا کردن یا ایجاد رکورد DB برای این عامل سیستمی
            var entity = await _payItemRepository.GetBySystemCodeAsync(dto.SystemCode,cancellationToken);

            if (entity == null)
            {
                // اولین بار override می‌شود → رکورد جدید با IsCustom = false
                entity = new PayItem(
                    defaultItem.Name,
                    defaultItem.SystemCode,
                    defaultItem.Type,
                    defaultItem.DataType,
                    dto.IsInsured,
                    dto.IsTaxable,
                    isEditable: true,    // ثابت
                    dto.IsActive,
                    dto.SortOrder);

                // فرمول
                var formulaToUse = ResolveFormula(dto, defaultItem);
                if (entity.DataType == PayItem.PayItemDataType.Formula && !string.IsNullOrWhiteSpace(formulaToUse.formula))
                    entity.AddFormula(formulaToUse.formula, formulaToUse.validFrom);

                await _payItemRepository.AddAsync(entity, cancellationToken);
            }
            else
            {
                // بروزرسانی فیلدهای مجاز
                entity.UpdateIsInsured(dto.IsInsured);
                entity.UpdateIsTaxable(dto.IsTaxable);
                entity.UpdateIsActive(dto.IsActive);
                entity.UpdateSortOrder(dto.SortOrder);

                // مدیریت فرمول
                if (entity.DataType == PayItem.PayItemDataType.Formula)
                {
                    var formulaToUse = ResolveFormula(dto, defaultItem);
                    if (!string.IsNullOrWhiteSpace(formulaToUse.formula))
                        entity.ReplaceFormula(formulaToUse.formula, formulaToUse.validFrom);
                }

                await _payItemRepository.UpdateAsync(entity, cancellationToken);
            }

            await _payItemRepository.ExecuteInTransactionAsync(async () => await _payItemRepository.SaveChangesAsync(cancellationToken), cancellationToken);

            return MergeWithDefault(MapToDto(entity), defaultItem);
        }


        public async Task ResetSystemPayItemFormulaAsync(string systemCode, CancellationToken cancellationToken = default)
        {
            var defaultItem = GetDefaultItem(systemCode)
                              ?? throw new ArgumentException($"عامل سیستمی با کد '{systemCode}' یافت نشد");

            var entity = await _payItemRepository.GetBySystemCodeAsync(systemCode, cancellationToken);
            if (entity == null) return; // هنوز override نشده، فرمول همان default است

            var defaultFormula = defaultItem.Formulas.FirstOrDefault(f => f.IsActive)?.Formula;
            if (string.IsNullOrWhiteSpace(defaultFormula)) return;

            entity.ReplaceFormula(defaultFormula, DateTime.Now);

            await _payItemRepository.UpdateAsync(entity, cancellationToken);
            await _payItemRepository.SaveChangesAsync(cancellationToken);
        }

        public bool IsSystemItem(string systemCode)
            => PayItemConstants.AllSystemItems.Any(x =>
                x.SystemCode.Equals(systemCode, StringComparison.OrdinalIgnoreCase));

        public string? GetDefaultFormula(string systemCode)
            => GetDefaultItem(systemCode)
                ?.Formulas.FirstOrDefault(f => f.IsActive)
                ?.Formula;

        #region Private Helpers
        private static PayItemConstants.PayItemDto? GetDefaultItem(string systemCode)
            => PayItemConstants.AllSystemItems
                .FirstOrDefault(x => x.SystemCode.Equals(systemCode, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// تصمیم‌گیری درباره فرمول نهایی:
        /// اگر UseDefaultFormula = true یا فرمول وارد شده خالی بود → فرمول پیش‌فرض
        /// </summary>
        private (string formula, DateTime validFrom) ResolveFormula(
            UpdateSystemPayItemDto dto, PayItemConstants.PayItemDto defaultItem)
        {
            var defaultFormula = defaultItem.Formulas.FirstOrDefault(f => f.IsActive)?.Formula ?? string.Empty;

            if (dto.UseDefaultFormula || string.IsNullOrWhiteSpace(dto.Formula))
                return (defaultFormula, dto.FormulaValidFromDate ?? DateTime.Now);

            return (dto.Formula, dto.FormulaValidFromDate ?? DateTime.Now);
        }


        /// <summary>
        /// ترکیب DTO از DB با default از Constants:
        /// Name / SystemCode / Type / DataType همیشه از constants می‌آید
        /// </summary>
        private static PayItemConstants.PayItemDto MergeWithDefault(
            PayItemConstants.PayItemDto dbDto,
            PayItemConstants.PayItemDto defaultItem)
        {
            // اگر فرمولی در DB نداشت، فرمول پیش‌فرض را نمایش بده
            if (dbDto.DataType == PayItem.PayItemDataType.Formula
                && !dbDto.Formulas.Any(f => f.IsActive)
                && defaultItem.Formulas.Any(f => f.IsActive))
            {
                dbDto.Formulas = defaultItem.Formulas
                    .Select(f => new PayItemConstants.PayItemFormulaDto
                    {
                        Formula = f.Formula + "  ‹پیش‌فرض›",
                        Version = f.Version,
                        ValidFromDate = f.ValidFromDate,
                        IsActive = true
                    }).ToList();
            }

            // فیلدهای ثابت از constants
            dbDto.Name = defaultItem.Name;
            dbDto.SystemCode = defaultItem.SystemCode;
            dbDto.Type = defaultItem.Type;
            dbDto.DataType = defaultItem.DataType;
            dbDto.Group = defaultItem.Group;
            dbDto.IsCustom = false;

            return dbDto;
        }

        private static PayItemConstants.PayItemDto MapToDto(PayItem item)
        {
            return new PayItemConstants.PayItemDto
            {
                Id = item.Id,
                Name = item.Name,
                SystemCode = item.SystemCode,
                Type = item.Type,
                DataType = item.DataType,
                IsInsured = item.IsInsured,
                IsTaxable = item.IsTaxable,
                IsEditable = item.IsEditable,
                IsActive = item.IsActive,
                SortOrder = item.SortOrder,
                IsCustom = item.IsCustom,
                Formulas = item.Formulas.Select(f => new PayItemConstants.PayItemFormulaDto
                {
                    Id = f.Id,
                    Formula = f.Formula,
                    Version = f.Version,
                    ValidFromDate = f.ValidFromDate,
                    ValidToDate = f.ValidToDate,
                    IsActive = f.IsActive
                }).ToList()
            };
        }
        #endregion
    }
}
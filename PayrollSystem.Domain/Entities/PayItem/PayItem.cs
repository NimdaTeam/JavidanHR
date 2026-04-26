using Ardalis.GuardClauses;
using _0_Framework.EntityBase;
using _0_Framework.Utilities.Security;
using PayrollSystem.Domain.Common;

namespace PayrollSystem.Domain.Entities.PayItem
{
    public class PayItem : EntityBase
    {
        public string Name { get; private set; }
        public string SystemCode { get; private set; }
        public PayItemType Type { get; private set; }
        public PayItemDataType DataType { get; private set; }
        public bool IsInsured { get; private set; }
        public bool IsTaxable { get; private set; }
        public bool IsEditable { get; private set; }
        public bool IsActive { get; private set; }
        public int SortOrder { get; private set; }
        public bool IsCustom { get; private set; }

        // رابطه یک به چند با فرمول‌ها
        private readonly List<PayItemFormula> _formulas = [];
        public IReadOnlyCollection<PayItemFormula> Formulas => _formulas.AsReadOnly();

        // فرمول فعال فعلی (صرف‌نظر از تاریخ - فرض می‌کنیم تاریخ جاری مد نظر است)
        public PayItemFormula? CurrentFormula => GetCurrentFormula(DateTime.Now);

        private PayItem() { }

        public PayItem(
            string name,
            string systemCode,
            PayItemType type,
            PayItemDataType dataType,
            bool isInsured,
            bool isTaxable,
            bool isEditable,
            bool isActive,
            int sortOrder = 0)
        {
            UpdateName(name);
            UpdateSystemCode(systemCode);
            UpdateType(type);
            UpdateDataType(dataType);
            UpdateIsInsured(isInsured);
            UpdateIsTaxable(isTaxable);
            UpdateIsEditable(isEditable);
            UpdateIsActive(isActive);
            UpdateSortOrder(sortOrder);

            IsCustom = PayItemConstants.DefaultPayItems.All(x => x.SystemCode != systemCode);
        }

        // ---------------------- متدهای تغییر خصوصیات ----------------------
        public void UpdateName(string name)
        {
            Guard.Against.NullOrWhiteSpace(name, nameof(name));
            Guard.Against.OutOfRange(name.Length, nameof(name), 1, 100);
            Name = name.SanitizeString();
        }

        public void UpdateSystemCode(string code)
        {
            Guard.Against.NullOrWhiteSpace(code, nameof(code));
            code = code.Trim().ToUpperInvariant();
            Guard.Against.OutOfRange(code.Length, nameof(code), 1, 50);
            //if (!PayItemConstants.IsValidCode(code))
            //    throw new ArgumentException($"کد سیستمی '{code}' معتبر نیست. کدهای معتبر: {string.Join(", ", PayItemConstants.GetAllValidCodes())}", nameof(code));
            SystemCode = code.SanitizeString();
            IsCustom = PayItemConstants.GetAllValidItems().All(x => x.SystemCode != code);
        }

        public void UpdateType(PayItemType type) => Type = type;
        public void UpdateDataType(PayItemDataType dataType) => DataType = dataType;
        public void UpdateIsInsured(bool isInsured) => IsInsured = isInsured;
        public void UpdateIsTaxable(bool isTaxable) => IsTaxable = isTaxable;
        public void UpdateIsEditable(bool isEditable) => IsEditable = isEditable;
        public void UpdateIsActive(bool isActive) => IsActive = isActive;
        public void UpdateSortOrder(int sortOrder)
        {
            Guard.Against.Negative(sortOrder, nameof(sortOrder));
            SortOrder = sortOrder;
        }

        // ---------------------- مدیریت فرمول‌ها (فقط زمانی که DataType == Formula باشد) ----------------------
        public void AddFormula(string formula, DateTime validFromDate, DateTime? validToDate = null)
        {
            if (DataType != PayItemDataType.Formula)
                throw new InvalidOperationException("فقط آیتم‌های با نوع فرمول می‌توانند فرمول داشته باشند");

            // غیرفعال کردن فرمول فعال قبلی (در صورتی که همپوشانی زمانی داشته باشد)
            var existingActive = _formulas.FirstOrDefault(f => f.IsActive);
            if (existingActive != null)
            {
                // اگر فرمول جدید همپوشانی زمانی با فرمول فعال قبلی دارد، آن را غیرفعال کن
                if (existingActive.ValidFromDate <= validFromDate &&
                    (!existingActive.ValidToDate.HasValue || existingActive.ValidToDate >= validFromDate))
                {
                    existingActive.Deactivate();
                }
            }

            var newFormula = new PayItemFormula(Id, formula.SanitizeString(), validFromDate, validToDate);
            _formulas.Add(newFormula);
        }

        public void UpdateCurrentFormula(string newFormula)
        {
            if (DataType != PayItemDataType.Formula)
                throw new InvalidOperationException("فقط آیتم‌های با نوع فرمول می‌توانند فرمول داشته باشند");

            var current = CurrentFormula;
            if (current == null)
                throw new InvalidOperationException("هیچ فرمول فعالی برای این آیتم وجود ندارد");

            current.UpdateFormula(newFormula.SanitizeString());
        }

        // پایان اعتبار فرمول فعلی (غیرفعال کردن و تنظیم ValidToDate)
        public void ExpireCurrentFormula(DateTime expireDate)
        {
            var current = CurrentFormula;
            if (current == null) return;

            if (expireDate <= current.ValidFromDate)
                throw new ArgumentException("تاریخ پایان اعتبار باید بعد از تاریخ شروع باشد");

            current.UpdateValidToDate(expireDate);
            current.Deactivate();
        }

        // جایگزینی فرمول
        public void ReplaceFormula(string newFormula, DateTime newValidFromDate)
        {
            if (CurrentFormula != null)
                ExpireCurrentFormula(newValidFromDate.AddSeconds(-1));
            AddFormula(newFormula.SanitizeString(), newValidFromDate);
        }

        // فعال کردن یک فرمول خاص و غیرفعال کردن سایر فرمول‌ها
        public void ActivateFormula(long formulaId)
        {
            var target = _formulas.FirstOrDefault(f => f.Id == formulaId);
            if (target == null)
                throw new ArgumentException("فرمول یافت نشد");

            foreach (var f in _formulas)
            {
                if (f.Id == formulaId)
                    f.Activate();
                else
                    f.Deactivate();
            }
        }



        // فرمول فعال و معتبر در زمان جاری
        public PayItemFormula? GetCurrentFormula(DateTime atDate)
        {
            return _formulas.SingleOrDefault(f => f.IsValidAt(atDate));
        }


        // ---------------------- Enums توکار ----------------------
        public enum PayItemType
        {
            Earning,    // مزایا
            Deduction,  // کسورات
            Info        // اطلاعاتی
        }

        public enum PayItemDataType
        {
            Decimal,    // مقدار عددی ثابت یا محاسباتی
            Boolean,    // درست/غلط
            Formula,    // محاسبه از طریق فرمول
            UserInput   // ورودی دستی در زمان صدور فیش
        }
    }
}
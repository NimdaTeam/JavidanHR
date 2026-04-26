// PayItemDto.cs
using PayrollSystem.Domain.Entities.PayItem;
using System.ComponentModel.DataAnnotations;

namespace PayrollSystem.Application.DTOs
{
    // ---------------------- DTO ایجاد عامل حقوقی ----------------------
    public class CreatePayItemDto
    {
        [Required(ErrorMessage = "نام عامل حقوقی الزامی است")]
        [MaxLength(100, ErrorMessage = "نام نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        [Display(Name = "نام عامل حقوقی")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "کد سیستمی الزامی است")]
        [MaxLength(50, ErrorMessage = "کد سیستمی نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        [Display(Name = "کد سیستمی")]
        public string SystemCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "نوع عامل الزامی است")]
        [Display(Name = "نوع عامل")]
        public PayItem.PayItemType Type { get; set; }

        [Required(ErrorMessage = "نوع داده الزامی است")]
        [Display(Name = "نوع داده")]
        public PayItem.PayItemDataType DataType { get; set; }

        [Display(Name = "مشمول بیمه")]
        public bool IsInsured { get; set; } = true;

        [Display(Name = "مشمول مالیات")]
        public bool IsTaxable { get; set; } = true;

        [Display(Name = "قابل ویرایش")]
        public bool IsEditable { get; set; } = true;

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        [Range(0, int.MaxValue, ErrorMessage = "ترتیب نمایش باید عدد مثبت باشد")]
        [Display(Name = "ترتیب نمایش")]
        public int SortOrder { get; set; } = 0;

        // فقط در صورتی که DataType == Formula باشد
        [Display(Name = "فرمول")]
        public string? Formula { get; set; }

        [Display(Name = "تاریخ شروع اعتبار فرمول")]
        public DateTime? FormulaValidFromDate { get; set; }
    }

    // ---------------------- DTO ویرایش عامل حقوقی ----------------------
    public class UpdatePayItemDto:CreatePayItemDto
    {
        public long Id { get; set; }
        public bool IsCustom { get; set; }
    }

    /// <summary>
    /// DTO ویژه ویرایش عوامل سیستمی.
    /// Name / SystemCode / Type / DataType فقط‌خواندنی‌اند و در view نمایش داده می‌شوند.
    /// </summary>
    public class UpdateSystemPayItemDto
    {
        // ── فقط‌خواندنی ──────────────────────────────
        public string SystemCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string TypeDisplay { get; set; } = string.Empty;       // "مزایا" | "کسورات" | ...
        public string DataTypeDisplay { get; set; } = string.Empty;   // "فرمول" | "عدد اعشاری" | ...
        public bool HasDataTypeFormula { get; set; }                   // آیا این عامل فرمول‌پذیر است؟
        public string? DefaultFormula { get; set; }                    // فرمول پیش‌فرض از PayItemConstants

        // ── قابل ویرایش ──────────────────────────────
        public bool IsInsured { get; set; }
        public bool IsTaxable { get; set; }
        public bool IsActive { get; set; }

        [Range(0, int.MaxValue)]
        public int SortOrder { get; set; }

        // فرمول override — اگر UseDefaultFormula = true باشد، فرمول پیش‌فرض اعمال می‌شود
        public bool UseDefaultFormula { get; set; } = true;
        public string? Formula { get; set; }
        public DateTime? FormulaValidFromDate { get; set; }
    }

    // ---------------------- DTO افزودن/ویرایش فرمول ----------------------
    public class AddPayItemFormulaDto
    {
        public long PayItemId { get; set; }

        [Required(ErrorMessage = "فرمول الزامی است")]
        [MaxLength(500, ErrorMessage = "فرمول نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        [Display(Name = "فرمول")]
        public string Formula { get; set; } = string.Empty;

        [Required(ErrorMessage = "تاریخ شروع اعتبار الزامی است")]
        [Display(Name = "تاریخ شروع اعتبار")]
        public DateTime ValidFromDate { get; set; } = DateTime.Now;

        [Display(Name = "تاریخ پایان اعتبار")]
        public DateTime? ValidToDate { get; set; }
    }
}
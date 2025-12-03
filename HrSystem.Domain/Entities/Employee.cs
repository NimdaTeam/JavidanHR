using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _0_Framework.EntityBase;

namespace HrSystem.Domain.Entities
{
    [Table("Employees")]
    public class Employee : EntityBase
    {
        // === اطلاعات پایه - مرحله ۱ (اجباری) ===
        [Required(ErrorMessage = "کد کارمندی الزامی است")]
        [StringLength(20)]
        [Display(Name = "کد کارمندی")]
        public string EmployeeCode { get; set; } = null!;

        [Required(ErrorMessage = "نام الزامی است")]
        [StringLength(50)]
        [Display(Name = "نام")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "نام خانوادگی الزامی است")]
        [StringLength(70)]
        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; } = null!;

        [NotMapped]
        [Display(Name = "نام و نام خانوادگی")]
        public string FullName => $"{FirstName} {LastName}".Trim();

        [Required(ErrorMessage = "کد ملی الزامی است")]
        [StringLength(10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "کد ملی باید ۱۰ رقم باشد")]
        [Display(Name = "کد ملی")]
        public string NationalCode { get; set; } = null!;

        [Required]
        [Display(Name = "تاریخ تولد")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [NotMapped]
        [Display(Name = "سن (سال)")]
        public int Age => DateTime.Now.Year - BirthDate.Year - (DateTime.Now.DayOfYear < BirthDate.DayOfYear ? 1 : 0);

        [Required]
        [Display(Name = "جنسیت")]
        public Gender Gender { get; set; }

        [Required]
        [Display(Name = "وضعیت تاهل")]
        public MaritalStatus MaritalStatus { get; set; }

        [Display(Name = "تعداد فرزند")]
        public int ChildrenCount { get; set; } = 0;

        // === مرحله ۲ - اطلاعات تماس (همه اختیاری) ===
        [Phone]
        [StringLength(11)]
        [Display(Name = "شماره تماس")]
        public string? PhoneNumber { get; set; }

        [EmailAddress]
        [Display(Name = "ایمیل")]
        public string? Email { get; set; }

        [StringLength(500)]
        [Display(Name = "آدرس")]
        public string? Address { get; set; }

        [StringLength(150)]
        [Display(Name = "شخص ضروری در مواقع اضطرار")]
        public string? EmergencyContactName { get; set; }

        [Phone]
        [StringLength(11)]
        [Display(Name = "شماره تماس اضطرار")]
        public string? EmergencyContactPhone { get; set; }

        [Display(Name = "تصویر پرسنلی")]
        public string? ProfileImageUrl { get; set; }

        // === مرحله ۳ - اطلاعات سازمانی (اختیاری) ===
        [StringLength(200)]
        [Display(Name = "واحد سازمانی")]
        public string? Department { get; set; }

        [StringLength(200)]
        [Display(Name = "پست سازمانی")]
        public string? Position { get; set; }

        [Display(Name = "سطح تحصیلات")]
        public EducationLevel? EducationLevel { get; set; }

        [StringLength(150)]
        [Display(Name = "رشته تحصیلی")]
        public string? FieldOfStudy { get; set; }

        [Display(Name = "نوع قرارداد")]
        public ContractType? ContractType { get; set; }

        [Display(Name = "نوع همکاری")]
        public CooperationType? CooperationType { get; set; }

        [Display(Name = "تاریخ استخدام")]
        [DataType(DataType.Date)]
        public DateTime? HireDate { get; set; }

        [NotMapped]
        [Display(Name = "سابقه کار (سال)")]
        public int WorkExperienceYears => HireDate.HasValue
            ? (TerminationDate.HasValue
                ? TerminationDate.Value.Year - HireDate.Value.Year
                : DateTime.Today.Year - HireDate.Value.Year)
            : 0;

        // === مرحله ۴ - اطلاعات مالی (اختیاری) ===
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "حقوق پایه")]
        public decimal? BaseSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "فوق‌العاده مزایا و کمک‌هزینه‌ها")]
        public decimal? Benefits { get; set; } = 0;

        [NotMapped]
        [Display(Name = "کل دریافتی")]
        public decimal TotalSalary => (BaseSalary ?? 0) + (Benefits ?? 0);

        [StringLength(100)]
        [Display(Name = "بانک حساب")]
        public string? BankName { get; set; }

        [StringLength(50)]
        [Display(Name = "شماره حساب")]
        public string? AccountNumber { get; set; }

        [StringLength(50)]
        [Display(Name = "شماره کارت")]
        public string? CardNumber { get; set; }

        [Display(Name = "نحوه پرداخت حقوق")]
        public PaymentMethod? PaymentMethod { get; set; }

        // === مرحله ۵ - اطلاعات اضافی (همه اختیاری) ===
        [StringLength(50)]
        [Display(Name = "شماره بیمه")]
        public string? InsuranceNumber { get; set; }

        [Display(Name = "وضعیت سلامت")]
        public HealthStatus? HealthStatus { get; set; }

        [StringLength(100)]
        [Display(Name = "سطح مهارت")]
        public string? SkillLevel { get; set; }

        [Display(Name = "دوره‌های آموزشی گذرانده")]
        public string? CompletedTrainings { get; set; }

        [Display(Name = "مدارک و گواهینامه‌ها")]
        public string? Certificates { get; set; }

        [Display(Name = "پروژه‌های محول شده")]
        public string? AssignedProjects { get; set; }

        [Display(Name = "توضیحات")]
        public string? Notes { get; set; }

        // === وضعیت بازنشستگی و ترک کار (اختیاری) ===
        [Display(Name = "وضعیت بازنشستگی")]
        public RetirementStatus? RetirementStatus { get; set; }

        [Display(Name = "تاریخ بازنشستگی")]
        [DataType(DataType.Date)]
        public DateTime? RetirementDate { get; set; }

        [Display(Name = "وضعیت فعالیت")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "دلیل ترک کار")]
        public string? TerminationReason { get; set; }

        [Display(Name = "تاریخ ترک کار")]
        [DataType(DataType.Date)]
        public DateTime? TerminationDate { get; set; }

        // === مرخصی ===
        [Display(Name = "روزهای مرخصی استحقاقی")]
        public int AnnualLeaveDays { get; set; } = 26;

        [Display(Name = "روزهای مرخصی استفاده شده")]
        public int UsedLeaveDays { get; set; } = 0;

        [NotMapped]
        [Display(Name = "روزهای مرخصی باقیمانده")]
        public int RemainingLeaveDays => AnnualLeaveDays - UsedLeaveDays;

        // === ارزیابی ===
        [Range(0, 100)]
        [Display(Name = "نمره ارزیابی عملکرد")]
        public int? PerformanceScore { get; set; }

        // === Audit ===
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    // ==== Enums ====
    public enum Gender
    {
        Male = 1,
        Female = 2
    }

    public enum MaritalStatus
    {
        Single = 1,
        Married = 2,
        Divorced = 3,
        Widowed = 4        // همسر فوت‌شده
    }

    public enum EducationLevel
    {
        BelowDiploma = 1,
        Diploma = 2,
        Associate = 3,
        Bachelor = 4,
        Master = 5,
        Doctorate = 6
    }

    public enum ContractType
    {
        Permanent = 1,      // رسمی
        FixedTerm = 2,      // پیمانی/قراردادی
        DailyWage = 3,      // روزمزد
        PartTime = 4,
        CommissionBased = 5 // کارمزدی
    }

    public enum CooperationType
    {
        FullTime = 1,
        PartTime = 2,
        Consultant = 3,
        Volunteer = 4,
        BoardMember = 5     // هیئت امنا
    }

    public enum HealthStatus
    {
        Healthy = 1,
        ChronicIllness = 2,
        MedicalExemption = 3
    }

    public enum RetirementStatus
    {
        Active = 1,
        NearRetirement = 2,
        Retired = 3
    }

    public enum PaymentMethod
    {
        BankTransfer = 1,
        Cash = 2,
        CardToCard = 3
    }

}

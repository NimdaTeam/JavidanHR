using HrSystem.Application.common.Extensions;
using HrSystem.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using _0_Framework.Utilities.Helpers;

namespace HrSystem.Application.DTO
{
    public class Step1PersonalVM
    {
        [Required] public string EmployeeCode { get; set; }
        [Required] public string FirstName { get; set; }
        [Required] public string LastName { get; set; }
        [Required] public string NationalCode { get; set; }
        [Required] public DateTime BirthDate { get; set; }
        [Required] public Gender Gender { get; set; }
        [Required] public MaritalStatus MaritalStatus { get; set; }
        public int ChildrenCount { get; set; }
        public IFormFile? ProfileImage { get; set; }

        public string CurrentImage { get; set; }

        public bool IsUpdating { get; set; } = false;
        public long? Id { get; set; } = null;

        public string? PersianBirthDateStringify { get; set; }
    }

    public class Step2ContactVM
    {
        public long Id { get; set; }
        [Required] public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string EmergencyContactName { get; set; }
        public string EmergencyContactPhone { get; set; }
    }

    public class Step3OrganizationalVM
    {
        public long Id { get; set; }
        [Required] public string Department { get; set; }
        [Required] public string Position { get; set; }
        [Required] public EducationLevel EducationLevel { get; set; }
        public string FieldOfStudy { get; set; }
        [Required] public ContractType ContractType { get; set; }
        [Required] public CooperationType CooperationType { get; set; }
        [Required] public DateTime HireDate { get; set; }

        public string HireDatePersianStringify { get; set; }
    }

    public class Step4FinancialVM
    {
        public long Id { get; set; }
        [Required] public decimal BaseSalary { get; set; }
        public decimal Benefits { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string CardNumber { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }

    public class Step5AdditionalVM
    {
        public long Id { get; set; }
        public string InsuranceNumber { get; set; }
        public HealthStatus HealthStatus { get; set; } = HealthStatus.Healthy;
        public string SkillLevel { get; set; }
        public string CompletedTrainings { get; set; }
        public string Certificates { get; set; }
        public string AssignedProjects { get; set; }
        public string Notes { get; set; }
    }


    public class EmployeeListItemDTO
    {
        public long Id { get; set; }
        public string EmployeeCode { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string FullName => $"{FirstName} {LastName}".Trim();

        public string? ProfileImageUrl { get; set; }

        // اطلاعات سازمانی
        public string? Department { get; set; }
        public string? Position { get; set; }
        public CooperationType? CooperationType { get; set; }

        // وضعیت
        public bool IsActive { get; set; } = true;
        public MaritalStatus? MaritalStatus { get; set; }

        // --- وضعیت تکمیل هر مرحله ---
        public bool Step1Completed => true; // مرحله ۱ همیشه کامله (چون بدونش کارمند ساخته نمیشه)
        public bool Step2Completed =>
            !string.IsNullOrWhiteSpace(PhoneNumber) ||
            !string.IsNullOrWhiteSpace(Email) ||
            !string.IsNullOrWhiteSpace(Address);

        public bool Step3Completed =>
            Department != null &&
            Position != null &&
            CooperationType != null &&
            HireDate != null;

        public bool Step4Completed =>
            BaseSalary > 0 &&
            PaymentMethod != null &&
            !string.IsNullOrWhiteSpace(BankName);

        public bool Step5Completed =>
            !string.IsNullOrWhiteSpace(InsuranceNumber) ||
            HealthStatus != null ||
            !string.IsNullOrWhiteSpace(SkillLevel) ||
            !string.IsNullOrWhiteSpace(Notes);

        // درصد تکمیل کلی (اختیاری برای نمایش)
        public int ProfileCompletionPercentage =>
            (Step1Completed ? 20 : 0) +
            (Step2Completed ? 20 : 0) +
            (Step3Completed ? 20 : 0) +
            (Step4Completed ? 20 : 0) +
            (Step5Completed ? 20 : 0);

        // --- فیلدهای مورد نیاز برای چک کردن تکمیل ---
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public DateTime? HireDate { get; set; }
        public decimal BaseSalary { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public string? BankName { get; set; }
        public string? InsuranceNumber { get; set; }
        public HealthStatus? HealthStatus { get; set; }
        public string? SkillLevel { get; set; }
        public string? Notes { get; set; }
    }

    public class EmployeeDetailsVM : Employee
    {
        // --- فقط چیزایی که در Entity نیست یا نیاز به فرمت دارن ---
        public string PersianBirthDate => BirthDate.ToShamsi();
        public string PersianHireDate => HireDate?.ToShamsi() ?? "وارد نشده";
        public string PersianTerminationDate => TerminationDate?.ToShamsi() ?? "فعال";
        public string PersianRetirementDate => RetirementDate?.ToShamsi() ?? "فعال";

        // --- برای نمایش بهتر Enum ها ---
        public string GenderDisplay => Gender.ToPersian();
        public string MaritalStatusDisplay => MaritalStatus.ToPersian();
        public string EducationLevelDisplay => EducationLevel?.ToPersian() ?? "مشخص نشده";
        public string ContractTypeDisplay => ContractType?.ToPersian() ?? "مشخص نشده";
        public string CooperationTypeDisplay => CooperationType?.ToPersian() ?? "مشخص نشده";
        public string PaymentMethodDisplay => PaymentMethod?.ToPersian() ?? "مشخص نشده";
        public string HealthStatusDisplay => HealthStatus?.ToPersian() ?? "سالم";
        public string RetirementStatusDisplay => RetirementStatus?.ToPersian() ?? "فعال";

        // --- وضعیت تکمیل مراحل ---
        public bool Step1Completed => true;

        public bool Step2Completed =>
            !string.IsNullOrWhiteSpace(PhoneNumber) ||
            !string.IsNullOrWhiteSpace(Email) ||
            !string.IsNullOrWhiteSpace(Address);

        public bool Step3Completed =>
            !string.IsNullOrWhiteSpace(Department) &&
            !string.IsNullOrWhiteSpace(Position) &&
            EducationLevel.HasValue &&
            ContractType.HasValue &&
            CooperationType.HasValue &&
            HireDate.HasValue;

        public bool Step4Completed =>
            BaseSalary.HasValue && BaseSalary > 0 &&
            !string.IsNullOrWhiteSpace(BankName) &&
            PaymentMethod.HasValue;

        public bool Step5Completed =>
            !string.IsNullOrWhiteSpace(InsuranceNumber) ||
            HealthStatus.HasValue ||
            !string.IsNullOrWhiteSpace(SkillLevel);

        // --- درصد تکمیل پروفایل ---
        public int ProfileCompletionPercentage =>
            (Step1Completed ? 20 : 0) +
            (Step2Completed ? 20 : 0) +
            (Step3Completed ? 20 : 0) +
            (Step4Completed ? 20 : 0) +
            (Step5Completed ? 20 : 0);
    }
}

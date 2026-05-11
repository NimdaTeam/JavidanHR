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
using _0_Framework.Mappings.Interfaces;
using _0_Framework.Utilities.Helpers;

namespace HrSystem.Application.DTO
{
    public class EmployeeWorkshopListItem
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }

    public class Step1PersonalVM
    {
        public long? UserId { get; set; }
        public long? Id { get; set; }
        public bool IsUpdating { get; set; } = false;

        //workshop
        [Required] public long? WorkshopId { get; set; }

        // هویتی
        [Required] public string EmployeeCode { get; set; } = null!;
        [Required] public string FirstName { get; set; } = null!;
        [Required] public string LastName { get; set; } = null!;
        public string? PreviousLastName { get; set; }
        [Required] public string FathersName { get; set; } = null!;
        public string? Nickname { get; set; }
        [Required] public string NationalCode { get; set; } = null!;
        public string? IdNumber { get; set; }
        public string? IdIssuePlace { get; set; }
        public string? BirthPlace { get; set; }
        [Required] public string PersianBirthDateStringify { get; set; } = null!;
        [Required] public string MobilePhone { get; set; } = null!;
        public string? HomePhone { get; set; }
        [Required] public Gender Gender { get; set; }

        // نظام وظیفه
        public MilitaryStatus MilitaryStatus { get; set; }
        public string? ServiceEndDatePersian { get; set; }
        public string? ExemptionReason { get; set; }

        // تأهل و مسکن
        [Required] public MaritalStatus MaritalStatus { get; set; }
        public string? MarriageDatePersian { get; set; }
        [Required] public HousingStatus HousingStatus { get; set; }
        public string? PersonalAddress { get; set; }
        public string? RentalAddress { get; set; }

        // اعضای خانواده
        public List<FamilyMemberVM> FamilyMembers { get; set; } = new();

        // تصویر
        public IFormFile? ProfileImage { get; set; }
        public string? CurrentImage { get; set; }
    }

    public class Step2EducationVM
    {
        public long EmployeeId { get; set; }
        public bool IsUpdating { get; set; } = false;

        // === تحصیلات رسمی (یک رکورد) ===
        public EducationLevel? AcademicLevel { get; set; }
        public string? FieldOfStudy { get; set; }
        public string? StartYearPersian { get; set; }
        public string? EndYearPersian { get; set; }
        public string? InstituteName { get; set; }
        public string? InstituteCity { get; set; }
        public string? InstituteAddress { get; set; }

        public List<EmployeeTrainingVM> Trainings { get; set; } = [];
    }

    public class EmployeeTrainingVM
    {
        public long? Id { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string Institute { get; set; } = string.Empty;
        public string? Hours { get; set; }
        public string? CertificateNumberAndDate { get; set; }
        public string? Notes { get; set; }
    }



    //step 3 
    public class Step3WorkExperienceVM
    {
        public long EmployeeId { get; set; }
        public bool IsUpdating { get; set; } = true;

        // لیست تجربیات کاری
        public List<WorkExperienceVM> WorkExperiences { get; set; } = [];
    }


    public class Step4EmploymentVM
    {
        public long EmployeeId { get; set; }
        public bool IsUpdating { get; set; } = true;

        // اطلاعات سازمانی
        [Required(ErrorMessage = "معاونت/مدیریت الزامی است")]
        public string Management { get; set; } = null!;

        [Required(ErrorMessage = "دپارتمان الزامی است")]
        public string Department { get; set; } = null!;

        [Required(ErrorMessage = "واحد سازمانی الزامی است")]
        public string Unit { get; set; } = null!;

        [Required(ErrorMessage = "سمت الزامی است")]
        public string Position { get; set; } = null!;

        [Required(ErrorMessage = "نوع قرارداد الزامی است")]
        public ContractType? ContractType { get; set; }

        [Required(ErrorMessage = "نوع همکاری الزامی است")]
        public CooperationType? CooperationType { get; set; }

        [Required(ErrorMessage = "تاریخ استخدام الزامی است")]
        public string HireDatePersian { get; set; } = null!;

        public string? InsuranceNumber { get; set; }

        [Required(ErrorMessage = "وضعیت سلامت الزامی است")]
        public HealthStatus? HealthStatus { get; set; }

        public int AnnualLeaveDays { get; set; } = 26;
        public int UsedLeaveDays { get; set; } = 0;

        public int? PerformanceScore { get; set; }

        public RetirementStatus? RetirementStatus { get; set; }
        public string? RetirementDatePersian { get; set; }

        [Required(ErrorMessage = "وضعیت فعالیت الزامی است")]
        public bool IsActive { get; set; } = true;

        public string? TerminationReason { get; set; }
        public string? TerminationDatePersian { get; set; }
    }


    public class Step5FinancialVM
    {
        public long EmployeeId { get; set; }
        public bool IsUpdating { get; set; } = true;

        // اطلاعات مالی
        public string? BaseSalary { get; set; }
        public string? Benefits { get; set; } = "";
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? CardNumber { get; set; }

        [Required(ErrorMessage = "روش پرداخت الزامی است")]
        public PaymentMethod? PaymentMethod { get; set; }

        // لیست وام‌ها
        public List<EmployeeLoanVM> Loans { get; set; } = new();
    }

    public class EmployeeLoanVM
    {
        public long? Id { get; set; }
        public string Amount { get; set; }
        public string BorrowerName { get; set; } = null!;
        public string? Guarantors { get; set; }
        public string? SettlementDatePersian { get; set; }
    }

    /// <summary>
    /// Dashboard VM
    /// </summary>
    public class EmployeeDetailsVM
    {
        public bool IsProfileCompletedByEmployee { get; set; } = false;
        public long UserId { get; set; } 
        public long Id { get; set; }
        public string EmployeeCode { get; set; } = null!;
        public string FullName => $"{FirstName} {LastName}";
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string FatherName { get; set; } = null!;
        public string? ProfileImageUrl { get; set; }
        public string NationalCode { get; set; } = null!;
        public DateTime BirthDate { get; set; }
        public string PersianBirthDate => BirthDate.ToShamsi();
        public int Age => (DateTime.Now.Year - BirthDate.Year);
        public Gender Gender { get; set; }
        public string GenderDisplay => Gender == Gender.Male ? "مرد" : "زن";

        public string MilitaryServiceStatus { get; set; }
        public string HomeAddress { get; set; }
    

        public string? MobilePhone { get; set; }
        public string? HomePhone { get; set; }

        public string? IdIssuePlace { get; set; }
        public string? IdNumber { get; set; }
        public string? BirthPlace { get; set; }
        public string? FathersName { get; set; }
        public string? PreviousLastName { get; set; }
        public string? Nickname { get; set; }
        public string? MarriageDatePersian { get; set; }
        public MilitaryStatus MilitaryStatus { get; set; }
        public string? MilitaryStatusDisplay { get; set; }
        public string? ServiceEndDatePersian { get; set; }
        public string? ExemptionReason { get; set; }
        public HousingStatus? HousingStatus { get; set; }
        public string? HousingStatusDisplay { get; set; }
        public string? PersonalAddress { get; set; }
        public string? RentalAddress { get; set; }
        public string? EducationStartYear { get; set; }
        public string? EducationEndYear { get; set; }
        public string? EducationInstituteName { get; set; }
        public string? EducationInstituteCity { get; set; }



        public MaritalStatus? MaritalStatus { get; set; }
        public string MaritalStatusDisplay => MaritalStatus switch
        {
            Domain.Entities.MaritalStatus.Single => "مجرد",
            Domain.Entities.MaritalStatus.Married => "متاهل",
            Domain.Entities.MaritalStatus.Divorced => "مطلقه",
            Domain.Entities.MaritalStatus.Widowed => "همسر فوت شده",
            _ => "نامشخص"
        };

        public int ChildrenCount => FamilyMembers?.Count(x => x.Relation == "فرزند") ?? 0;
        public ICollection<EmployeeFamilyMember> FamilyMembers { get; set; } = new List<EmployeeFamilyMember>();

        public EmployeeEducation? Education { get; set; }
        public ICollection<EmployeeTraining> Trainings { get; set; } = [];
        public ICollection<EmployeeWorkExperience> WorkExperiences { get; set; } = [];
        public ICollection<EmployeeLoan> Loans { get; set; } = [];
        public EmployeeMaritalInfo? MaritalInfo { get; set; }

        public string? Management { get; set; }
        public string? Department { get; set; }
        public string? Unit { get; set; }
        public string? Position { get; set; }
        public EducationLevel? EducationLevel { get; set; }
        public string EducationLevelDisplay => EducationLevel?.ToPersian() ?? "نامشخص";
        public string? FieldOfStudy { get; set; }
        public ContractType? ContractType { get; set; }
        public string ContractTypeDisplay => ContractType switch
        {
            Domain.Entities.ContractType.Permanent => "رسمی",
            Domain.Entities.ContractType.FixedTerm => "پیمانی/قراردادی",
            Domain.Entities.ContractType.DailyWage => "روزمزد",
            Domain.Entities.ContractType.PartTime => "پاره‌وقت",
            Domain.Entities.ContractType.CommissionBased => "کارمزدی",
            _ => "نامشخص"
        };
        public CooperationType? CooperationType { get; set; }
        public string CooperationTypeDisplay => CooperationType switch
        {
            Domain.Entities.CooperationType.FullTime => "تمام‌وقت",
            Domain.Entities.CooperationType.PartTime => "پاره‌وقت",
            Domain.Entities.CooperationType.Consultant => "مشاور",
            Domain.Entities.CooperationType.Volunteer => "داوطلب",
            Domain.Entities.CooperationType.BoardMember => "هیئت امنا",
            _ => "نامشخص"
        };
        public DateTime? HireDate { get; set; }
        public string PersianHireDate => HireDate?.ToShamsi() ?? "وارد نشده";
        public int WorkExperienceYears => HireDate.HasValue ? (DateTime.Now.Year - HireDate.Value.Year) : 0;

        public decimal? BaseSalary { get; set; }
        public decimal? Benefits { get; set; }
        public decimal TotalSalary => (BaseSalary ?? 0) + (Benefits ?? 0);
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? CardNumber { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public string PaymentMethodDisplay => PaymentMethod switch
        {
            Domain.Entities.PaymentMethod.BankTransfer => "انتقال بانکی",
            Domain.Entities.PaymentMethod.Cash => "نقدی",
            Domain.Entities.PaymentMethod.CardToCard => "کارت به کارت",
            _ => "نامشخص"
        };

        public string? InsuranceNumber { get; set; }
        public HealthStatus? HealthStatus { get; set; }
        public string HealthStatusDisplay => HealthStatus switch
        {
            Domain.Entities.HealthStatus.Healthy => "سالم",
            Domain.Entities.HealthStatus.ChronicIllness => "بیماری مزمن",
            Domain.Entities.HealthStatus.MedicalExemption => "معافیت پزشکی",
            _ => "نامشخص"
        };

        public int AnnualLeaveDays { get; set; } = 26;
        public int UsedLeaveDays { get; set; } = 0;
        public int RemainingLeaveDays => AnnualLeaveDays - UsedLeaveDays;

        public int? PerformanceScore { get; set; }

        public RetirementStatus? RetirementStatus { get; set; }
        public DateTime? RetirementDate { get; set; }

        public bool IsActive { get; set; } = true;
        public string? TerminationReason { get; set; }
        public DateTime? TerminationDate { get; set; }

        public string? Notes { get; set; }

        // وضعیت تکمیل هر مرحله
        public bool Step1Completed => !string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(NationalCode);
        public bool Step2Completed => Education != null || Trainings.Any();
        public bool Step3Completed => WorkExperiences.Any();
        public bool Step4Completed => Department is not null && Position is not null && HireDate is not null ;
        public bool Step5Completed => BaseSalary is not null && PaymentMethod is not null; 

        public int ProfileCompletionPercentage
        {
            get
            {
                int total = 5;
                int completed = 0;
                if (Step1Completed) completed++;
                if (Step2Completed) completed++;
                if (Step3Completed) completed++;
                if (Step4Completed) completed++;
                if (Step5Completed) completed++;
                return (completed * 100) / total;
            }
        }
    }

    //--------------------------------



    public class FamilyMemberVM
    {
        public long? Id { get; set; }
        public string FullName { get; set; } = "";
        public string FathersName { get; set; } = "";
        public string PersianBirthDate { get; set; } = "";
        public string Relation { get; set; } = "";
        public string AddressOrWorkplace { get; set; } = "";
    }

    public class EmployeeListItemDTO
    {
        public bool IsInformationConfirmed { get; set; }

        public long Id { get; set; }
        public string EmployeeCode { get; set; } = "";
        public string FullName { get; set; } = "";
        public string? ProfileImageUrl { get; set; }
        public string? Department { get; set; }
        public string? Position { get; set; }
        public bool IsActive { get; set; } = true;

        public CooperationType CooperationType { get; set; }
        public MaritalStatus MaritalStatus { get; set; }

        // وضعیت تکمیل هر مرحله
        public bool Step1Completed { get; set; }
        public bool Step2Completed { get; set; }
        public bool Step3Completed { get; set; }
        public bool Step4Completed { get; set; }
        public bool Step5Completed { get; set; }

        public int ProfileCompletionPercentage => CalculateCompletion();

        private int CalculateCompletion()
        {
            int total = 5;
            int completed = 0;
            if (Step1Completed) completed++;
            if (Step2Completed) completed++;
            if (Step3Completed) completed++;
            if (Step4Completed) completed++;
            if (Step5Completed) completed++;
            return (completed * 100) / total;
        }
    }
}

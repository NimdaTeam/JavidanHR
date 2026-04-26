using System.ComponentModel.DataAnnotations.Schema;
using _0_Framework.EntityBase;

namespace HrSystem.Domain.Entities
{
    [Table("Employees")]
    public class Employee : EntityBase
    {
        public long UserId { get; set; }

        public long? WorkShopId { get; set; }

        public string EmployeeCode { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? FathersName { get; set; }
        public string? Nickname { get; set; }
        public string? PreviousLastName { get; set; }
        public string NationalCode { get; set; } = null!;
        public string? IdNumber { get; set; }           // شماره شناسنامه
        public string? IdIssuePlace { get; set; }       // محل صدور
        public string? BirthPlace { get; set; }         // محل تولد
        public DateTime BirthDate { get; set; }
        public Gender Gender { get; set; }
        public string? MobilePhone { get; set; }        // تلفن همراه
        public string? HomePhone { get; set; }          // تلفن ثابت (جدید)
        public string? ProfileImageUrl { get; set; }

        // === وضعیت تأیید پروفایل ===
        public bool IsProfileCompletedByEmployee { get; set; } = false;
        public bool IsApprovedByAdmin { get; set; } = false;
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }

        // === اطلاعات سازمانی (موقتا اینجا نگه داشتم) ===
        public string? Management { get; set; }
        public string? Department { get; set; }
        public string? Unit { get; set; }
        public string? Position { get; set; }


        public EducationLevel? EducationLevel { get; set; }
        public string? FieldOfStudy { get; set; }
        public ContractType? ContractType { get; set; }
        public CooperationType? CooperationType { get; set; }
        public DateTime? HireDate { get; set; }

        // === اطلاعات مالی ===
        public decimal? BaseSalary { get; set; }
        public decimal? Benefits { get; set; } = 0;
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? CardNumber { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }

        // === بیمه، سلامت، مرخصی، ارزیابی، بازنشستگی ===
        public string? InsuranceNumber { get; set; }
        public HealthStatus? HealthStatus { get; set; }
        public int AnnualLeaveDays { get; set; } = 26;
        public int UsedLeaveDays { get; set; } = 0;
        public int? PerformanceScore { get; set; }
        public RetirementStatus? RetirementStatus { get; set; }
        public DateTime? RetirementDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string? TerminationReason { get; set; }
        public DateTime? TerminationDate { get; set; }

        // === روابط ===
        public EmployeeMilitaryService? MilitaryService { get; set; }
        public EmployeeEducation? Education { get; set; }
        public EmployeeMaritalInfo? MaritalInfo { get; set; }
        public EmployeeAddress? AddressInfo { get; set; }

        public ICollection<EmployeeFamilyMember> FamilyMembers { get; set; } = new List<EmployeeFamilyMember>();
        public ICollection<EmployeeTraining> Trainings { get; set; } = new List<EmployeeTraining>();
        public ICollection<EmployeeWorkExperience> WorkExperiences { get; set; } = new List<EmployeeWorkExperience>();
        public ICollection<EmployeeLoan> Loans { get; set; } = new List<EmployeeLoan>();

        public string GetFullName()
        {
            return $"{FirstName} {LastName}";
        }
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

// CompleteEmployeeProfileVM.cs

using HrSystem.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace HrSystem.Application.DTO
{
    public class CompleteEmployeeProfileVM
    {
        public long EmployeeId { get; set; }

        // مشخصات فردی
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string FathersName { get; set; } = "";
        public string Nickname { get; set; } = "";
        public string PreviousLastName { get; set; } = "";
        public string IdNumber { get; set; } = "";
        public string IdIssuePlace { get; set; } = "";
        public string BirthPlace { get; set; } = "";
        public string NationalCode { get; set; } = "";
        public string PersianBirthDate { get; set; } = "";
        public string MobilePhone { get; set; } = "";
        public string HomePhone { get; set; } = "";
        public Gender Gender { get; set; }

        // نظام وظیفه
        public MilitaryStatus MilitaryStatus { get; set; } = MilitaryStatus.None;
        public string ServiceEndDatePersian { get; set; } = "";
        public string ExemptionReason { get; set; } = "";

        // تحصیلات
        public EducationLevel EducationLevel { get; set; } = EducationLevel.BelowDiploma;
        public string FieldOfStudy { get; set; } = "";
        public int? StartYear { get; set; }
        public int? EndYear { get; set; }
        public string InstituteName { get; set; } = "";
        public string InstituteCity { get; set; } = "";
        public string InstituteAddress { get; set; } = "";

        // وضعیت تأهل
        public MaritalStatus MaritalStatus { get; set; } = MaritalStatus.Single;
        public string MarriageDatePersian { get; set; } = "";

        // آدرس و مسکن
        public HousingStatus HousingStatus { get; set; } = HousingStatus.Personal;
        public string PersonalAddress { get; set; } = "";
        public string RentalAddress { get; set; } = "";

        // جداول
        public List<FamilyMemberVM> FamilyMembers { get; set; } = new();
        public List<TrainingVM> Trainings { get; set; } = new();
        public List<WorkExperienceVM> WorkExperiences { get; set; } = new();
        public List<LoanVM> Loans { get; set; } = new();
    }


    public class TrainingVM
    {
        public long Id { get; set; }
        public string CourseName { get; set; } = "";
        public string Institute { get; set; } = "";
        public int? Hours { get; set; }
        public string CertificateNumberAndDate { get; set; } = "";
        public string Notes { get; set; } = "";
    }

    public class WorkExperienceVM
    {
        public long? Id { get; set; } // برای ویرایش

        [Required(ErrorMessage = "نوع تجربه الزامی است")]
        public WorkExperienceType Type { get; set; }

        [Required(ErrorMessage = "نام سازمان الزامی است")]
        public string Organization { get; set; } = string.Empty;

        [Required(ErrorMessage = "عنوان شغلی الزامی است")]
        public string Position { get; set; } = string.Empty;

        public string? DirectManager { get; set; }

        [Required(ErrorMessage = "تاریخ شروع الزامی است")]
        public string StartDatePersian { get; set; } = string.Empty;

        public string? EndDatePersian { get; set; } // اختیاری (برای شغل فعلی خالی می‌ماند)

        public bool HasInsurance { get; set; } = false;

        public string? TerminationReason { get; set; }

        public string? Notes { get; set; }
    }

    public class LoanVM
    {
        public long Id { get; set; }
        public decimal Amount { get; set; }
        public string BorrowerName { get; set; } = "";
        public string Guarantors { get; set; } = "";
        public string SettlementDatePersian { get; set; } = "";
    }
}
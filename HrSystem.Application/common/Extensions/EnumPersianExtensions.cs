using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HrSystem.Domain.Entities;

namespace HrSystem.Application.common.Extensions
{
    public static class EnumPersianExtensions
    {
        public static string ToPersian(this Gender gender) => gender switch
        {
            Gender.Male => "مرد",
            Gender.Female => "زن",
            _ => gender.ToString()
        };

        public static string ToPersian(this MaritalStatus status) => status switch
        {
            MaritalStatus.Single => "مجرد",
            MaritalStatus.Married => "متاهل",
            MaritalStatus.Divorced => "مطلقه",
            MaritalStatus.Widowed => "همسر فوت‌شده",
            _ => status.ToString()
        };

        public static string ToPersian(this EducationLevel level) => level switch
        {
            EducationLevel.BelowDiploma => "زیردیپلم",
            EducationLevel.Diploma => "دیپلم",
            EducationLevel.Associate => "کاردانی",
            EducationLevel.Bachelor => "کارشناسی",
            EducationLevel.Master => "کارشناسی ارشد",
            EducationLevel.Doctorate => "دکتری",
            _ => level.ToString()
        };

        public static string ToPersian(this ContractType type) => type switch
        {
            ContractType.Permanent => "رسمی",
            ContractType.FixedTerm => "قراردادی/پیمانی",
            ContractType.DailyWage => "روزمزد",
            ContractType.PartTime => "پاره‌وقت",
            ContractType.CommissionBased => "کارمزدی",
            _ => type.ToString()
        };

        public static string ToPersian(this CooperationType type) => type switch
        {
            CooperationType.FullTime => "تمام‌وقت",
            CooperationType.PartTime => "پاره‌وقت",
            CooperationType.Consultant => "مشاور",
            CooperationType.Volunteer => "داوطلب",
            CooperationType.BoardMember => "هیئت امنا",
            _ => type.ToString()
        };

        public static string ToPersian(this HealthStatus status) => status switch
        {
            HealthStatus.Healthy => "سالم",
            HealthStatus.ChronicIllness => "بیماری خاص",
            HealthStatus.MedicalExemption => "معافیت پزشکی",
            _ => status.ToString()
        };

        public static string ToPersian(this RetirementStatus status) => status switch
        {
            RetirementStatus.Active => "در حال کار",
            RetirementStatus.NearRetirement => "در شرف بازنشستگی",
            RetirementStatus.Retired => "بازنشسته",
            _ => status.ToString()
        };

        public static string ToPersian(this PaymentMethod method) => method switch
        {
            PaymentMethod.BankTransfer => "انتقال به حساب",
            PaymentMethod.Cash => "نقدی",
            PaymentMethod.CardToCard => "کارت به کارت",
            _ => method.ToString()
        };

        public static string ToPersian(this HousingStatus method) => method switch
        {
            HousingStatus.Personal => "ملکی",
            HousingStatus.Rental => "استیجاری",
            _ => method.ToString()
        }; 


        public static string ToPersian(this MilitaryStatus method) => method switch
        {
            MilitaryStatus.Ended => "پایان یافته",
            MilitaryStatus.Exempt => "معاف از خدمت",
            MilitaryStatus.None => "-",
            _ => method.ToString()
        };
    }
}

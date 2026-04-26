using DNTPersianUtils.Core;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace _0_Framework.Utilities.Helpers
{
    public static class DateHelper
    {
        public static string ToShamsi(this DateTime value)
        {
            if (value.Year < 622)
                return "-";

            PersianCalendar pc = new PersianCalendar();
            return pc.GetYear(value) + "/" + pc.GetMonth(value).ToString("00") + "/" +
                   pc.GetDayOfMonth(value).ToString("00");
        }

        public static string ToShamsi(this DateTime? value)
        {
            if (value is null)
                return "-";

            if (value.Value.Year < 622)
                return "-";

            PersianCalendar pc = new PersianCalendar();
            return pc.GetYear(value.Value) + "/" + pc.GetMonth(value.Value).ToString("00") + "/" +
                   pc.GetDayOfMonth(value.Value).ToString("00");
        }


        public static DateTime? ToGregorian(this string value)
        {
            if(string.IsNullOrWhiteSpace(value))
                return null;

            var dateParts = value.Split("/");

            PersianCalendar pc = new PersianCalendar();
            return pc.ToDateTime(int.Parse(dateParts[0]), int.Parse(dateParts[1]), int.Parse(dateParts[2]),0,0,0,0);
        }


        public static string ToShamsiWithTime(this DateTime value)
        {
            if (value.Year < 622)
                return "-";

            PersianCalendar pc = new PersianCalendar();
            return pc.GetYear(value) + "/" + pc.GetMonth(value).ToString("00") + "/" +
                   pc.GetDayOfMonth(value).ToString("00") + " - " +
                   pc.GetMinute(value).ToString("00") + " : " + pc.GetHour(value).ToString("00");
        }
        public static int GetMonthOfDate(this DateTime value)
        {
            PersianCalendar pc = new PersianCalendar();
            return pc.GetMonth(value);
        }

        public static int GetYearOfDate(this DateTime value)
        {
            PersianCalendar pc = new PersianCalendar();
            return pc.GetYear(value);
        }


        public static string ToShamsiWithTime(this DateTime? date)
        {
            if (date is null)
                return "-";

            if (date.Value.Year < 622)
                return "-";

            PersianCalendar pc = new PersianCalendar();
            DateTime value = date.GetValueOrDefault();
            return pc.GetYear(value) + "/" + pc.GetMonth(value).ToString("00") + "/" +
                   pc.GetDayOfMonth(value).ToString("00") + " - " +
                   pc.GetMinute(value).ToString("00") + " : " + pc.GetHour(value).ToString("00");
        }

        public static string ToShamsiWithoutDate(this DateTime? date)
        {
            if (date is null)
                return "-";

            if (date.Value.Year < 622)
                return "-";

            PersianCalendar pc = new PersianCalendar();
            DateTime value = date.GetValueOrDefault();
            return pc.GetSecond(value).ToString("00") + " : " + pc.GetMinute(value).ToString("00") + " : " + pc.GetHour(value).ToString("00");
        }

        public static string GetStartOfDate(this DateTime? date)
        {
            if (date is null)
                return "";

            // اگر سال تاریخ وارد شده کمتر از 622 باشد، خروجی به شکل "-"
            if (date.GetValueOrDefault().Year < 622)
                return "";

            // شروع روز: 00:00:00
            return new DateTime(date.GetValueOrDefault().Year, date.GetValueOrDefault().Month, date.GetValueOrDefault().Day, 0, 0, 0, DateTimeKind.Unspecified)
                .ToString("yyyy-MM-ddTHH:mm:ss");
        }

        public static string GetEndOfDate(this DateTime? date)
        {
            if (date is null)
                return "-";

            // اگر سال تاریخ وارد شده کمتر از 622 باشد، خروجی به شکل "-"
            if (date.GetValueOrDefault().Year < 622)
                return "";

            // پایان روز: 23:59:59
            return new DateTime(date.GetValueOrDefault().Year, date.GetValueOrDefault().Month, date.GetValueOrDefault().Day, 23, 59, 59, DateTimeKind.Unspecified)
                .ToString("yyyy-MM-ddTHH:mm:ss");
        }


        public static string GetStartOfDate(this DateTime date)
        {
            // اگر سال تاریخ وارد شده کمتر از 622 باشد، خروجی به شکل "-"
            if (date.Year < 622)
                return "";

            // شروع روز: 00:00:00
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Unspecified)
                .ToString("yyyy-MM-ddTHH:mm:ss");
        }

        public static string GetEndOfDate(this DateTime date)
        {
            // اگر سال تاریخ وارد شده کمتر از 622 باشد، خروجی به شکل "-"
            if (date.Year < 622)
                return "";

            // پایان روز: 23:59:59
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, DateTimeKind.Unspecified)
                .ToString("yyyy-MM-ddTHH:mm:ss");
        }

        public static List<DateEntity> GetLast20Years()
        {
            List<DateEntity> dateList = new List<DateEntity>();

            PersianCalendar pc = new PersianCalendar();

            var currentDate = DateTime.Today;

            var currentYear = pc.GetYear(currentDate);

            dateList.Add(new DateEntity(currentYear.ToString(), currentYear.ToString()));

            currentYear--;

            for (int i = 0; i < 20; i++)
            {
                dateList.Add(new DateEntity(currentYear.ToString(), currentYear.ToString()));

                currentYear--;
            }

            return dateList;
        }

        public static List<DateEntity> GetMonthsOfYear()
        {
            return
            [
               new DateEntity("فروردین", "01"),
               new DateEntity("اردیبهشت", "02"),
               new DateEntity("خرداد", "03"),
               new DateEntity("تیر", "04"),
               new DateEntity("مرداد", "05"),
               new DateEntity("شهریور", "06"),
               new DateEntity("مهر", "07"),
               new DateEntity("آبان", "08"),
               new DateEntity("آذر", "09"),
               new DateEntity("دی", "10"),
               new DateEntity("بهمن", "11"),
               new DateEntity("اسفند", "12"),
            ];
        }

        public static bool IsValidPersianDate(this string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            try
            {
                value.TryParsePersianDateToDateTimeOrDateTimeOffset(out bool result);
                return result;
            }
            catch
            {
                return false;
            }
        }

        public static string ToHourMinuteFormat(this int? total)
        {
            if (total is null)
                return "-";

            var hours = total / 60;
            var minutes = total % 60;

            return $"{hours:00}:{minutes:00}";
        }

        public static string ToHourMinuteFormat(this int total)
        {
            var hours = total / 60;
            var minutes = total % 60;

            return $"{hours:00}:{minutes:00}";
        }

        public static string ToHourMinuteFormat(this double total)
        {

            var hours = total / 60;
            var minutes = total % 60;

            return $"{hours:00}:{minutes:00}";
        }
    }

    public sealed class DateEntity(string title, string value)
    {
        public string EntityTitle { get; set; } = title;

        public string EntityValue { get; set; } = value;
    }
}

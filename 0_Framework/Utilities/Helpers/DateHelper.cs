using System.Globalization;

namespace _0_Framework.Utilities.Helpers
{
    public static class DateHelper
    {
        public static string ToShamsi(this DateTime value)
        {
            PersianCalendar pc = new PersianCalendar();
            return pc.GetYear(value) + "/" + pc.GetMonth(value).ToString("00") + "/" +
                   pc.GetDayOfMonth(value).ToString("00");
        }

        public static string ToShamsi(this DateTime? value)
        {
            PersianCalendar pc = new PersianCalendar();
            return pc.GetYear(value.Value) + "/" + pc.GetMonth(value.Value).ToString("00") + "/" +
                   pc.GetDayOfMonth(value.Value).ToString("00");
        }

        public static string ToShamsiWithTime(this DateTime value)
        {
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
            PersianCalendar pc = new PersianCalendar();
            DateTime value = date.GetValueOrDefault();
            return pc.GetYear(value) + "/" + pc.GetMonth(value).ToString("00") + "/" +
                   pc.GetDayOfMonth(value).ToString("00") + " - " +
                   pc.GetMinute(value).ToString("00") + " : " + pc.GetHour(value).ToString("00");
        }

        public static List<DateEntity> GetLast20Years()
        {
            List<DateEntity> dateList = new List<DateEntity>();

            PersianCalendar pc = new PersianCalendar();

            var currentDate = DateTime.Today;

            var currentYear = pc.GetYear(currentDate);

            dateList.Add(new DateEntity(currentYear.ToString(),currentYear.ToString()));

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
               new DateEntity("فروردین", "1"),
               new DateEntity("اردیبهشت", "2"),
               new DateEntity("خرداد", "3"),
               new DateEntity("تیر", "4"),
               new DateEntity("مرداد", "5"),
               new DateEntity("شهریور", "6"),
               new DateEntity("مهر", "7"),
               new DateEntity("آبان", "8"),
               new DateEntity("آذر", "9"),
               new DateEntity("دی", "10"),
               new DateEntity("بهمن", "11"),
               new DateEntity("اسفند", "12"),
            ];
        }
    }

    public sealed class DateEntity(string title,string value)
    {
        public string EntityTitle { get; set; } = title;

        public string EntityValue { get; set; } = value;
    }
}

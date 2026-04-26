using Ganss.Xss;
using System.Reflection;

namespace _0_Framework.Utilities.Security
{
    public static class StringSanitizer
    {
        private static readonly char[] PersianDigits = ['۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹'];
        private static readonly char[] EnglishDigits = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];

        public static string SanitizeString(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;


            var sanitizer = new HtmlSanitizer();

            sanitizer.AllowedTags.Clear();
            sanitizer.AllowedTags.Add("b");
            sanitizer.AllowedTags.Add("i");
            sanitizer.AllowedTags.Add("p");
            sanitizer.AllowedTags.Add("br");

            sanitizer.AllowedAttributes.Clear();

            #region Convert arabic and persian characters
            // تبدیل اعداد فارسی به انگلیسی
            for (int i = 0; i < PersianDigits.Length; i++)
            {
                text = text.Replace(PersianDigits[i], EnglishDigits[i]);
            }

            // تبدیل کاراکترهای عربی به فارسی استاندارد
            text =  text.Replace('ي', 'ی')   // ي عربی → ی فارسی
                .Replace('ك', 'ک')   // ك عربی → ک فارسی
                .Replace('ة', 'ه')   // ة → ه
                .Replace('ؤ', 'و')   // مؤ → و
                .Replace('إ', 'ا')   // إ → ا
                .Replace('أ', 'ا')   // أ → ا
                .Replace('آ', 'ا')   // آ → ا
                .Replace('ء', 'ئ');  
            #endregion

            var sanitizedString = sanitizer.Sanitize(text);

            return sanitizedString;
        }

        public static T Sanitize<T>(this T obj) where T : class, new()
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            // ایجاد یک نمونه جدید از نوع T
            T sanitizedObj = new T();

            // پیمایش روی پراپرتی‌های عمومی
            foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                // فقط پراپرتی‌هایی که قابل خواندن و نوشتن هستند
                if (property.CanRead && property.CanWrite)
                {
                    var value = property.GetValue(obj);

                    // اگر پراپرتی از نوع string باشد
                    if (property.PropertyType == typeof(string) && value != null)
                    {
                        // سsanitize کردن مقدار رشته‌ای
                        var sanitizedValue = ((string)value).SanitizeString();
                        property.SetValue(sanitizedObj, sanitizedValue);
                    }
                    else
                    {
                        // کپی مقدار بدون تغییر برای پراپرتی‌های غیررشته‌ای
                        property.SetValue(sanitizedObj, value);
                    }
                }
            }

            return sanitizedObj;
        }
    }
}

using Ganss.Xss;
using System.Reflection;

namespace _0_Framework.Utilities.Security
{
    public static class StringSanitizer
    {
        public static string SanitizeString(this string text)
        {
            var sanitizer = new HtmlSanitizer();

            sanitizer.AllowedTags.Clear();
            sanitizer.AllowedTags.Add("b");
            sanitizer.AllowedTags.Add("i");
            sanitizer.AllowedTags.Add("p");
            sanitizer.AllowedTags.Add("br");

            sanitizer.AllowedAttributes.Clear();

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

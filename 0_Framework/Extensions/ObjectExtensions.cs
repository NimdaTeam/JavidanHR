// مسیر: _0_Framework/Extensions/ObjectExtensions.cs

using System.Text.Json;

namespace _0_Framework.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// تبدیل هر شیء به نوع دلخواه با استفاده از Json (سریع، بدون AutoMapper)
        /// </summary>
        public static T MapTo<T>(this object source) where T : class, new()
        {
            if (source == null) return null;

            var json = System.Text.Json.JsonSerializer.Serialize(source);
            return System.Text.Json.JsonSerializer.Deserialize<T>(json)!;
        }

        /// <summary>
        /// نسخه با تنظیمات دلخواه (در صورت نیاز)
        /// </summary>
        public static T MapTo<T>(this object source, JsonSerializerOptions options) where T : class, new()
        {
            if (source == null) return null;

            var json = System.Text.Json.JsonSerializer.Serialize(source, options);
            return System.Text.Json.JsonSerializer.Deserialize<T>(json, options)!;
        }
    }
}
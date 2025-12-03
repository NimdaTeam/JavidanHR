using System.Linq.Expressions;

namespace WebHost.Helpers.GlobalHelpers
{
    public static class SearchHelper
    {
        public static IEnumerable<T> Search<T>(
            IEnumerable<T> data,
            string searchQuery,
            params Expression<Func<T, string?>>[] propertySelectors)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return data; // اگر جستجو خالی بود، همان لیست را برگردان

            // برای هماهنگی بیشتر
            searchQuery = searchQuery.Trim();

            // پیاده‌سازی جستجو با Any روی propertySelectors
            return data.Where(item =>
                propertySelectors.Any(selector =>
                {
                    // بدست آوردن مقدار رشته فیلد مربوطه
                    var value = selector.Compile().Invoke(item);
                    // بررسی اینکه آیا جستجو بخشی از مقدار فیلد هست یا نه
                    return !string.IsNullOrEmpty(value)
                           && value.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0;
                })
            );
        }
    }
}

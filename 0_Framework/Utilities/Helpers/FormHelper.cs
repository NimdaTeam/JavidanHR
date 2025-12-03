using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace _0_Framework.Utilities.Helpers
{
    public static class FormHelper
    {
        public static Dictionary<string, string> GetQuestionTitles(string formJson)
        {
            var titles = new Dictionary<string, string>();
            try
            {
                var jsonDoc = JsonDocument.Parse(formJson);
                var root = jsonDoc.RootElement;

                // بررسی وجود "pages"
                if (root.TryGetProperty("pages", out var pages))
                {
                    foreach (var page in pages.EnumerateArray())
                    {
                        // بررسی وجود "elements"
                        if (page.TryGetProperty("elements", out var elements))
                        {
                            foreach (var element in elements.EnumerateArray())
                            {
                                // استخراج "name" و "title"
                                if (element.TryGetProperty("name", out var name))
                                {
                                    var key = name.GetString();
                                    var titleText = element.TryGetProperty("title", out var title)
                                        ? title.GetString()
                                        : key; // استفاده از name اگر title وجود نداشته باشد

                                    if (!string.IsNullOrEmpty(key))
                                    {
                                        titles[key] = string.IsNullOrEmpty(titleText) ? key : titleText;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // در صورت خطا، دیکشنری خالی برمی‌گردانیم
                Console.WriteLine($"Error parsing form JSON: {ex.Message}");
            }
            return titles;
        }
    }
}

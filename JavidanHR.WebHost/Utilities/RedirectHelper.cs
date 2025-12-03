using Microsoft.AspNetCore.Mvc;

namespace WebHost.Utilities
{
    public static class RedirectHelper
    {
        /// <summary>
        /// ریدایرکت به آدرس Referrer اگر معتبر باشد، در غیر این صورت به آدرس پیش‌فرض.
        /// </summary>
        /// <param name="controller">کنترلری که متد از آن فراخوانی می‌شود</param>
        /// <param name="defaultRedirectUrl">آدرس پیش‌فرض برای ریدایرکت در صورت نامعتبر بودن Referrer</param>
        /// <returns>IActionResult برای ریدایرکت</returns>
        public static IActionResult RedirectToReferrer(this ControllerBase controller, string defaultRedirectUrl = "/")
        {
            var referrer = controller.Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referrer) &&
                Uri.TryCreate(referrer, UriKind.Absolute, out var uri) &&
                uri.Host == controller.Request.Host.Host)
            {
                return controller.Redirect(referrer);
            }

            return controller.Redirect(defaultRedirectUrl);
        }
    }
}

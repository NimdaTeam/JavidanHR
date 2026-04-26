using _0_Framework.Utilities.NotificationSystem;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace JavidanHR.WebHost.Controllers
{
    public class BaseController : Controller
    {
        protected IActionResult SmartRedirect(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            var referer = Request.Headers["Referer"].ToString();

            if (!string.IsNullOrEmpty(referer) && Url.IsLocalUrl(referer))
            {
                return Redirect(referer);
            }

            return Redirect("/");
        }

        public void ShowNotification(string title, string message = "", string type = ApplicationMessagesIcon.SuccessIcon)
        {
            NotificationSystem.ShowNotification(TempData, title, message, type);
        }

        public string CurrentUsername()
        {
            var username = User?.Identity?.Name ?? "-";
            return username;
        }
    }
}
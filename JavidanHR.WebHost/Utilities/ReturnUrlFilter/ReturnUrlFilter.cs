using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace JavidanHR.WebHost.Utilities.ReturnUrlFilter
{
    public class ReturnUrlFilter : IActionFilter
    {
        private readonly IRequestContextAccessor _ctx;

        public ReturnUrlFilter(IRequestContextAccessor ctx)
        {
            _ctx = ctx;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;

            var returnUrl = request.Query["returnUrl"].ToString();

            if (string.IsNullOrEmpty(returnUrl))
            {
                var referer = request.Headers["Referer"].ToString();
                returnUrl = referer;
            }

            _ctx.Context.ReturnUrl = returnUrl;

            // فقط GET
            if (request.Method != "GET")
                return;

            var currentUrl = request.Path + request.QueryString;

            // جلوگیری از loop
            if (currentUrl.Contains("returnUrl="))
                currentUrl = request.Path;

            // تزریق به ViewData (نه ViewBag)
            if (context.Controller is Controller controller)
            {
                controller.ViewData["ReturnUrl"] = currentUrl;
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}

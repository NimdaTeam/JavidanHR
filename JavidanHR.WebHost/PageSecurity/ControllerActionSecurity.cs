using System.Text.Json;
using _0_Framework.Utilities.NotificationSystem;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using AuthenticationSystem.Services.Repositories;
using AuthenticationSystem.SystemPermissions;
using WebHost.Utilities;

namespace WebHost.PageSecurity
{
    public class PermissionAttribute : ActionFilterAttribute
    {
        private readonly SystemPermissions.PermissionList _requiredPermission;

        public PermissionAttribute(SystemPermissions.PermissionList requiredPermission)
        {
            _requiredPermission = requiredPermission;
        }

        public override async void OnActionExecuting(ActionExecutingContext context)
        {
            var roleService =
                (IRoleRepository)context.HttpContext.RequestServices.GetRequiredService(typeof(IRoleRepository));

            //var userService =
            //    (IUserRepository)context.HttpContext.RequestServices.GetRequiredService(typeof(IUserRepository));

            //var redisService =
            //    (RedisService)context.HttpContext.RequestServices.GetRequiredService(typeof(RedisService));

            if (!roleService.IsUserAuthenticated().Result)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            //var user = await userService.GetUserByPhoneNumber(context.HttpContext.User.Identity.Name);

            //if (user is null)
            //{
            //    context.Result = new UnauthorizedResult();
            //    return;
            //}

            //var permissionsOnRedis = redisService.GetStringAsync(user.Id.ToString());
            var userPermissions = await roleService.GetLoggedInUserPermissions();
            //var userPermissions = new List<long>();

            //userPermissions = contextPermissions;

            //else
            //{
            //    userPermissions =  roleService!.GetUserPermissions(user.Id).Result.Select(x=>x.Permission).ToList();
            //}

            if (userPermissions.All(p => p != (long)_requiredPermission))
            {
                if (context.Controller is Controller controller)
                {
                    NotificationSystem
                        .ShowNotification(controller.TempData, "دسترسی غیر مجاز", "دسترسی شما به این قسمت محدود شده است", ApplicationMessagesIcon.ErrorIcon);
                }

                context.Result = new RedirectResult("/");
            }

        }
    }
}
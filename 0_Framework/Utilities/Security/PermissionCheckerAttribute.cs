//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc.Filters;

//namespace _0_Framework.Utilities.Security
//{
//	public class PermissionCheckerAttribute : AuthorizeAttribute, IAuthorizationFilter
//	{
//		private UserService _userService;
//		// private int _permissionId = 0;
//		private int _roleId;

//		public PermissionCheckerAttribute(int role)
//		{
//			_roleId = role;
//		}
//		//public PermissionCheckerAttribute(int permissionId)
//		//{
//		//    _permissionId = permissionId;
//		//}

//		//public void OnAuthorization(AuthorizationFilterContext context)
//		//{
//		//    _permissionService =
//		//        (IPermissionService) context.HttpContext.RequestServices.GetService(typeof(IPermissionService));
//		//    if (context.HttpContext.User.Identity.IsAuthenticated)
//		//    {
//		//        string userName = context.HttpContext.User.Identity.Name;

//		//        if (!_permissionService.CheckPermission(_permissionId, userName))
//		//        {
//		//            context.Result = new RedirectResult("/Login");
//		//        }
//		//    }
//		//    else
//		//    {
//		//        context.Result = new RedirectResult("/Login");
//		//    }
//		//}

//		public void OnAuthorization(AuthorizationFilterContext context)
//		{
//			_userService =
//					(UserService)context.HttpContext.RequestServices.GetService(typeof(IUserRepository));
//			if (context.HttpContext.User.Identity.IsAuthenticated)
//			{
//				string userName = context.HttpContext.User.Identity.Name;

//				if (!_userService.CheckRole(userName, _roleId))
//				{
//					if (!_userService.CheckRole(userName, 1))
//					{
//						//context.Result = new RedirectResult("/Login");

//						context.HttpContext.Response.Redirect("/login");
//					}

//				}
//			}
//			else
//			{
//				context.HttpContext.Response.Redirect("/login");
//			}
//		}
//	}
//}

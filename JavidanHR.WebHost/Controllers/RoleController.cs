using _0_Framework.DTO;
using _0_Framework.Utilities.NotificationSystem;
using _0_Framework.Utilities.Pagination;
using _0_Framework.Utilities.Security;
using AuthenticationSystem.Infrastructure;
using AuthenticationSystem.Services.Repositories;
using AuthenticationSystem.SystemPermissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebHost.Helpers.GlobalHelpers;
using WebHost.PageSecurity;

namespace JavidanHR.WebHost.Controllers
{
    [Authorize]
    public class RoleController : Controller
    {
        private readonly IRoleRepository _roleService;
        private readonly IUserRepository _userService;

        public RoleController(IRoleRepository roleService, IUserRepository userService)
        {
            _roleService = roleService;
            _userService = userService;
        }

        [Route("AllRoles")]
        [Permission(SystemPermissions.PermissionList.RolesList)]
        public async Task<IActionResult> AllRoles(string searchQuery = "", int page = 1)
        {
            var roles = await _roleService.GetRolesForRolesGrid();

            roles = SearchHelper.Search(
                roles,
                searchQuery.SanitizeString(),
                x => x.Name
            ).ToList();

            var pageModel = PaginationHelper.Paginate(new PaginationRequest<RoleViewModel>()
            {
                SearchQuery = searchQuery.SanitizeString(),
                CurrentPage = page,
                ModelList = roles
            });

            return View(pageModel);
        }

        [Route("AddRole")]
        [Permission(SystemPermissions.PermissionList.AddNewRole)]
        public IActionResult AddRole()
        {
            var permissions = AuthenticationSystemBootstrapper
                .SystemPermissionExposer();

            ViewBag.Permissions = permissions;

            return PartialView();
        }

        [Route("AddRole")]
        [Permission(SystemPermissions.PermissionList.AddNewRole)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRole(string roleName, List<long> permissions)
        {
            if (await _roleService.IsRoleNameDuplicate(roleName.SanitizeString()))
            {
                NotificationSystem
                    .ShowNotification(TempData, ApplicationMessages.DuplicateValueTitle, ApplicationMessages.DuplicateValueDescription, ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("AllRoles");
            }

            try
            {
                var role = await _roleService.CreateNewRole(roleName.SanitizeString(), _userService.GetUserByPhoneNumber(User.Identity!.Name!).Id);

                await _roleService.AddPermissionsToRole(permissions, role.Id);

                NotificationSystem
                    .ShowNotification(TempData, ApplicationMessages.OperationSuccessful, "", ApplicationMessagesIcon.SuccessIcon);
                return RedirectToAction("AllRoles");
            }
            catch (Exception e)
            {
                NotificationSystem
                    .ShowNotification(TempData, ApplicationMessages.OperationFailed, "", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("AllRoles");
            }
        }


        [Route("UpdateRole/{roleId}")]
        [Permission(SystemPermissions.PermissionList.UpdateRole)]
        public async Task<IActionResult> UpdateRole(long roleId)
        {
            var role = await _roleService.GetAsNoTrackingAsync(roleId);

            if (role is null)
            {
                NotificationSystem
                    .ShowNotification(TempData, "عملیات ناموفق", "رکوردی با این مشخصات یافت نشد", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("AllRoles");
            }

            ViewBag.permissions = await _roleService.GetPermissionsForRoleEdit(roleId);
            return PartialView(role);
        }

        [Route("UpdateRole/{roleId}")]
        [Permission(SystemPermissions.PermissionList.UpdateRole)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(long roleId, string roleName, List<long> permissions)
        {
            var role = await _roleService.GetAsync(roleId);

            if (role is null)
            {
                NotificationSystem
                    .ShowNotification(TempData, "عملیات ناموفق", "رکوردی با این مشخصات یافت نشد", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("AllRoles");
            }

            if (await _roleService.IsRoleNameDuplicateForEdit(roleId, roleName))
            {
                NotificationSystem
                    .ShowNotification(TempData, ApplicationMessages.DuplicateValueTitle, ApplicationMessages.DuplicateValueDescription, ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("AllRoles");
            }

            var operation = await _roleService
                .UpdateRoleInformation(roleId, roleName, permissions);

            if (operation.IsSuccessful)
            {
                NotificationSystem
                    .ShowNotification(TempData, ApplicationMessages.OperationSuccessful, "", ApplicationMessagesIcon.SuccessIcon);
            }
            else
            {
                NotificationSystem
                    .ShowNotification(TempData, ApplicationMessages.OperationFailed, "", ApplicationMessagesIcon.ErrorIcon);
            }

            return RedirectToAction("AllRoles");
        }


        [Route("DeleteRole/{roleId}")]
        [Permission(SystemPermissions.PermissionList.DeleteRole)]
        public async Task<IActionResult> DeleteRole(long roleId)
        {
            var role = await _roleService.GetAsync(roleId);

            if (role is null)
            {
                NotificationSystem
                    .ShowNotification(TempData, "عملیات ناموفق", "رکوردی با این مشخصات یافت نشد", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("AllRoles");
            }


            if (role.Name is "پیش فرض")
            {
                NotificationSystem
                    .ShowNotification(TempData, "عملیات ناموفق", "سطح دسترسی پیش فرض قابل حذف شدن نیست", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("AllRoles");
            }

            try
            {
                if (await _roleService.DoesRoleHasAnyUsers(roleId))
                    await _roleService.MigrateAllUsersToDefaultRole(roleId);

                if (await _roleService.DoesRoleHasAnyPermissions(roleId))
                    await _roleService.DeleteAllRolePermissions(roleId);

                role.SoftDelete();

                await _roleService.UpdateAsync(role);
                await _roleService.SaveChangesAsync();

                NotificationSystem
                    .ShowNotification(TempData, ApplicationMessages.OperationSuccessful, "", ApplicationMessagesIcon.SuccessIcon);


                return RedirectToAction("AllRoles");
            }
            catch (Exception e)
            {
                NotificationSystem
                    .ShowNotification(TempData, ApplicationMessages.OperationFailed, "", ApplicationMessagesIcon.ErrorIcon);


                return RedirectToAction("AllRoles");
            }
        }

    }
}

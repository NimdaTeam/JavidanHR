using _0_Framework.DTO;
using _0_Framework.Utilities.Helpers;
using _0_Framework.Utilities.NotificationSystem;
using _0_Framework.Utilities.Pagination;
using _0_Framework.Utilities.Security;
using AuthenticationSystem.Domain.User;
using AuthenticationSystem.Services.Repositories;
using AuthenticationSystem.SystemPermissions;
using AuthenticationSystem.Utilities;
using JavidanHR.WebHost.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebHost.Helpers.GlobalHelpers;
using WebHost.PageSecurity;
using WebHost.Utilities;
using static AuthenticationSystem.Utilities.PasswordSecurity;

namespace JavidanHR.WebHost.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserRepository _userService;

        private readonly IRoleRepository _roleService;

        public UserController(IUserRepository userService, IRoleRepository roleService)
        {
            _userService = userService;
            _roleService = roleService;
        }

        [Route("AllUsers")]
        [Permission(SystemPermissions.PermissionList.UsersList)]
        public async Task<IActionResult> AllUsers(string searchQuery = "", int page = 1)
        {
            var users = await _userService.GetAllUsersForGrid();

            //search 
            users = SearchHelper.Search(
                users,
                searchQuery.SanitizeString(),
                x => x.Fullname,
                x => x.PhoneNumber
                ).ToList();



            var paginationResult = PaginationHelper.Paginate(new PaginationRequest<AllUsersViewModel>()
            {
                ModelList = users,
                CurrentPage = page,
                SearchQuery = searchQuery.SanitizeString()
            });

            var currentUser = await _userService.GetUserByPhoneNumber(User);
            if (currentUser?.Id != null) ViewBag.currentUserId = currentUser.Id;

            return View(paginationResult);
        }

        [Route("AddUser")]
        [Permission(SystemPermissions.PermissionList.AddNewUser)]
        public async Task<IActionResult> AddUser()
        {
            ViewBag.roles = await _roleService.GetAllWithIncludesAsync();

            return PartialView();
        }

        [Route("AddUser")]
        [HttpPost]
        [Permission(SystemPermissions.PermissionList.AddNewUser)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(Users user, List<long> roles)
        {
            if (await _userService.IsPhoneNumberDuplicate(user.PhoneNumber))
            {
                NotificationSystem
                    .ShowNotification(TempData, ApplicationMessages.DuplicateValueTitle, ApplicationMessages.DuplicateValueDescription, ApplicationMessagesIcon.ErrorIcon);

                return RedirectToAction("AllUsers");
            }

            try
            {
                user.IsActive = true;
                var createdUser = await _userService.CreateNewUser(user);

                await _roleService.AddRoleToUser(roles, createdUser);

                NotificationSystem
                    .ShowNotification(TempData, ApplicationMessages.OperationSuccessful, "", ApplicationMessagesIcon.SuccessIcon);

                return RedirectToAction("AllUsers");
            }
            catch (Exception e)
            {
                NotificationSystem
                    .ShowNotification(TempData, ApplicationMessages.OperationFailed, "", ApplicationMessagesIcon.ErrorIcon);

                return RedirectToAction("AllUsers");
            }
        }

        [Route("UpdateUser/{userId}")]
        [Permission(SystemPermissions.PermissionList.UpdateUser)]
        public async Task<IActionResult> UpdateUser(long userId)
        {
            var user = await _userService.GetAsNoTrackingAsync(userId);

            ViewBag.roles = await _roleService.GetUserRolesForEditUser(userId);

            return PartialView(user);
        }

        [Route("UpdateUser/{userId}")]
        [Permission(SystemPermissions.PermissionList.UpdateUser)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUser(Users user, List<long> roles)
        {
            if (await _userService.IsPhoneNumberDuplicateForEdit(user.Id, user.PhoneNumber))
            {
                NotificationSystem
                    .ShowNotification(TempData, ApplicationMessages.DuplicateValueTitle, ApplicationMessages.DuplicateValueDescription, ApplicationMessagesIcon.ErrorIcon);

                return RedirectToAction("AllUsers");
            }

            try
            {
                await _roleService.RemoveRolesFromUser(user.Id);
                await _userService.UpdateAsync(user);
                await _userService.SaveChangesAsync();

                await _roleService.AddRoleToUser(roles, user.Id);

                NotificationSystem
                    .ShowNotification(TempData, ApplicationMessages.OperationSuccessful, "", ApplicationMessagesIcon.SuccessIcon);
                return RedirectToAction("AllUsers");
            }
            catch (Exception e)
            {
                NotificationSystem
                    .ShowNotification(TempData, ApplicationMessages.OperationFailed, "", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("AllUsers");
            }
        }

        [Route("DeleteUser/{userId}")]
        [Permission(SystemPermissions.PermissionList.DeleteUser)]
        public async Task<IActionResult> DeleteUser(long userId)
        {
            try
            {
                var user = await _userService.GetAsync(userId);
                if (user == null)
                {
                    NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }

                await _roleService.RemoveRolesFromUser(userId);

                user.SoftDelete();
                await _userService.UpdateAsync(user);

                await _userService.SaveChangesAsync();

                NotificationSystem
                    .ShowNotification(TempData, ApplicationMessages.OperationSuccessful, "", ApplicationMessagesIcon.SuccessIcon);
                return RedirectToAction("AllUsers");
            }
            catch (Exception e)
            {
                NotificationSystem
                    .ShowNotification(TempData, ApplicationMessages.OperationFailed, "", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("AllUsers");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckUserInfo()
        {
            var user = await _userService.GetUserByPhoneNumber(User.Identity!.Name!);
            if (user == null)
            {
                return Json(new { isComplete = false, redirectUrl = "/" });
            }

            bool isComplete = await _userService.IsUserInfoComplete(user.Id);
            string? redirectUrl = isComplete ? "" : Url.Action("EditUserInfo", "User", new { userId = user.Id });

            return Json(new { isComplete, redirectUrl });
        }

        [Route("EditUserInfo/{userId}")]
        public async Task<IActionResult> EditUserInfo(long userId)
        {
            var user = await _userService.GetAsNoTrackingAsync(userId);
            if (user == null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                return this.RedirectToReferrer();
            }

            if (await _userService.IsUserInfoComplete(userId))
            {
                return this.RedirectToReferrer();
            }

            return View(user);
        }

        [Route("EditUserInfo/{userId}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUserInfo(Users user)
        {
            var foundUser = await _userService.GetAsync(user.Id);
            if (foundUser == null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                return this.RedirectToReferrer();
            }

            if (string.IsNullOrWhiteSpace(user.NationalCode) || string.IsNullOrWhiteSpace(user.FullName))
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.MalformedInput, "", ApplicationMessagesIcon.ErrorIcon);
                return this.RedirectToReferrer();
            }
            if (await _userService.IsPhoneNumberDuplicateForEdit(user.Id, user.PhoneNumber))
            {
                NotificationSystem
                    .ShowNotification(TempData, ApplicationMessages.DuplicateValueTitle, ApplicationMessages.DuplicateValueDescription, ApplicationMessagesIcon.ErrorIcon);
                return this.RedirectToReferrer();
            }

            if (!user.NationalCode.IsValidNationalCode())
            {
                NotificationSystem.ShowNotification(TempData, "کد ملی وارد شده صحیح نمی باشد", "", ApplicationMessagesIcon.ErrorIcon);
                return this.RedirectToReferrer();
            }
            try
            {
                foundUser.NationalCode = user.NationalCode.SanitizeString();
                foundUser.FullName = user.FullName.SanitizeString();
                foundUser.FatherName = user.FatherName!.SanitizeString();

                await _userService.UpdateAsync(foundUser);

                await _userService.SaveChangesAsync();

                NotificationSystem
                    .ShowNotification(TempData, ApplicationMessages.OperationSuccessful, "", ApplicationMessagesIcon.SuccessIcon);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception e)
            {
                NotificationSystem
                    .ShowNotification(TempData, ApplicationMessages.OperationFailed, "", ApplicationMessagesIcon.ErrorIcon);
                return this.RedirectToReferrer();
            }
        }

        [Route("User/ChangePassword")]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userService.GetUserByPhoneNumber(User);

            if (user is null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                return this.RedirectToReferrer();
            }

            return View(user);
        }

        [HttpPost]
        [Route("User/ChangePassword")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                NotificationSystem.ShowNotification(TempData, "همه فیلدها الزامی هستند", "همه فیلدها الزامی هستند.", ApplicationMessagesIcon.ErrorIcon);
                return View(await _userService.GetUserByPhoneNumber(User));
            }

            var user = await _userService.GetUserByPhoneNumber(User);
            if (user == null) return NotFound();

            // ۱. چک رمز فعلی
            if (!_userService.VerifyPassword(user.PasswordHash, currentPassword))
            {
                NotificationSystem.ShowNotification(TempData, "رمز عبور فعلی اشتباه است", "رمز عبور فعلی اشتباه است.", ApplicationMessagesIcon.ErrorIcon);
                return View(user);
            }

            // ۲. چک تکرار رمز
            if (newPassword != confirmPassword)
            {
                NotificationSystem.ShowNotification(TempData, "رمز عبور جدید و تکرار آن مطابقت ندارند", "رمزهای جدید مطابقت ندارند.", ApplicationMessagesIcon.ErrorIcon);
                return View(user);
            }

            // ۳. چک قدرت رمز (همون که قبلاً ساختیم)
            var (score, message, _) = PasswordStrengthChecker.Evaluate(newPassword, user.PhoneNumber, user.FullName);
            if (score < 70)
            {
                NotificationSystem.ShowNotification(TempData, $"رمز ضعیف - {message}", message, ApplicationMessagesIcon.ErrorIcon);
                return View(user);
            }

            // ۴. ذخیره رمز جدید
            user.PasswordHash = PasswordSecurity.PasswordHasher.HashPassword(newPassword);
            user.LastPasswordChangedAt = DateTime.UtcNow;

            await _userService.UpdateAsync(user);
            await _userService.SaveChangesAsync();

            // ۵. لاگ تغییر رمز
            var log = new UserLoginHistory
            {
                UserId = user.Id,
                PhoneNumber = user.PhoneNumber,
                LoginAt = DateTime.UtcNow,
                IpAddress = GetClientIp(),
                UserAgent = Request.Headers["User-Agent"].ToString(),
                Method = LoginMethod.Password,
                IsSuccessful = true,
                FailureReason = "PasswordChanged"
            };
            await _userService.AddUserLoginHistory(log);

            NotificationSystem.ShowNotification(TempData, "رمز عبور با موفقیت تغییر کرد", "رمز عبور با موفقیت تغییر کرد.", ApplicationMessagesIcon.SuccessIcon);

            return RedirectToAction("Index", "Home");
        }

        [Route("User/ChangeUserPasswordFromAdmin/{userId}")]
        [Permission(SystemPermissions.PermissionList.ChangeUserPasswordFromAdmin)]
        public async Task<IActionResult> ChangeUserPasswordFromAdmin(long userId)
        {
            var user = await _userService.GetAsNoTrackingAsync(userId);
            if (user == null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                return this.RedirectToReferrer();
            }

            return View(user);
        }


        [HttpPost]
        [Route("User/ChangeUserPasswordFromAdmin/{userId}")]
        [Authorize]
        [ValidateAntiForgeryToken]
        [Permission(SystemPermissions.PermissionList.ChangeUserPasswordFromAdmin)]
        public async Task<IActionResult> ChangePassword(long userId, string newPassword, string confirmPassword)
        {
            var user = await _userService.GetAsync(userId);
            if (user is null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                return this.RedirectToReferrer();
            }

            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                NotificationSystem.ShowNotification(TempData, "همه فیلدها الزامی هستند", "همه فیلدها الزامی هستند.", ApplicationMessagesIcon.ErrorIcon);
                return View(user);
            }

            // 1. چک تکرار رمز
            if (newPassword != confirmPassword)
            {
                NotificationSystem.ShowNotification(TempData, "رمز عبور جدید و تکرار آن مطابقت ندارند", "رمزهای جدید مطابقت ندارند.", ApplicationMessagesIcon.ErrorIcon);
                return View(user);
            }

            // ۳. چک قدرت رمز (همون که قبلاً ساختیم)
            var (score, message, _) = PasswordStrengthChecker.Evaluate(newPassword, user.PhoneNumber, user.FullName);
            if (score < 70)
            {
                NotificationSystem.ShowNotification(TempData, $"رمز ضعیف - {message}", message, ApplicationMessagesIcon.ErrorIcon);
                return View(user);
            }

            // ۴. ذخیره رمز جدید
            user.PasswordHash = PasswordSecurity.PasswordHasher.HashPassword(newPassword);
            user.LastPasswordChangedAt = DateTime.UtcNow;

            await _userService.UpdateAsync(user);
            await _userService.SaveChangesAsync();

            // ۵. لاگ تغییر رمز
            var log = new UserLoginHistory
            {
                UserId = user.Id,
                PhoneNumber = user.PhoneNumber,
                LoginAt = DateTime.UtcNow,
                IpAddress = GetClientIp(),
                UserAgent = Request.Headers["User-Agent"].ToString(),
                Method = LoginMethod.Password,
                IsSuccessful = true,
                FailureReason = "PasswordChanged"
            };
            await _userService.AddUserLoginHistory(log);

            NotificationSystem.ShowNotification(TempData, "رمز عبور با موفقیت تغییر کرد", "رمز عبور با موفقیت تغییر کرد.", ApplicationMessagesIcon.SuccessIcon);

            return RedirectToAction("AllUsers", "User");
        }

        [Route("Users/ChangeUserStatus/{userId}/{status}")]
        [Permission(SystemPermissions.PermissionList.ActivateAndDeactivateUsers)]
        public async Task<IActionResult> ChangeUserStatus(long userId, string status)
        {
            try
            {
                var user = await _userService.GetAsync(userId);
                if (user is null)
                {
                    NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "",
                        ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }

                switch (status)
                {
                    case "Activate":
                        user.IsActive = true;
                        break;
                    case "Deactivate":
                        user.IsActive = false;
                        break;
                }

                await _userService.UpdateAsync(user);
                await _userService.SaveChangesAsync();


                NotificationSystem.ShowNotification(TempData, ApplicationMessages.OperationSuccessful, "",
                    ApplicationMessagesIcon.SuccessIcon);
                return this.RedirectToReferrer();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.OperationFailed, "",
                    ApplicationMessagesIcon.ErrorIcon);
                return this.RedirectToReferrer();
            }
        }


        #region Helpers

        private string GetClientIp()
        {
            var headers = Request.Headers;

            if (headers.ContainsKey("X-Forwarded-For"))
                return headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();

            if (headers.ContainsKey("X-Real-IP"))
                return headers["X-Real-IP"].ToString();

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        #endregion

    }
}

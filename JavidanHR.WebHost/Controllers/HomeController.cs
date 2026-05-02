using _0_Framework.Utilities.NotificationSystem;
using AuthenticationSystem.Domain.User;
using AuthenticationSystem.Services.Repositories;
using HrSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JavidanHR.WebHost.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserRepository _userService;
        private readonly IEmployeeService _employeeService;

        public HomeController(IUserRepository userService, IEmployeeService employeeService)
        {
            _userService = userService;
            _employeeService = employeeService;
        }

        [Authorize]
        public  async Task<IActionResult> Index()
        {
            var currentUser = await _userService.GetUserByPhoneNumber(User);
            if (currentUser is null)
            {
                NotificationSystem.ShowNotification(TempData,ApplicationMessages.SessionExpired,"",ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login", "Account");
            }

            var employee = await _employeeService.SingleOrDefaultByCondition(x => x.UserId == currentUser.Id);
            ViewBag.IsEmployeeCreated = employee is not null;
            return View();
        }
    }
}

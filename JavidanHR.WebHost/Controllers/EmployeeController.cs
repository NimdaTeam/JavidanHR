using _0_Framework.FileUploader;
using _0_Framework.Utilities.Helpers;
using _0_Framework.Utilities.NotificationSystem;
using _0_Framework.Utilities.Pagination;
using _0_Framework.Utilities.Security;
using AuthenticationSystem.Domain.User;
using AuthenticationSystem.Services.Repositories;
using AuthenticationSystem.SystemPermissions;
using DNTPersianUtils.Core;
using HrSystem.Application.DTO;
using HrSystem.Application.Interfaces;
using HrSystem.Domain.Entities;
using JavidanHR.WebHost.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayrollSystem.Application.Interfaces;
using WebHost.Helpers.GlobalHelpers;
using WebHost.PageSecurity;
using WebHost.Utilities;

namespace JavidanHR.WebHost.Controllers
{
    [Route("Employee")]
    [Authorize]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IFileUploadService _fileUploadService;
        private readonly IUserRepository _userService;
        private readonly IRoleRepository _roleService;
        private readonly IWorkshopService _workshopService;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(IEmployeeService employeeService, IFileUploadService fileUploadService, IUserRepository userService, IRoleRepository roleService, IWorkshopService workshopService, ILogger<EmployeeController> logger)
        {
            _employeeService = employeeService;
            _fileUploadService = fileUploadService;
            _userService = userService;
            _roleService = roleService;
            _workshopService = workshopService;
            _logger = logger;
        }

        #region All Employees
        [Route("All")]
        [Permission(SystemPermissions.PermissionList.EmployeesList)]
        public async Task<IActionResult> AllEmployees(string searchQuery = "", int page = 1, string department = "", string position = "", MaritalStatus? maritalStatus = null, CooperationType? cooperationType = null)
        {
            var model = await _employeeService.GetAllEmployeesList();

            ViewBag.TotalCount = model.Count;

            model = SearchHelper.Search(model,
                searchQuery.SanitizeString(),
                x => x.FullName
            ).ToList();

            if (!string.IsNullOrWhiteSpace(department))
                model = model.Where(x => x.Department == department).ToList();

            if (!string.IsNullOrWhiteSpace(position))
                model = model.Where(x => x.Position == position).ToList();

            if (maritalStatus is not null)
                model = model.Where(x => x.MaritalStatus == maritalStatus).ToList();

            if (cooperationType is not null)
                model = model.Where(x => x.CooperationType == cooperationType).ToList();

            var paginatedModel = PaginationHelper.Paginate(new PaginationRequest<EmployeeListItemDTO>()
            {
                CurrentPage = page,
                ModelList = model,
                SearchQuery = searchQuery.SanitizeString()
            });

            ViewBag.Departments = await _employeeService.GetAllDepartments();
            ViewBag.Positions = await _employeeService.GetAllPositions();
            return View(paginatedModel);
        }

        #endregion

        #region Delete Employe
        [Route("Delete/{empId}")]
        [Permission(SystemPermissions.PermissionList.DeleteEmployee)]
        public async Task<IActionResult> Delete(long empId)
        {
            var employee = await _employeeService.GetById(empId);
            if (employee is null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("AllEmployees");
            }

            var status = await _employeeService.Delete(employee);

            if (status)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.OperationSuccessful, "",
                    ApplicationMessagesIcon.SuccessIcon);
            }
            else
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.OperationFailed, "",
                    ApplicationMessagesIcon.ErrorIcon);
            }
            return RedirectToAction("AllEmployees");
        }
        #endregion

        #region Step 1 - Employee Personal and Family Information
        [Route("AddEmployee")]
        public async Task<IActionResult> AddEmployee(bool isUpdating = false, long? employeeId = null, bool isAdminCreatingNewEmployee = false)
        {
            Step1PersonalVM model = new();

            var currentUser = await _userService.GetUserByPhoneNumber(User);
            if (currentUser == null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.SessionExpired, "", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login", "Account");
            }

            var userPermissions = await _roleService.GetLoggedInUserPermissions();

            var workshops = await _workshopService.GetAllWorkshopsAsync();
            var workshopList = new List<EmployeeWorkshopListItem>();
            

            if (isUpdating && employeeId is > 0)
            {
                var emp = await _employeeService.GetEmployeeForEdit(employeeId.Value);
                if (emp == null)
                {
                    NotificationSystem.ShowNotification(TempData, "کارمند یافت نشد.", "", ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }

                // چک دسترسی برای ویرایش (بعد از تأیید نهایی فقط با پرمیشن Override)
                if (emp.IsProfileCompletedByEmployee)
                {
                    if (emp.UserId == currentUser.Id)
                    {
                        NotificationSystem.ShowNotification(TempData, "اطلاعات کارمندی شما پیش از این تأیید شده و قابل ویرایش نیست.", "", ApplicationMessagesIcon.ErrorIcon);
                        return this.RedirectToReferrer();
                    }

                    if (!userPermissions.Contains((long)SystemPermissions.PermissionList.EditEmployeeAfterUserConfirmation))
                    {
                        NotificationSystem.ShowNotification(TempData, "پروفایل کارمند تأیید نهایی شده و قابل ویرایش نیست.", "", ApplicationMessagesIcon.ErrorIcon);
                        return this.RedirectToReferrer();
                    }
                }
                else
                {
                    if (emp.UserId != currentUser.Id &&
                        userPermissions.All(x => x != (long)SystemPermissions.PermissionList.EditEmployeePersonalInfo))
                    {
                        NotificationSystem.ShowNotification(TempData, ApplicationMessages.AccessDenied, "", ApplicationMessagesIcon.ErrorIcon);
                        return this.RedirectToReferrer();
                    }
                }

                ViewData["EmployeeTitle"] = $"{emp.FirstName} {emp.LastName} - ({emp.EmployeeCode})";
                model = _employeeService.GetEmployeeData_Step1(emp);

               workshopList = workshops.Select(x => new EmployeeWorkshopListItem()
                {
                    Id = x.Id,
                    Name = x.Name,
                    IsSelected = x.Id == model.WorkshopId
                }).ToList();

                ViewBag.workshops = workshopList;

                ViewBag.IsFirstTime = true;
                return View("AddEmployee", model);
            }

            // حالت ایجاد کارمند جدید (توسط خود کارمند یا ادمین)
            if (!isAdminCreatingNewEmployee)
            {
                var existingEmployee = await _employeeService.SingleOrDefaultByCondition(x => x.UserId == currentUser.Id);
                if (existingEmployee != null)
                {
                    return RedirectToAction("EmployeeDetails", new { empId = existingEmployee.Id });
                }
            }
            model = new Step1PersonalVM
            {
                Id = null,
                IsUpdating = false,
                NationalCode = "",
                MobilePhone = currentUser.PhoneNumber ?? ""
            };

            workshopList = workshops.Select(x => new EmployeeWorkshopListItem()
            {
                Id = x.Id,
                Name = x.Name,
                IsSelected = x.Id == model.WorkshopId
            }).ToList();

            ViewBag.workshops = workshopList;

            ViewData["Title"] = "تکمیل اطلاعات اولیه (مرحله ۱ از ۵)";
            return View("AddEmployee", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("AddEmployee")]
        public async Task<IActionResult> AddEmployee([FromForm] Step1PersonalVM emp)
        {
            var birthGregorian = emp.PersianBirthDateStringify.ToGregorianDateTime();
            if (!birthGregorian.HasValue)
            {
               NotificationSystem.ShowNotification(TempData,"تاریخ تولد وارد شده معتبر نیست","",ApplicationMessagesIcon.ErrorIcon);
               return View(emp);
            }

            if (!emp.NationalCode.IsValidNationalCode())
            {
                NotificationSystem.ShowNotification(TempData, "کد ملی وارد شده معتبر نمی باشد", "", ApplicationMessagesIcon.ErrorIcon);
                return View(emp);
            }

            var currentUser = await _userService.GetUserByPhoneNumber(User);
            if (currentUser is null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.SessionExpired, "", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login", "Account");
            }

            var currentUserPermissions = await _roleService.GetLoggedInUserPermissions();
            var isAdminCreatingEmployee = currentUserPermissions.Contains((long)SystemPermissions.PermissionList.CreateNewEmployee);

            if (!emp.IsUpdating)
            {
                // === حالت ایجاد کارمند جدید ===
                if (!isAdminCreatingEmployee)
                {
                    var existing = await _employeeService.SingleOrDefaultByCondition(x => x.UserId == currentUser.Id);
                    if (existing != null)
                    {
                        emp.IsUpdating = true;
                        return View(emp);
                    }
                }

                if (await _employeeService.IsExist(emp.EmployeeCode.SanitizeString(), emp.NationalCode.SanitizeString()))
                {
                    NotificationSystem.ShowNotification(TempData, ApplicationMessages.DuplicateValueTitle, "", ApplicationMessagesIcon.ErrorIcon);
                    return View(emp);
                }

                long? newUserId = currentUser.Id;

                // اگر ادمین داره ایجاد میکنه → کاربر جدید بساز
                if (isAdminCreatingEmployee && emp.MobilePhone != currentUser.PhoneNumber)
                {
                    var existingUser =
                        await _userService.SingleOrDefaultByConditionAsync(x =>
                            x.PhoneNumber == emp.MobilePhone.SanitizeString());

                    if (existingUser is not null)
                    {
                        newUserId = existingUser.Id;
                    }
                    else
                    {
                        var newUser = new Users
                        {
                            PhoneNumber = emp.MobilePhone.SanitizeString(),
                            FullName = $"{emp.FirstName.SanitizeString()} {emp.LastName.SanitizeString()}".Trim(),
                            FatherName = emp.FathersName.SanitizeString(),
                            NationalCode = emp.NationalCode.SanitizeString(),
                            IsActive = true
                        };

                        newUserId = await _userService.CreateNewUser(newUser);
                        if (newUserId <= 0)
                        {
                            NotificationSystem.ShowNotification(TempData, "خطا در ایجاد حساب کاربری کارمند جدید", "", ApplicationMessagesIcon.ErrorIcon);
                            return View(emp);
                        }
                    }
                }

                emp.UserId = newUserId.Value;
                
                var result = await _employeeService.AddNewEmployee_Step1(emp);

                var employeeLinkedUser = await _userService.GetAsync((long)emp.UserId);
                if (employeeLinkedUser is not null && (string.IsNullOrWhiteSpace(employeeLinkedUser.FullName) ||
                                                       string.IsNullOrWhiteSpace(employeeLinkedUser.FatherName) ||
                                                       string.IsNullOrWhiteSpace(employeeLinkedUser.NationalCode)))
                {
                    employeeLinkedUser.FullName = $"{emp.FirstName.SanitizeString()} {emp.LastName.SanitizeString()}";
                    employeeLinkedUser.FatherName = emp.FathersName.SanitizeString();
                    employeeLinkedUser.NationalCode = emp.NationalCode.SanitizeString();
                    await _userService.UpdateAsync(employeeLinkedUser);
                    await _userService.SaveChangesAsync();
                }

                if (result)
                    NotificationSystem.ShowNotification(TempData, ApplicationMessages.OperationSuccessful, "", ApplicationMessagesIcon.SuccessIcon);
                else
                    NotificationSystem.ShowNotification(TempData, ApplicationMessages.OperationFailed, "", ApplicationMessagesIcon.ErrorIcon);
            }
            else
            {
                // === حالت ویرایش ===
                if (!emp.Id.HasValue)
                {
                    NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }

                var employee = await _employeeService.GetEmployeeForEdit(emp.Id.Value);
                if (employee == null)
                {
                    NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }

                if (employee.IsProfileCompletedByEmployee)
                {
                    if (!currentUserPermissions.Contains((long)SystemPermissions.PermissionList.EditEmployeeAfterUserConfirmation))
                    {
                        NotificationSystem.ShowNotification(TempData, "پروفایل کارمند تأیید نهایی شده و قابل ویرایش نیست.", "", ApplicationMessagesIcon.ErrorIcon);
                        return this.RedirectToReferrer();
                    }
                }
                else
                {
                    if (employee.UserId != currentUser.Id &&
                        currentUserPermissions.All(x => x != (long)SystemPermissions.PermissionList.EditEmployeePersonalInfo))
                    {
                        NotificationSystem.ShowNotification(TempData, ApplicationMessages.AccessDenied, "", ApplicationMessagesIcon.ErrorIcon);
                        return this.RedirectToReferrer();
                    }
                }

                if (await _employeeService.IsExistForUpdate(emp.EmployeeCode.SanitizeString(), emp.NationalCode.SanitizeString(), emp.Id.Value))
                {
                    NotificationSystem.ShowNotification(TempData, ApplicationMessages.DuplicateValueTitle, "", ApplicationMessagesIcon.ErrorIcon);
                    return View(emp);
                }

                var result = await _employeeService.UpdateEmployee_Step1(employee, emp);
                if (result)
                    NotificationSystem.ShowNotification(TempData, ApplicationMessages.OperationSuccessful, "", ApplicationMessagesIcon.SuccessIcon);
                else
                    NotificationSystem.ShowNotification(TempData, ApplicationMessages.OperationFailed, "", ApplicationMessagesIcon.ErrorIcon);
            }

            if (currentUserPermissions.Any(x => x == (long)SystemPermissions.PermissionList.EditEmployeePersonalInfo) ||
                currentUserPermissions.Any(x => x == (long)SystemPermissions.PermissionList.CreateNewEmployee))
            {
                return RedirectToAction("AllEmployees");
            }

            return RedirectToAction("EmployeeDetails");
        }
        #endregion //done

        #region Step 2 - Education Information
        [HttpGet]
        [Route("EducationAndTrainings/{employeeId}")]
        public async Task<IActionResult> EducationAndTrainings(long employeeId)
        {
            var employee = await _employeeService.GetEmployeeForEdit(employeeId);
            if (employee == null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                return this.RedirectToReferrer();
            }

            var currentUser = await _userService.GetUserByPhoneNumber(User);
            if (currentUser is null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.SessionExpired, "", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login", "Account");
            }

            var userPermissions = await _roleService.GetLoggedInUserPermissions();

            // اگر پروفایل تأیید شده باشد
            if (employee.IsProfileCompletedByEmployee)
            {
                // خود کارمند اصلاً نباید بتونه بیاد تو صفحه
                if (employee.UserId == currentUser.Id)
                {
                    NotificationSystem.ShowNotification(TempData, "اطلاعات کارمندی شما پیش از این تأیید شده و قابل ویرایش نیست.", "", ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }

                // فقط کسی که پرمیشن Override داره می‌تونه ببینه و ویرایش کنه
                if (!userPermissions.Contains((long)SystemPermissions.PermissionList.EditEmployeeAfterUserConfirmation))
                {
                    NotificationSystem.ShowNotification(TempData, "پروفایل کارمند تأیید نهایی شده و قابل ویرایش نیست.", "", ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }
            }
            else
            {
                // قبل از تأیید: خود کارمند یا ادمین با پرمیشن عادی
                if (employee.UserId != currentUser.Id &&
                    userPermissions.All(x => x != (long)SystemPermissions.PermissionList.EditEmployeeEducationInfo))
                {
                    NotificationSystem.ShowNotification(TempData, ApplicationMessages.AccessDenied, "", ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }
            }

            var model = await _employeeService.GetStep2EducationData(employeeId);
            ViewData["Title"] = "تحصیلات و دوره‌های آموزشی (مرحله ۲ از ۵)";
            ViewData["EmployeeName"] = $"{employee.FirstName} {employee.LastName}";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("EducationAndTrainings/{employeeId}")]
        public async Task<IActionResult> EducationAndTrainings(long employeeId, Step2EducationVM model)
        {
            if (!ModelState.IsValid)
            {
                NotificationSystem.ShowNotification(TempData, "لطفاً فیلدهای ضروری را تکمیل کنید.", "", "error");
                return View(model);
            }

            var employee = await _employeeService.GetEmployeeForEdit(employeeId);
            if (employee == null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                return this.RedirectToReferrer();
            }

            var currentUser = await _userService.GetUserByPhoneNumber(User);
            if (currentUser is null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.SessionExpired, "", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login", "Account");
            }

            var userPermissions = await _roleService.GetLoggedInUserPermissions();

            // اگر پروفایل تأیید شده باشد
            if (employee.IsProfileCompletedByEmployee)
            {
                // خود کارمند نباید بتونه ذخیره کنه
                if (employee.UserId == currentUser.Id)
                {
                    NotificationSystem.ShowNotification(TempData, "اطلاعات کارمندی شما پیش از این تأیید شده و قابل ویرایش نیست.", "", ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }

                // فقط پرمیشن Override اجازه ذخیره می‌ده
                if (!userPermissions.Contains((long)SystemPermissions.PermissionList.EditEmployeeAfterUserConfirmation))
                {
                    NotificationSystem.ShowNotification(TempData, "پروفایل کارمند تأیید نهایی شده و قابل ویرایش نیست.", "", ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }
            }
            else
            {
                // قبل از تأیید: خود کارمند یا ادمین عادی
                if (employee.UserId != currentUser.Id &&
                    userPermissions.All(x => x != (long)SystemPermissions.PermissionList.EditEmployeeEducationInfo))
                {
                    NotificationSystem.ShowNotification(TempData, ApplicationMessages.AccessDenied, "", ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }
            }

            var success = await _employeeService.UpdateEmployeeEducationInformation_Step2(employee, model);
            if (success)
            {
                NotificationSystem.ShowNotification(TempData, "تحصیلات و دوره‌های آموزشی با موفقیت ذخیره شد", "", "success");
                return RedirectToAction("EmployeeDetails", new { empId = employee.Id });
            }

            NotificationSystem.ShowNotification(TempData, "خطا در ذخیره اطلاعات", "", "error");
            return View(model);
        }
        #endregion

        #region Step 3 - Work Experiences
        [HttpGet]
        [Route("WorkExperiences/{employeeId}")]
        public async Task<IActionResult> WorkExperiences(long employeeId)
        {
            var employee = await _employeeService.GetEmployeeForEdit(employeeId);
            if (employee == null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                return this.RedirectToReferrer();
            }

            var currentUser = await _userService.GetUserByPhoneNumber(User);
            if (currentUser is null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.SessionExpired, "", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login", "Account");
            }

            var userPermissions = await _roleService.GetLoggedInUserPermissions();

            // اگر پروفایل تأیید نهایی شده باشد
            if (employee.IsProfileCompletedByEmployee)
            {
                // خود کارمند دیگه نباید بتونه وارد صفحه بشه
                if (employee.UserId == currentUser.Id)
                {
                    NotificationSystem.ShowNotification(TempData, "اطلاعات کارمندی شما پیش از این تأیید شده و قابل ویرایش نیست.", "", ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }

                // فقط کسی که پرمیشن Override داره اجازه ورود داره
                if (!userPermissions.Contains((long)SystemPermissions.PermissionList.EditEmployeeAfterUserConfirmation))
                {
                    NotificationSystem.ShowNotification(TempData, "پروفایل کارمند تأیید نهایی شده و قابل ویرایش نیست.", "", ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }
            }
            else
            {
                // قبل از تأیید نهایی: خود کارمند یا ادمین با پرمیشن عادی
                if (employee.UserId != currentUser.Id &&
                    userPermissions.All(x => x != (long)SystemPermissions.PermissionList.EditEmployeeWorkExperienceInfo))
                {
                    NotificationSystem.ShowNotification(TempData, ApplicationMessages.AccessDenied, "", ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }
            }

            var model = _employeeService.GetStep3WorkExperienceData(employee);
            ViewData["Title"] = "تجربیات کاری (مرحله ۳ از ۵)";
            ViewData["EmployeeName"] = $"{employee.FirstName} {employee.LastName}";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("WorkExperiences/{employeeId}")]
        public async Task<IActionResult> WorkExperiences([FromForm] Step3WorkExperienceVM model)
        {
            if (!ModelState.IsValid)
            {
                NotificationSystem.ShowNotification(TempData, "لطفاً تمامی فیلدهای الزامی را تکمیل کنید.", "", "error");
                return View(model);
            }

            var isStartDatesValid = model.WorkExperiences.Select(item => item.StartDatePersian.ToGregorianDateTime()).Select(startGregorian => startGregorian.HasValue).ToList();
            if (isStartDatesValid.Any(x => x == false))
            {
                NotificationSystem.ShowNotification(TempData,"تاریخ شروع یک یا چند مورد از تجارب کاری وارد شده معتبر نیست","",ApplicationMessagesIcon.ErrorIcon);
            }

            var employee = await _employeeService.GetEmployeeForEdit(model.EmployeeId);
            if (employee == null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                return this.RedirectToReferrer();
            }

            var currentUser = await _userService.GetUserByPhoneNumber(User);
            if (currentUser is null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.SessionExpired, "", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login", "Account");
            }

            var userPermissions = await _roleService.GetLoggedInUserPermissions();

            // اگر پروفایل تأیید نهایی شده باشد
            if (employee.IsProfileCompletedByEmployee)
            {
                // خود کارمند نباید بتونه ذخیره کنه
                if (employee.UserId == currentUser.Id)
                {
                    NotificationSystem.ShowNotification(TempData, "اطلاعات کارمندی شما پیش از این تأیید شده و قابل ویرایش نیست.", "", ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }

                // فقط با پرمیشن Override اجازه ذخیره داره
                if (!userPermissions.Contains((long)SystemPermissions.PermissionList.EditEmployeeAfterUserConfirmation))
                {
                    NotificationSystem.ShowNotification(TempData, "پروفایل کارمند تأیید نهایی شده و قابل ویرایش نیست.", "", ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }
            }
            else
            {
                // قبل از تأیید نهایی: خود کارمند یا ادمین عادی
                if (employee.UserId != currentUser.Id &&
                    userPermissions.All(x => x != (long)SystemPermissions.PermissionList.EditEmployeeWorkExperienceInfo))
                {
                    NotificationSystem.ShowNotification(TempData, ApplicationMessages.AccessDenied, "", ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }
            }

            // تبدیل تاریخ‌ها (همون کد قبلی بدون تغییر)
            foreach (var item in model.WorkExperiences)
            {
                if (!item.StartDatePersian.IsValidPersianDate())
                    ModelState.AddModelError("", $"تاریخ شروع در تجربه \"{item.Organization}\" نامعتبر است.");
                if (!string.IsNullOrEmpty(item.EndDatePersian) && !item.EndDatePersian.IsValidPersianDate())
                    ModelState.AddModelError("", $"تاریخ پایان در تجربه \"{item.Organization}\" نامعتبر است.");
            }

            var success = await _employeeService.UpdateEmployeeWorkExperience_Step3(employee, model);

            if (success)
            {
                NotificationSystem.ShowNotification(TempData, "تجربیات کاری با موفقیت ذخیره شد", "", "success");
                return RedirectToAction("EmployeeDetails", new { empId = employee.Id });
            }

            NotificationSystem.ShowNotification(TempData, "خطا در ذخیره اطلاعات", "", "error");
            return View(model);
        }
        #endregion

        #region Step 4 - Organizational Information
        [HttpGet]
        [Route("OrganizationalInformation/{employeeId}")]
        public async Task<IActionResult> OrganizationalInformation(long employeeId)
        {
            var employee = await _employeeService.GetEmployeeForEdit(employeeId);
            if (employee == null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                return this.RedirectToReferrer();
            }

            var currentUser = await _userService.GetUserByPhoneNumber(User);
            if (currentUser is null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.SessionExpired, "", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login", "Account");
            }

            var userPermissions = await _roleService.GetLoggedInUserPermissions();

            // اگر پروفایل تأیید نهایی شده باشد
            if (employee.IsProfileCompletedByEmployee)
            {
                // خود کارمند نباید بتونه وارد صفحه بشه
                if (employee.UserId == currentUser.Id)
                {
                    NotificationSystem.ShowNotification(TempData, "اطلاعات کارمندی شما پیش از این تأیید شده و قابل ویرایش نیست.", "", ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }

                // فقط با پرمیشن Override اجازه داره
                if (!userPermissions.Contains((long)SystemPermissions.PermissionList.EditEmployeeAfterUserConfirmation))
                {
                    NotificationSystem.ShowNotification(TempData, "پروفایل کارمند تأیید نهایی شده و قابل ویرایش نیست.", "", ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }
            }
            else
            {
                // قبل از تأیید نهایی: خود کارمند یا ادمین با پرمیشن عادی
                if (employee.UserId != currentUser.Id &&
                    userPermissions.All(x => x != (long)SystemPermissions.PermissionList.EditEmployeeOrganizationalInfo))
                {
                    NotificationSystem.ShowNotification(TempData, ApplicationMessages.AccessDenied, "", ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }
            }

            var model = _employeeService.GetStep4EmploymentData_Step4(employee);
            ViewData["EmployeeName"] = $"{employee.FirstName} {employee.LastName}";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("OrganizationalInformation/{employeeId}")]
        public async Task<IActionResult> OrganizationalInformation([FromForm] Step4EmploymentVM model)
        {
            if (!ModelState.IsValid)
            {
                NotificationSystem.ShowNotification(TempData, "لطفاً فیلدهای الزامی را تکمیل کنید.", "", "error");
                ViewData["EmployeeName"] = "نامشخص";
                return View(model);
            }

            var employee = await _employeeService.GetEmployeeForEdit(model.EmployeeId);
            if (employee == null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                return this.RedirectToReferrer();
            }

            var currentUser = await _userService.GetUserByPhoneNumber(User);
            if (currentUser is null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.SessionExpired, "", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login", "Account");
            }

            var userPermissions = await _roleService.GetLoggedInUserPermissions();

            // اگر پروفایل تأیید نهایی شده باشد
            if (employee.IsProfileCompletedByEmployee)
            {
                // خود کارمند نباید بتونه ذخیره کنه
                if (employee.UserId == currentUser.Id)
                {
                    NotificationSystem.ShowNotification(TempData, "اطلاعات کارمندی شما پیش از این تأیید شده و قابل ویرایش نیست.", "", ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }

                // فقط با پرمیشن Override اجازه ذخیره داره
                if (!userPermissions.Contains((long)SystemPermissions.PermissionList.EditEmployeeAfterUserConfirmation))
                {
                    NotificationSystem.ShowNotification(TempData, "پروفایل کارمند تأیید نهایی شده و قابل ویرایش نیست.", "", ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }
            }
            else
            {
                // قبل از تأیید نهایی: خود کارمند یا ادمین عادی
                if (employee.UserId != currentUser.Id &&
                    userPermissions.All(x => x != (long)SystemPermissions.PermissionList.EditEmployeeOrganizationalInfo))
                {
                    NotificationSystem.ShowNotification(TempData, ApplicationMessages.AccessDenied, "", ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }
            }

            var success = await _employeeService.UpdateEmployeeEmploymentData_Step4(employee, model);

            if (success)
            {
                NotificationSystem.ShowNotification(TempData, "اطلاعات استخدامی با موفقیت ذخیره شد", "", "success");
                return RedirectToAction("EmployeeDetails", new { empId = employee.Id });
            }

            NotificationSystem.ShowNotification(TempData, "خطا در ذخیره اطلاعات", "", "error");
            return View(model);
        }
        #endregion

        #region Step 5 - Financial Information
        [HttpGet]
        [Route("FinancialInformation/{employeeId}")]
        public async Task<IActionResult> FinancialInformation(long employeeId)
        {
            var employee = await _employeeService.GetEmployeeForEdit(employeeId);
            if (employee == null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                return this.RedirectToReferrer();
            }

            var currentUser = await _userService.GetUserByPhoneNumber(User);
            if (currentUser is null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.SessionExpired, "", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login", "Account");
            }

            var userPermissions = await _roleService.GetLoggedInUserPermissions();

            // اگر پروفایل قبلاً تأیید نهایی شده باشد
            if (employee.IsProfileCompletedByEmployee)
            {
                // خود کارمند اصلاً نباید وارد صفحه بشه
                if (employee.UserId == currentUser.Id)
                {
                    NotificationSystem.ShowNotification(TempData, "اطلاعات کارمندی شما پیش از این تأیید شده و قابل ویرایش نیست.", "", ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }

                // فقط کسی که پرمیشن Override داره اجازه ورود داره
                if (!userPermissions.Contains((long)SystemPermissions.PermissionList.EditEmployeeAfterUserConfirmation))
                {
                    NotificationSystem.ShowNotification(TempData, "پروفایل کارمند تأیید نهایی شده و قابل ویرایش نیست.", "", ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }
            }
            else
            {
                // قبل از تأیید نهایی: خود کارمند یا ادمین با پرمیشن عادی
                if (employee.UserId != currentUser.Id &&
                    userPermissions.All(x => x != (long)SystemPermissions.PermissionList.EditEmployeeFinancialInfo))
                {
                    NotificationSystem.ShowNotification(TempData, ApplicationMessages.AccessDenied, "", ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }
            }

            var model = _employeeService.GetFinancialData_Step5(employee);
            ViewData["EmployeeName"] = $"{employee.FirstName} {employee.LastName}";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("FinancialInformation/{employeeId}")]
        public async Task<IActionResult> FinancialInformation([FromForm] Step5FinancialVM model)
        {
            if (!ModelState.IsValid)
            {
                NotificationSystem.ShowNotification(TempData, "لطفاً فیلدهای الزامی را تکمیل کنید.", "", "error");
                ViewData["EmployeeName"] = "نامشخص";
                return View(model);
            }

            var employee = await _employeeService.GetEmployeeForEdit(model.EmployeeId);
            if (employee == null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                return this.RedirectToReferrer();
            }

            var currentUser = await _userService.GetUserByPhoneNumber(User);
            if (currentUser is null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.SessionExpired, "", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login", "Account");
            }

            var userPermissions = await _roleService.GetLoggedInUserPermissions();

            // اگر پروفایل قبلاً تأیید نهایی شده باشد
            if (employee.IsProfileCompletedByEmployee)
            {
                // خود کارمند نباید بتونه ذخیره کنه
                if (employee.UserId == currentUser.Id)
                {
                    NotificationSystem.ShowNotification(TempData, "اطلاعات کارمندی شما پیش از این تأیید شده و قابل ویرایش نیست.", "", ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }

                // فقط با پرمیشن Override اجازه ذخیره داره
                if (!userPermissions.Contains((long)SystemPermissions.PermissionList.EditEmployeeAfterUserConfirmation))
                {
                    NotificationSystem.ShowNotification(TempData, "پروفایل کارمند تأیید نهایی شده و قابل ویرایش نیست.", "", ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }
            }
            else
            {
                // قبل از تأیید نهایی: خود کارمند یا ادمین عادی
                if (employee.UserId != currentUser.Id &&
                    userPermissions.All(x => x != (long)SystemPermissions.PermissionList.EditEmployeeFinancialInfo))
                {
                    NotificationSystem.ShowNotification(TempData, ApplicationMessages.AccessDenied, "", ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }
            }

            var success = await _employeeService.UpdateEmployeeFinancialData_Step5(employee, model);

            if (success)
            {
                NotificationSystem.ShowNotification(TempData, "اطلاعات مالی با موفقیت ذخیره شد!", "", "success");
                return RedirectToAction("EmployeeDetails", new { empId = employee.Id });
            }

            NotificationSystem.ShowNotification(TempData, "خطا در ذخیره اطلاعات", "", "error");
            return View(model);
        }
        #endregion

        #region Employee Details
        [HttpGet]
        [Route("Details")]
        public async Task<IActionResult> EmployeeDetails(long? empId = null)
        {
            var currentUser = await _userService.GetUserByPhoneNumber(User);
            if (currentUser is null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.SessionExpired, "", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login", "Account");
            }
            var userPermissions = await _roleService.GetLoggedInUserPermissions();

            if (empId is null)
            {
                var employee = await _employeeService.SingleOrDefaultByCondition(x => x.UserId == currentUser.Id);
                if (employee == null)
                {
                    NotificationSystem.ShowNotification(TempData, "اطلاعات شما در سامانه یافت نشد، لطفا نسبت به تکمیل اطلاعات کارمندی خود اقدام فرمایید.", "", ApplicationMessagesIcon.ErrorIcon);
                    return RedirectToAction("AddEmployee");
                }

                empId = employee.Id;
            }


            var employeeDetails = await _employeeService.GetEmployeeDetails((long)empId);
            if (employeeDetails.Id <= 0)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                return this.RedirectToReferrer();
            }

            if (employeeDetails.UserId == currentUser.Id || userPermissions.Any(x => x == (long)SystemPermissions.PermissionList.ViewEmployeeDetails))
                return View(employeeDetails);

            NotificationSystem.ShowNotification(TempData, ApplicationMessages.AccessDenied, "", ApplicationMessagesIcon.ErrorIcon);
            return this.RedirectToReferrer();
        }
        #endregion

        #region Print Profile
        [Route("PrintProfile/{empId}")]
        public async Task<IActionResult> PrintProfile(long empId)
        {
            var model = await _employeeService.GetEmployeeDetails(empId);
            if (model.Id <= 0)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                return this.RedirectToReferrer();
            }

            return View(model);
        }
        #endregion

        #region Confirm Employee Data (by Employee)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ConfirmProfileFinal/{employeeId}")]
        public async Task<IActionResult> ConfirmProfileFinal(long employeeId)
        {
            var currentUser = await _userService.GetUserByPhoneNumber(User);
            if (currentUser == null)
                return Json(new { success = false, message = "جلسه شما منقضی شده است." });

            var employee = await _employeeService.GetEmployeeForEdit(employeeId);
            if (employee == null)
                return Json(new { success = false, message = "کارمند یافت نشد." });

            // فقط خود کارمند می‌تونه تأیید نهایی کنه
            if (employee.UserId != currentUser.Id)
                return Json(new { success = false, message = "شما اجازه تأیید نهایی این پروفایل را ندارید." });

            // اگر قبلاً تأیید کرده باشه
            if (employee.IsProfileCompletedByEmployee)
                return Json(new { success = false, message = "پروفایل قبلاً تأیید نهایی شده است." });

            // چک حداقل درصد تکمیل (اختیاری — می‌تونی حذف کنی)
            var details = await _employeeService.GetEmployeeDetails(employeeId);
            if (details.ProfileCompletionPercentage < 95)
                return Json(new { success = false, message = "پروفایل شما هنوز کامل نیست. لطفاً تمام مراحل را تکمیل کنید." });

            // تأیید نهایی + ثبت لاگ
            employee.IsProfileCompletedByEmployee = true;

            var success = await _employeeService.Update(employee);

            if (success)
            {
                // اختیاری: لاگ فعالیت
                  _logger.LogInformation($"کارمند {employee.FirstName} {employee.LastName} پروفایل خود را تأیید نهایی کرد.");

                return Json(new { success = true, message = "پروفایل شما با موفقیت تأیید نهایی شد و دیگر قابل ویرایش توسط شما نیست." });
            }

            return Json(new { success = false, message = "خطا در تأیید نهایی پروفایل. لطفاً مجدد تلاش کنید." });
        }

        #endregion

        #region Unlock User Confirm Lock to Re Edit By  Employee
        [Route("UnlockProfile/{employeeId}")]
        [Permission(SystemPermissions.PermissionList.UnlockEmployeeDataForReEditByEmployee)]
        public async Task<IActionResult> UnlockProfile(long employeeId)
        {
            var currentUser = await _userService.GetUserByPhoneNumber(User);
            if (currentUser == null)
            {
                NotificationSystem.ShowNotification(TempData,ApplicationMessages.SessionExpired,"",ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login", "Account");
            }
                

            var employee = await _employeeService.GetEmployeeForEdit(employeeId);
            if (employee == null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                return this.RedirectToReferrer();
            }


            employee.IsProfileCompletedByEmployee = false;
            var success = await _employeeService.Update(employee);

            if (success)
            {
                _logger.LogInformation($"قفل تایید اطلاعات کارمند {employee.FirstName} {employee.LastName} توسط {currentUser.FullName} {currentUser.Id} ریست شد. ");
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.OperationSuccessful, "", ApplicationMessagesIcon.SuccessIcon);
                return this.RedirectToReferrer();
            }

            NotificationSystem.ShowNotification(TempData, ApplicationMessages.OperationFailed, "", ApplicationMessagesIcon.ErrorIcon);
            return this.RedirectToReferrer();
        }

        #endregion
    }
}

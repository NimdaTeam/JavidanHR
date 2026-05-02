using _0_Framework.Extensions;
using _0_Framework.Utilities.Helpers;
using _0_Framework.Utilities.NotificationSystem;
using _0_Framework.Utilities.Pagination;
using _0_Framework.Utilities.Security;
using AngleSharp.Css.Dom;
using AttendanceSystem.Application.Interfaces;
using AttendanceSystem.Domain.Entities;
using AttendanceSystem.Infrastructure.ApiHelper;
using AttendanceSystem.Infrastructure.Dto;
using AuthenticationSystem.Domain.User;
using AuthenticationSystem.Services.Repositories;
using AuthenticationSystem.SystemPermissions;
using Azure;
using DNTPersianUtils.Core;
using HrSystem.Application.Interfaces;
using JavidanHR.WebHost.Utilities;
using JavidanHR.WebHost.Utilities.ReturnUrlFilter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;
using NetTopologySuite.Index.HPRtree;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using WebHost.PageSecurity;
using WebHost.Utilities;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;
using JsonConverter = System.Text.Json.Serialization.JsonConverter;
using JsonSerializer = System.Text.Json.JsonSerializer;
using ManualAttendanceRequest = AttendanceSystem.Infrastructure.Dto.ManualAttendanceRequest;

namespace JavidanHR.WebHost.Controllers
{
    [Route("Attendance")]
    [Authorize]
    public class AttendanceController : BaseController
    {
        private readonly IUserRepository _userService;
        private readonly IEmployeeService _employeeService;
        private readonly IRoleRepository _roleService;
        private readonly IManualAttendanceRequestService _manualAttendanceRequestService;
        private readonly AttendanceSystemApiHelper _attendanceSystemApiHelper;
        private readonly IRequestContextAccessor _ctx;

        public AttendanceController(IUserRepository userService, IEmployeeService employeeService, IRoleRepository roleService, IManualAttendanceRequestService manualAttendanceRequestService, AttendanceSystemApiHelper attendanceSystemApiHelper, IRequestContextAccessor ctx)
        {
            _userService = userService;
            _employeeService = employeeService;
            _roleService = roleService;
            _manualAttendanceRequestService = manualAttendanceRequestService;
            _attendanceSystemApiHelper = attendanceSystemApiHelper;
            _ctx = ctx;
        }


        [Route("All")]
        [Permission(SystemPermissions.PermissionList.AttendanceAllRecordsList)]
        public async Task<IActionResult> All(string? persianDateString, long? userFilter)
        {
            DateTime? dateFilter = null;
            var filteredUser = new Users();
            var apiRequest = new AttendanceRecordsWithRangeRequest();

            if (!string.IsNullOrWhiteSpace(persianDateString))
            {
                dateFilter = persianDateString.ToGregorian();
            }


            if (dateFilter != null)
            {
                apiRequest.From = dateFilter.GetStartOfDate();
                apiRequest.To = dateFilter.GetEndOfDate();
            }
            else
            {
                apiRequest.From = DateTime.Now.GetStartOfDate();
                apiRequest.To = DateTime.Now.GetEndOfDate();
            }


            if (userFilter != null)
            {
                apiRequest.PersonalCode = userFilter.ToString();
                filteredUser = await _userService.GetAsNoTrackingAsync(userFilter.Value);
            }


            var result = await _attendanceSystemApiHelper.GetRangeApiResult(apiRequest);

            #region Pass Information to view
            ViewBag.users = await _employeeService.GetAll();
            ViewBag.filteredUser = filteredUser?.FullName ?? "";
            #endregion

            if (result.ApiResult.Result != "success")
            {
                ShowNotification(ApplicationMessages.OperationFailed, "", ApplicationMessagesIcon.ErrorIcon);
                return SmartRedirect(_ctx.Context.ReturnUrl);
            }

            if (result.Data is null || !result.Data.Items.Any())
                return View(new List<AttendanceLogItem>());

            foreach (var item in result.Data.Items)
            {
                long.TryParse(item.PersonalCode, out var userId);
                var user = await _employeeService.SingleOrDefaultByCondition(x => x.UserId == userId);

                if (user is null)
                {
                    continue;
                }

                item.UserId = user.UserId;
                item.UserFullName = $"{user.FirstName} {user.LastName}";
                item.UserAvatar = user.ProfileImageUrl;
            }

            return View(result.Data.Items.ToList());
        }

        [Route("AllEmployeesMonthlyReport")]
        [Permission(SystemPermissions.PermissionList.AttendanceAllEmployeesMonthlyReport)]
        public async Task<IActionResult> AllEmployeesMonthlyReport(int? year, int? month, long? userFilter)
        {
            var selectedYear = year;
            var selectedMonth = month;
            var currentYear = int.Parse(DateTime.Now.ToShamsi().Split("/")[0]);
            var currentMonth = int.Parse(DateTime.Now.ToShamsi().Split("/")[1]);

            if ((year is null || month is null) ||
                (year > currentYear || year < 1350 || month > 12 || month < 1))
            {
                selectedYear = currentYear;
                selectedMonth = currentMonth;
            }

            var selectedDateTimePersian = $"{selectedYear}/{selectedMonth}/15";
            var selectedDateTime = selectedDateTimePersian.ToGregorianDateOnly();
            var monthStartEnd = selectedDateTime.GetPersianMonthStartAndEndDates();
            var startDate = monthStartEnd!.StartDateOnly.GetStartOfDay().ToShamsi().ToGregorian().GetStartOfDate();
            var endDate = monthStartEnd.EndDateOnly.GetEndOfDay().ToShamsi().ToGregorian().GetStartOfDate();

            var filteredUser = new Users();
            if (userFilter != null)
            {
                filteredUser = await _userService.GetAsync(userFilter.Value);
            }

            #region Pass information to view
            ViewBag.users = await _employeeService.GetAll();
            ViewBag.filteredUser = filteredUser?.FullName ?? "";
            ViewBag.years = DateHelper.GetLast20Years();
            ViewBag.months = DateHelper.GetMonthsOfYear();
            ViewBag.currentYear = selectedYear ?? currentYear;
            ViewBag.currentMonth = selectedMonth ?? currentMonth;
            #endregion

            var employees = await _employeeService.GetAll();

            var tasks = employees.Select(async e =>
            {
                var apiRequest = new FullAttendanceReportRequest()
                {
                    From = startDate,
                    To = endDate,
                    PersonalCode = e.UserId.ToString()
                };

                var result = await _attendanceSystemApiHelper.GetFullAttendanceReport(apiRequest);

                if (result is null || result.Status != "success")
                {
                    return new MonthlyAttendanceReportItem()
                    {
                        UserId = e.UserId,
                        UserFullName = $"{e.FirstName} {e.LastName}",
                        UserAvatar = e.ProfileImageUrl,
                        TotalWorkDays = 0,
                        DurationMinutes = 0,
                        TotalDeficitMinutes = 0,
                        TotalOverTimeMinutes = 0
                    };
                }

                return new MonthlyAttendanceReportItem()
                {
                    UserId = e.UserId,
                    UserFullName = $"{e.FirstName} {e.LastName}",
                    UserAvatar = e.ProfileImageUrl,
                    TotalWorkDays = result.Data.Count(x => x.TotalPresentMins > 0),
                    DurationMinutes = result.Data.Sum(x => x.TotalPresentMins),
                    TotalDeficitMinutes = result.Data.Sum(x => x.DeficitMins),
                    TotalOverTimeMinutes = result.Data.Sum(x => x.ApprovedOtMins)
                };
            });

            // اجرای موازی تمام درخواست‌ها
            var results = await Task.WhenAll(tasks);

            var pageModel = new MonthlyAttendanceReportVM
            {
                Items = results.ToList()
            };
            // =========================================================

            return View(pageModel);
        }

        [Route("UserTodayAttendance")]
        public async Task<IActionResult> UserTodayAttendance()
        {
            var apiRequest = new AttendanceRecordsWithRangeRequest();

            var user = await _userService.GetUserByPhoneNumber(CurrentUsername());
            if (user is null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.SessionExpired, "", ApplicationMessagesIcon.ErrorIcon);
                return SmartRedirect(_ctx.Context.ReturnUrl);
            }

            apiRequest.From = DateTime.Now.GetStartOfDate();
            apiRequest.To = DateTime.Now.GetEndOfDate();
            apiRequest.PersonalCode = user.Id.ToString();

            var result = await _attendanceSystemApiHelper.GetRangeApiResult(apiRequest);

            if (result.ApiResult.Result != "success")
            {
                ShowNotification(ApplicationMessages.OperationFailed, "", ApplicationMessagesIcon.ErrorIcon);
                return SmartRedirect(_ctx.Context.ReturnUrl);
            }

            #region Pass user to view
            ViewBag.user = user;
            #endregion

            if (result.Data is null || !result.Data.Items.Any())
                return View(new List<AttendanceLogItem>());

            return View(result.Data.Items.ToList());
        }

        [Route("EmployeeMonthlyReport")]
        public async Task<IActionResult> EmployeeMonthlyReport(long? userFilter, int? year, int? month) //userFilter is for Filtered User Id
        {
            var apiRequest = new FullAttendanceReportRequest();

            Users? user;
            var currentUser = await _userService.GetUserByPhoneNumber(CurrentUsername());
            if (currentUser is null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.SessionExpired, "", ApplicationMessagesIcon.ErrorIcon);
                return this.RedirectToReferrer();
            }

            if (userFilter is null)
            {
                user = await _userService.GetUserByPhoneNumber(CurrentUsername());
            }
            else
            {
                user = await _userService.GetAsNoTrackingAsync(userFilter.Value);
            }

            if (user is null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                return SmartRedirect(_ctx.Context.ReturnUrl);
            }

            if (userFilter != null && userFilter != currentUser.Id)
            {
                if (!await _roleService.CheckPermission((long)SystemPermissions.PermissionList.AttendanceEmployeeMonthlyReport, currentUser.Id))
                {
                    NotificationSystem.ShowNotification(TempData, ApplicationMessages.AccessDenied, "", ApplicationMessagesIcon.ErrorIcon);
                    return SmartRedirect(_ctx.Context.ReturnUrl);
                }
            }

            var selectedYear = year;
            var selectedMonth = month;

            var currentYear = int.Parse(DateTime.Now.ToShamsi().Split("/")[0]);
            var currentMonth = int.Parse(DateTime.Now.ToShamsi().Split("/")[1]);

            if ((year is null || month is null) ||
                (year > currentYear || year < 1350 || month > 12 || month < 1))
            {
                selectedYear = currentYear;
                selectedMonth = currentMonth;
            }

            var selectedDateTimePersian = $"{selectedYear}/{selectedMonth}/15";

            var selectedDateTime = selectedDateTimePersian.ToGregorianDateOnly();

            var monthStartEnd = selectedDateTime.GetPersianMonthStartAndEndDates();

            var startDate = monthStartEnd!.StartDateOnly.GetStartOfDay().ToShamsi().ToGregorian().GetStartOfDate();
            var endDate = monthStartEnd.EndDateOnly.GetEndOfDay().ToShamsi().ToGregorian().GetStartOfDate();



            apiRequest.From = startDate;
            apiRequest.To = endDate;
            apiRequest.PersonalCode = user.Id.ToString();

            var result = await _attendanceSystemApiHelper.GetFullAttendanceReport(apiRequest);

            if (result is null || result.Status != "success")
            {
                ShowNotification(ApplicationMessages.OperationFailed, "", ApplicationMessagesIcon.ErrorIcon);
                return SmartRedirect(_ctx.Context.ReturnUrl);
            }

            var pageModel = new List<AttendanceRecord>();

            #region Pass information to view
            ViewBag.filteredUser = user;

            ViewBag.years = DateHelper.GetLast20Years();
            ViewBag.months = DateHelper.GetMonthsOfYear();

            ViewBag.currentYear = selectedYear ?? currentYear;
            ViewBag.currentMonth = selectedMonth ?? currentMonth;

            var calendar = selectedYear!.Value.CreatePersianMonthCalendar(selectedMonth!.Value);
            ViewBag.calendar = calendar;

            ViewBag.currentUser = user;

            ViewBag.filteredYear = selectedYear;
            ViewBag.filteredMonth = selectedMonth;
            #endregion

            if (!result.Data.Any())
                return View(pageModel);

            pageModel = result.Data;

            return View(pageModel);
        }

        [Route("AttendanceLiveStatus")]
        [Permission(SystemPermissions.PermissionList.AttendanceLiveStatus)]
        public async Task<IActionResult> AttendanceLiveStatus()
        {
            var apiRequest = new AttendanceRecordsWithRangeRequest()
            {
                From = DateTime.Now.GetStartOfDate(),
                To = DateTime.Now.GetEndOfDate()
            };

            var result = await _attendanceSystemApiHelper.GetRangeApiResult(apiRequest);

            if (result.ApiResult.Result != "success")
            {
                ShowNotification(ApplicationMessages.OperationFailed, "", ApplicationMessagesIcon.ErrorIcon);
                return SmartRedirect(_ctx.Context.ReturnUrl);
            }

            var pageModel = new List<LiveAttendanceStatusVM>();

            if (result.Data is null || !result.Data.Items.Any())
                return View(new List<LiveAttendanceStatusVM>());

            foreach (var item in result.Data.Items)
            {
                long.TryParse(item.PersonalCode, out long userId);
                var user = await _employeeService.SingleOrDefaultByCondition(x => x.UserId == userId);

                if (user is null)
                {
                    continue;
                }

                if (pageModel.All(x => x.UserId != user.UserId))
                {
                    pageModel.Add(new LiveAttendanceStatusVM()
                    {
                        FullName = user.GetFullName(),
                        UserAvatar = user.ProfileImageUrl ?? "",
                        UserId = user.UserId,
                        IsPresent = false,
                        TrafficItems =
                        [
                            new EmployeeMonthlyAttendanceReportItemTrafficItem()
                            {
                                InDateTime = item.InDateTime,
                                IsInManual = bool.Parse(item.IsInManual),

                                OutDateTime = item.OutDateTime,
                                IsOutManual = bool.Parse(item.IsOutManual),
                                TotalDuration = item.DurationMinutes
                            }
                        ]
                    });
                }
                else
                {
                    var foundRecord = pageModel.FirstOrDefault(x => x.UserId == item.UserId);
                    if (foundRecord is null)
                        continue;

                    foundRecord.TrafficItems.Add(new EmployeeMonthlyAttendanceReportItemTrafficItem()
                    {
                        InDateTime = item.InDateTime,
                        IsInManual = bool.Parse(item.IsInManual),

                        OutDateTime = item.OutDateTime,
                        IsOutManual = bool.Parse(item.IsOutManual),
                        TotalDuration = item.DurationMinutes
                    });
                }
            }

            foreach (var item in pageModel.Where(item => item.TrafficItems.Any() &&
                                                         item.TrafficItems.Any(x => x is { InDateTime: not null, OutDateTime: null })))
            {
                item.IsPresent = true;
            }

            var employees = await _employeeService.GetAll();

            foreach (var employee in employees.Where(employee => pageModel.All(x => x.UserId != employee.UserId)))
            {
                pageModel.Add(new LiveAttendanceStatusVM()
                {
                    FullName = employee.GetFullName(),
                    IsPresent = false,
                    UserAvatar = employee.ProfileImageUrl ?? "",
                    UserId = employee.UserId
                });
            }

            return View(pageModel);
        }

        [Route("EditRecord/{sessionId}/{userId}/{workDateString}")]
        public async Task<IActionResult> EditRecord(long sessionId, long userId, string workDateString)
        {
            var user = await _userService.GetAsNoTrackingAsync(userId);
            var currentUser = await _userService.GetUserByPhoneNumber(CurrentUsername());

            if (user is null || currentUser is null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "",
                    ApplicationMessagesIcon.ErrorIcon);
                return SmartRedirect(_ctx.Context.ReturnUrl);
            }


            if (user.Id != currentUser.Id)
            {
                var permissionGranted =
                    await _roleService.CheckPermission(
                        (long)SystemPermissions.PermissionList.AttendanceEditDateRecords,
                        currentUser.Id);

                if (!permissionGranted)
                {
                    ShowNotification(ApplicationMessages.AccessDenied, "", ApplicationMessagesIcon.ErrorIcon);
                    return SmartRedirect(_ctx.Context.ReturnUrl);
                }
            }

            var apiRequest = new AttendanceRecordsWithRangeRequest();

            DateTime.TryParse(workDateString.Replace("-", "/"), out var workDate);

            apiRequest.From = workDate.GetStartOfDate();
            apiRequest.To = workDate.GetEndOfDate();
            apiRequest.PersonalCode = user.Id.ToString();

            var result = await _attendanceSystemApiHelper.GetRangeApiResult(apiRequest);

            if (result.ApiResult.Result != "success")
            {
                ShowNotification(ApplicationMessages.OperationFailed, "", ApplicationMessagesIcon.ErrorIcon);
                return SmartRedirect(_ctx.Context.ReturnUrl);
            }

            #region Pass user to view
            ViewBag.user = user;
            #endregion

            var item = result.Data?.Items.FirstOrDefault(x => x.SessionId == sessionId);

            if (item is not null)
            {
                ViewBag.item = item;
                return View();
            }

            NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
            return this.RedirectToReferrer();
        }

        [Route("EditRecord/{sessionId}/{userId}/{workDateString}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRecord([FromForm] EditAttendanceRecordVM item)
        {
            var user = await _userService.GetAsNoTrackingAsync(item.UserId);
            var currentUser = await _userService.GetUserByPhoneNumber(CurrentUsername());

            if (user is null || currentUser is null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "",
                    ApplicationMessagesIcon.ErrorIcon);
                return SmartRedirect(_ctx.Context.ReturnUrl);
            }


            if (user.Id != currentUser.Id)
            {
                var permissionGranted =
                    await _roleService.CheckPermission(
                        (long)SystemPermissions.PermissionList.AttendanceEditDateRecords,
                        currentUser.Id);

                if (!permissionGranted)
                {
                    ShowNotification(ApplicationMessages.AccessDenied, "", ApplicationMessagesIcon.ErrorIcon);
                    return SmartRedirect(_ctx.Context.ReturnUrl);
                }
            }

            var request = new ManualAttendanceRequest()
            {
                PersonalCode = item.UserId,
                WorkDate = item.WorkDate.ToString(),
                TimesString = $"{item.EntranceTime},{item.ExitTime}",
                ReasonId = 6,
                Description = $"ثبت دستی تردد برای کارمند: {user.FullName} در تاریخ: {DateTime.Now.ToShamsiWithTime()}",
                CreatedById = currentUser.Id
            };


            var result = await _attendanceSystemApiHelper.PostManualAttendanceRequest(request);

            if (result.Result == "success")
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.OperationSuccessful, "", ApplicationMessagesIcon.SuccessIcon);
                return SmartRedirect(_ctx.Context.ReturnUrl);
            }

            NotificationSystem.ShowNotification(TempData, ApplicationMessages.OperationFailed, "", ApplicationMessagesIcon.ErrorIcon);
            return SmartRedirect(_ctx.Context.ReturnUrl);
        }

        [Route("EditDateAttendanceRecords/{userId}")]
        public async Task<IActionResult> EditDateAttendanceRecords(long userId, DateTime workDate)
        {
            var apiRequest = new AttendanceRecordsWithRangeRequest();

            var user = await _userService.GetAsNoTrackingAsync(userId);
            var currentUser = await _userService.GetUserByPhoneNumber(CurrentUsername());

            if (user is null || currentUser is null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "",
                    ApplicationMessagesIcon.ErrorIcon);
                return SmartRedirect(_ctx.Context.ReturnUrl);
            }


            if (user.Id != currentUser.Id)
            {
                var permissionGranted =
                    await _roleService.CheckPermission(
                        (long)SystemPermissions.PermissionList.AttendanceEditDateRecords,
                        currentUser.Id);

                if (!permissionGranted)
                {
                    ShowNotification(ApplicationMessages.AccessDenied, "", ApplicationMessagesIcon.ErrorIcon);
                    return SmartRedirect(_ctx.Context.ReturnUrl);
                }
            }

            apiRequest.From = workDate.GetStartOfDate();
            apiRequest.To = workDate.GetEndOfDate();
            apiRequest.PersonalCode = user.Id.ToString();


            var result = await _attendanceSystemApiHelper.GetRangeApiResult(apiRequest);

            if (result.ApiResult.Result != "success")
            {
                ShowNotification(ApplicationMessages.OperationFailed, "", ApplicationMessagesIcon.ErrorIcon);
                return SmartRedirect(_ctx.Context.ReturnUrl);
            }

            #region Pass user to view
            ViewBag.user = user;
            ViewBag.currentUser = currentUser;
            ViewBag.date = workDate;
            #endregion

            if (result.Data is null || !result.Data.Items.Any())
                return View(new EditAttendanceRecordsVM()
                {
                    UserId = user.Id,
                    LogItems = []
                });

            return View(new EditAttendanceRecordsVM()
            {
                UserId = user.Id,
                LogItems = result.Data.Items.ToList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("EditDateAttendanceRecords/{userId}")]
        public async Task<IActionResult> EditDateAttendanceRecords([FromForm] EditAttendanceRecordsVM data)
        {
            try
            {
                var workDate = data.WorkDate.Date;

                var user = await _userService.GetAsNoTrackingAsync(data.UserId);
                var currentUser = await _userService.GetUserByPhoneNumber(CurrentUsername());

                if (user is null || currentUser is null)
                {
                    NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "",
                        ApplicationMessagesIcon.ErrorIcon);
                    return RedirectToAction("Login","Account");
                }

                var itemsToSendToAttendanceServerDateTimeList = new List<DateTime?>();

                foreach (var item in data.LogItems)
                {
                    if (item.InDateTime == null || item.OutDateTime == null)
                        continue;

                    itemsToSendToAttendanceServerDateTimeList.AddRange([item.InDateTime, item.OutDateTime]);
                }

                itemsToSendToAttendanceServerDateTimeList = itemsToSendToAttendanceServerDateTimeList.Order().ToList();

                if (itemsToSendToAttendanceServerDateTimeList.Count % 2 != 0)
                {
                    ShowNotification(ApplicationMessages.MalformedInput, "", ApplicationMessagesIcon.ErrorIcon);

                    return RedirectToAction(currentUser.Id == user.Id ? "MyManualRequests" : "AllManualRequests");   
                }

                #region Submit Manual Attendance Request By Admin

                if (user.Id != currentUser.Id)
                {
                    var permissionGranted =
                        await _roleService.CheckPermission(
                            (long)SystemPermissions.PermissionList.AttendanceEditDateRecords,
                            currentUser.Id);

                    if (!permissionGranted)
                    {
                        ShowNotification(ApplicationMessages.AccessDenied, "", ApplicationMessagesIcon.ErrorIcon);
                        return RedirectToAction(currentUser.Id == user.Id ? "MyManualRequests" : "AllManualRequests");
                    }


                    var request = new ManualAttendanceRequest()
                    {
                        PersonalCode = user.Id,
                        WorkDate = workDate.ToString(),
                        TimesString = string.Join(",",
                            itemsToSendToAttendanceServerDateTimeList.Select(x =>
                                new string($"{x.Value.Hour:00}:{x.Value.Minute:00}"))),
                        ReasonId = 6,
                        Description =
                            $"ثبت دستی تردد برای کارمند: {user.FullName} در تاریخ: {DateTime.Now.ToShamsiWithTime()} توسط کاربر: {currentUser.FullName}",
                        CreatedById = currentUser.Id
                    };


                    var result = await _attendanceSystemApiHelper.PostManualAttendanceRequest(request);

                    if (result.Result == "success")
                    {
                        NotificationSystem.ShowNotification(TempData, ApplicationMessages.OperationSuccessful, "",
                            ApplicationMessagesIcon.SuccessIcon);
                        return RedirectToAction(currentUser.Id == user.Id ? "MyManualRequests" : "AllManualRequests");
                    }

                    NotificationSystem.ShowNotification(TempData, ApplicationMessages.OperationFailed, "",
                        ApplicationMessagesIcon.ErrorIcon);
                    return RedirectToAction(currentUser.Id == user.Id ? "MyManualRequests" : "AllManualRequests");
                }

                #endregion

                var manualRequest = new AttendanceSystem.Domain.Entities.ManualAttendanceRequest()
                {
                    Reason = data.Reason.SanitizeString(),
                    UserId = currentUser.Id,
                    Username = currentUser.FullName ?? "-",
                    Status = RequestStatus.Pending,
                    AttendanceDate = workDate.Date,
                    AttendanceTimes = itemsToSendToAttendanceServerDateTimeList
                        .Select(x => new string($"{x.Value.Hour:00}:{x.Value.Minute:00}")).ToList(),
                    CreatorId = currentUser.Id
                };

                var addRequestResult = await _manualAttendanceRequestService.Add(manualRequest);

                if (!addRequestResult)
                {
                    ShowNotification(ApplicationMessages.OperationFailed, "", ApplicationMessagesIcon.ErrorIcon);
                    return RedirectToAction(currentUser.Id == user.Id ? "MyManualRequests" : "AllManualRequests");
                }

                ShowNotification("درخواست تردد دستی شما ثبت شد و پس از تأیید کارشناس سیستم در سامانه اعمال خواهد شد");
                return RedirectToAction(currentUser.Id == user.Id ? "MyManualRequests" : "AllManualRequests");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                ShowNotification(ApplicationMessages.OperationFailed, "", ApplicationMessagesIcon.ErrorIcon);
                return SmartRedirect(_ctx.Context.ReturnUrl);
            }
        }

        [Route("GetDailyAttendanceDetail")]
        [HttpGet]
        public async Task<IActionResult> GetDailyAttendanceDetail(long personalCode, string workDate)
        {
            try
            {
                var user = await _userService.GetAsNoTrackingAsync(personalCode);
                var currentUser = await _userService.GetUserByPhoneNumber(CurrentUsername());

                if (user is null || currentUser is null)
                {
                    return Json(new { success = false, message = "کاربری با این اطلاعات پیدا نشد" });
                }

                if (user.Id != currentUser.Id)
                {
                    if (!await _roleService.CheckPermission(
                            (long)SystemPermissions.PermissionList.AttendanceAllRecordsList,
                            currentUser.Id))
                    {
                        return Json(new { success = false, message = "دسترسی مجاز نیست" });
                    }
                }

                var apiRequest = new AttendanceRecordsWithRangeRequest();

                var isDateValid = DateTime.TryParse(workDate, out var validDate);

                if (!isDateValid)
                {
                    return Json(new { success = false, message = "تاریخ ارسال شده صحیح نیست" });
                }

                apiRequest.From = validDate.GetStartOfDate();
                apiRequest.To = validDate.GetEndOfDate();
                apiRequest.PersonalCode = user.Id.ToString();

                var result = await _attendanceSystemApiHelper.GetRangeApiResult(apiRequest);

                if (result.Data is null)
                {
                    return Json(new { success = false, message = "خطا در دریافت اطلاعات" });
                }

                return Json(new { success = true, data = result.Data.Items });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new { success = false, message = "خطا در دریافت اطلاعات: " + e.Message });
            }
        }


        [HttpGet("ExportAttendanceReport")]
        [Permission(SystemPermissions.PermissionList.AttendanceAllEmployeesMonthlyReport)]
        public async Task<IActionResult> ExportAttendanceReport(DateTime from, DateTime to)
        {
            if (from > to)
                return BadRequest("تاریخ شروع باید قبل از تاریخ پایان باشد");

            var employees = await _employeeService.GetAll();
            var allDates = Enumerable.Range(0, (to.Date - from.Date).Days)
                .Select(offset => from.Date.AddDays(offset))
                .ToList();

            var tasks = employees.Select(async employee =>
            {
                var apiRequest = new FullAttendanceReportRequest
                {
                    From = from.ToString("yyyy-MM-dd"),
                    To = to.ToString("yyyy-MM-dd"),
                    PersonalCode = employee.UserId.ToString()
                };

                var apiResult = await _attendanceSystemApiHelper.GetFullAttendanceReport(apiRequest);

                if (apiResult is null || apiResult.Status != "success")
                {
                    return new EmployeeReportData
                    {
                        EmployeeName = $"{employee.FirstName} {employee.LastName}",
                        TotalRegularMinutes = 0,
                        TotalOvertimeMinutes = 0,
                        TotalDeficitMinutes = 0,
                        AbsenceDays = allDates.Count
                    };
                }

                var itemsDict = apiResult.Data.ToDictionary(
                    item => DateTime.Parse(item.WorkDate).Date,
                    item => item);

                long regularMinutes = 0;
                long overtimeMinutes = 0;
                long deficitMinutes = 0;
                int absenceDays = 0;
                long thursdayOverTime = 0;

                foreach (var date in allDates)
                {
                    bool isHoliday = date.IsHoliday();
                    bool isThursday = date.GetPersianWeekDayNumber() == 6;

                    itemsDict.TryGetValue(date, out var item);

                    // روز تعطیل رسمی (جمعه یا تعطیلات ملی)
                    if (isHoliday)
                    {
                        if (item != null && item.TotalPresentMins > 0)
                        {
                            overtimeMinutes += item.TotalPresentMins + item.ApprovedOtMins;
                        }
                        continue; // تعطیل: بدون غیبت و بدون کارکرد عادی
                    }

                    // روز غیرتعطیل
                    if (item == null)
                    {
                        absenceDays++;
                        continue;
                    }

                    if (item.Status.Equals("Absent", StringComparison.OrdinalIgnoreCase) || item.TotalPresentMins == 0)
                    {
                        absenceDays++;
                        continue;
                    }

                    // روز حضور دارد
                    if (isThursday)
                    {

                        overtimeMinutes += item.TotalPresentMins - 0; //pending over time;
                    }
                    else
                    {
                        regularMinutes += item.TotalPresentMins;
                        overtimeMinutes += item.ApprovedOtMins;
                        deficitMinutes += item.DeficitMins;
                    }
                }

                return new EmployeeReportData
                {
                    EmployeeName = $"{employee.FirstName} {employee.LastName}",
                    TotalRegularMinutes = regularMinutes,
                    TotalOvertimeMinutes = overtimeMinutes,
                    TotalDeficitMinutes = deficitMinutes,
                    AbsenceDays = absenceDays
                };
            });

            var results = await Task.WhenAll(tasks);

            // تولید فایل اکسل (مثال با EPPlus)
            ExcelPackage.License.SetNonCommercialPersonal("YourName");
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Report");

            worksheet.Cells[1, 1].Value = "نام کارمند";
            worksheet.Cells[1, 2].Value = "کل ساعات کارکرد عادی";
            worksheet.Cells[1, 3].Value = "ساعات اضافه کار";
            worksheet.Cells[1, 4].Value = "ساعات کسری";
            worksheet.Cells[1, 5].Value = "تعداد روز غیبت";

            int row = 2;
            foreach (var data in results)
            {
                worksheet.Cells[row, 1].Value = data.EmployeeName;
                worksheet.Cells[row, 2].Value = int.Parse(data.TotalRegularMinutes.ToString()).ToHourMinuteFormat();
                worksheet.Cells[row, 3].Value = int.Parse(data.TotalOvertimeMinutes.ToString()).ToHourMinuteFormat();
                worksheet.Cells[row, 4].Value = int.Parse(data.TotalDeficitMinutes.ToString()).ToHourMinuteFormat();
                worksheet.Cells[row, 5].Value = data.AbsenceDays;
                row++;
            }

            worksheet.Cells.AutoFitColumns();
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            string fileName = $"AttendanceReport_{from:yyyyMMdd}_to_{to:yyyyMMdd}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        private class EmployeeReportData
        {
            public string EmployeeName { get; set; }
            public long TotalRegularMinutes { get; set; }
            public long TotalOvertimeMinutes { get; set; }
            public long TotalDeficitMinutes { get; set; }
            public int AbsenceDays { get; set; }
        }

        #region Manual Requests
        [Route("ManualRequests/All")]
        [Permission(SystemPermissions.PermissionList.AttendanceAllManualRequestList)]
        public async Task<IActionResult> AllManualRequests(long? userFilter, int? status, int page = 1)
        {
            var allRequests = await _manualAttendanceRequestService.GetAll();

            var filteredRequests = allRequests.AsEnumerable();

            if (userFilter.HasValue)
            {
                filteredRequests = filteredRequests.Where(x => x.UserId == userFilter.Value);
            }

            if (status.HasValue)
            {
                filteredRequests = filteredRequests.Where(x => (int)x.Status == status.Value);
            }

            var requests = filteredRequests.ToList();

            var pageModel = PaginationHelper.Paginate(
                new PaginationRequest<AttendanceSystem.Domain.Entities.ManualAttendanceRequest>()
                {
                    CurrentPage = page,
                    ModelList = requests,
                    SearchQuery = ""
                });

            #region Pass Items to view
            ViewBag.users = await _employeeService.GetAll();
            #endregion    

            return View(pageModel);
        }

        [Route("ManualRequests/approve/{reqId}")]
        [Permission(SystemPermissions.PermissionList.AttendanceApproveManualRequest)]
        public async Task<IActionResult> ApproveManualRequest(long reqId)
        {
            var rangeApiRequest = new AttendanceRecordsWithRangeRequest();

            var request = await _manualAttendanceRequestService.GetById(reqId);
            if (request == null)
            {
                ShowNotification(ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("AllManualRequests");
            }

            var currentUser = await _userService.GetUserByPhoneNumber(CurrentUsername());
            var requestUser = await _userService.GetAsync(request.UserId);
            if (currentUser == null || requestUser == null)
            {
                ShowNotification(ApplicationMessages.SessionExpired, "", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("AllManualRequests");
            }

            var workDate = request.AttendanceDate.Date;

            var itemsToSendToAttendanceServerDateTimeList = new List<DateTime?>();

            rangeApiRequest.From = workDate.GetStartOfDate();
            rangeApiRequest.To = workDate.GetEndOfDate();
            rangeApiRequest.PersonalCode = request.UserId.ToString();

            var currentItems = await _attendanceSystemApiHelper.GetRangeApiResult(rangeApiRequest);

            if (currentItems.ApiResult.Result != "success")
            {
                ShowNotification(ApplicationMessages.OperationFailed, "", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("AllManualRequests");
            }

            if (currentItems.Data != null)
            {
                foreach (var item in currentItems.Data.Items)
                {
                    if (item.InDateTime == null || item.OutDateTime == null)
                        continue;

                    itemsToSendToAttendanceServerDateTimeList.AddRange([item.InDateTime, item.OutDateTime]);
                }
            }

            foreach (var t in request.AttendanceTimes)
            {
                DateTime.TryParse(t, out var time);
                itemsToSendToAttendanceServerDateTimeList.Add(time);
            }

            itemsToSendToAttendanceServerDateTimeList = itemsToSendToAttendanceServerDateTimeList.Order().ToList();

            if (itemsToSendToAttendanceServerDateTimeList.Count % 2 != 0)
            {
                ShowNotification(ApplicationMessages.MalformedInput, "", ApplicationMessagesIcon.ErrorIcon);
                return SmartRedirect(_ctx.Context.ReturnUrl);
            }

            #region Submit Manual Attendance Request By Admin
            var manualRequest = new ManualAttendanceRequest()
            {
                PersonalCode = request.UserId,
                WorkDate = workDate.ToString(),
                TimesString = string.Join(",",
                    itemsToSendToAttendanceServerDateTimeList.Select(x =>
                        new string($"{x.Value.Hour:00}:{x.Value.Minute:00}"))),
                ReasonId = 6,
                Description =
                    $"ثبت دستی تردد برای کارمند: {requestUser.FullName} در تاریخ: {DateTime.Now.ToShamsiWithTime()} توسط کاربر: {currentUser.FullName}",
                CreatedById = currentUser.Id
            };


            var result = await _attendanceSystemApiHelper.PostManualAttendanceRequest(manualRequest);
            #endregion



            var updateResult = await _manualAttendanceRequestService.Approve(request.Id, currentUser.Id);

            if (result.Result != "success" || !updateResult)
            {
                ShowNotification(ApplicationMessages.OperationFailed, "", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("AllManualRequests");
            }

            ShowNotification(ApplicationMessages.OperationSuccessful, "", ApplicationMessagesIcon.SuccessIcon);
            return RedirectToAction("AllManualRequests");
        }


        [Route("ManualRequests/reject/{reqId}")]
        [Permission(SystemPermissions.PermissionList.AttendanceRejectManualRequest)]
        public async Task<IActionResult> RejectManualRequest(long reqId)
        {
            var request = await _manualAttendanceRequestService.GetById(reqId);
            if (request == null)
            {
                ShowNotification(ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("AllManualRequests");
            }

            var currentUser = await _userService.GetUserByPhoneNumber(CurrentUsername());
            var requestUser = await _userService.GetAsNoTrackingAsync(request.UserId);
            if (currentUser == null || requestUser == null)
            {
                ShowNotification(ApplicationMessages.SessionExpired, "", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("AllManualRequests");
            }

            if (request.Status == RequestStatus.Approved)
            {
                var workDate = request.AttendanceDate.Date;
                var itemsToSendToAttendanceServerDateTimeList = new List<DateTime?>();

                var rangeApiRequest = new AttendanceRecordsWithRangeRequest
                {
                    From = workDate.GetStartOfDate(),
                    To = workDate.GetEndOfDate(),
                    PersonalCode = request.UserId.ToString()
                };

                var currentItems = await _attendanceSystemApiHelper.GetRangeApiResult(rangeApiRequest);

                if (currentItems.ApiResult.Result != "success")
                {
                    ShowNotification(ApplicationMessages.OperationFailed, "", ApplicationMessagesIcon.ErrorIcon);
                    return RedirectToAction("AllManualRequests");
                }

                if (currentItems.Data != null)
                {
                    foreach (var item in currentItems.Data.Items)
                    {
                        if (item.InDateTime == null || item.OutDateTime == null)
                            continue;

                        var inDateTime = $"{item.InDateTime.Value.Hour:00}:{item.InDateTime.Value.Minute:00}";
                        var outDateTime = $"{item.OutDateTime.Value.Hour:00}:{item.OutDateTime.Value.Minute:00}";

                        if (request.AttendanceTimes.Any(x => x == inDateTime) ||
                            request.AttendanceTimes.Any(x => x == outDateTime))
                        {
                            continue;
                        }
                        itemsToSendToAttendanceServerDateTimeList.AddRange([item.InDateTime, item.OutDateTime]);
                    }
                }

                itemsToSendToAttendanceServerDateTimeList = itemsToSendToAttendanceServerDateTimeList.Order().ToList();

                if (itemsToSendToAttendanceServerDateTimeList.Count % 2 != 0)
                {
                    ShowNotification(ApplicationMessages.MalformedInput, "", ApplicationMessagesIcon.ErrorIcon);
                    return SmartRedirect(_ctx.Context.ReturnUrl);
                }

                #region Submit Manual Attendance Request By Admin
                var manualRequest = new ManualAttendanceRequest()
                {
                    PersonalCode = request.UserId,
                    WorkDate = workDate.ToString(),
                    TimesString = string.Join(",",
                        itemsToSendToAttendanceServerDateTimeList.Select(x =>
                            new string($"{x.Value.Hour:00}:{x.Value.Minute:00}"))),
                    ReasonId = 6,
                    Description =
                        $"ثبت دستی تردد برای کارمند: {requestUser.FullName} در تاریخ: {DateTime.Now.ToShamsiWithTime()} توسط کاربر: {currentUser.FullName}",
                    CreatedById = currentUser.Id
                };


                var result = await _attendanceSystemApiHelper.PostManualAttendanceRequest(manualRequest);
                if (result.Result == "success")
                {
                    var rejectResult = await _manualAttendanceRequestService.Reject(request.Id, currentUser.Id, "-");

                    if (!rejectResult)
                    {
                        ShowNotification(ApplicationMessages.OperationFailed, "", ApplicationMessagesIcon.ErrorIcon);
                        return RedirectToAction("AllManualRequests");
                    }

                    ShowNotification(ApplicationMessages.OperationSuccessful, "", ApplicationMessagesIcon.SuccessIcon);
                    return RedirectToAction("AllManualRequests");
                }
                ShowNotification(ApplicationMessages.OperationSuccessful, "", ApplicationMessagesIcon.SuccessIcon);
                return RedirectToAction("AllManualRequests");
                #endregion
            }

            var updateResult = await _manualAttendanceRequestService.Reject(request.Id, currentUser.Id, "-");

            if (!updateResult)
            {
                ShowNotification(ApplicationMessages.OperationFailed, "", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("AllManualRequests");
            }

            ShowNotification(ApplicationMessages.OperationSuccessful, "", ApplicationMessagesIcon.SuccessIcon);
            return RedirectToAction("AllManualRequests");
        }

        [Route("ManualRequests/MyRequests")]
        public async Task<IActionResult> MyManualRequests(int? status, int page = 1)
        {
            var currentUser = await _userService.GetUserByPhoneNumber(CurrentUsername());
            if (currentUser == null)
            {
                ShowNotification(ApplicationMessages.SessionExpired,"",ApplicationMessagesIcon.ErrorIcon);
                return SmartRedirect(_ctx.Context.ReturnUrl);
            }

            var allRequests = await _manualAttendanceRequestService.GetByCondition(x=>x.UserId == currentUser.Id);

            var filteredRequests = allRequests.AsEnumerable();

            if (status.HasValue)
            {
                filteredRequests = filteredRequests.Where(x => (int)x.Status == status.Value);
            }

            var requests = filteredRequests.ToList();

            var pageModel = PaginationHelper.Paginate(
                new PaginationRequest<AttendanceSystem.Domain.Entities.ManualAttendanceRequest>()
                {
                    CurrentPage = page,
                    ModelList = requests,
                    SearchQuery = ""
                });

            #region Pass Items to view
            ViewBag.user = currentUser;
            #endregion    

            return View(pageModel);
        }

        [Route("ManualRequests/Delete/{reqId}")]
        public async Task<IActionResult> DeleteManualRequest(long reqId)
        {
            var request = await _manualAttendanceRequestService.GetById(reqId);
            if (request is null)
            {
                ShowNotification(ApplicationMessages.NotFound,"",ApplicationMessagesIcon.ErrorIcon);
                return SmartRedirect(_ctx.Context.ReturnUrl);
            }

            var requestUser = await _userService.GetAsNoTrackingAsync(request.UserId);
            var currentUser = await _userService.GetUserByPhoneNumber(CurrentUsername());

            if (requestUser is null || currentUser is null)
            {
                ShowNotification(ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login","Account");
            }

            if (request.Status != RequestStatus.Pending)
            {
                ShowNotification("امکان حذف درخواست تأیید/رد شده وجود ندارد","",ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction(currentUser.Id == requestUser.Id ? "MyManualRequests" : "AllManualRequests");
            }


            if (currentUser.Id != requestUser.Id)
            {
                var permissionGranted =
                    await _roleService.CheckPermission(
                        (long)SystemPermissions.PermissionList.AttendanceDeleteManualRequest, currentUser.Id);

                if (!permissionGranted)
                {
                    ShowNotification(ApplicationMessages.AccessDenied,"",ApplicationMessagesIcon.ErrorIcon);
                    return RedirectToAction(currentUser.Id == requestUser.Id ? "MyManualRequests" : "AllManualRequests");
                }
            }

            var deleteResult = await _manualAttendanceRequestService.Delete(request);

            if (!deleteResult)
            {
                ShowNotification(ApplicationMessages.OperationFailed, "", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction(currentUser.Id == requestUser.Id ? "MyManualRequests" : "AllManualRequests");
            }

            ShowNotification(ApplicationMessages.OperationSuccessful, "", ApplicationMessagesIcon.SuccessIcon);
            return RedirectToAction(currentUser.Id == requestUser.Id ? "MyManualRequests" : "AllManualRequests");
        }
        #endregion            
    }
}

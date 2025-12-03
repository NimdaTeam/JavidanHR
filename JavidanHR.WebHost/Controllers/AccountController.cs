using System.Security.Claims;
using System.Text.Json;
using _0_Framework.DTO;
using _0_Framework.Utilities.Helpers;
using _0_Framework.Utilities.NotificationSystem;
using _0_Framework.Utilities.Security;
using _0_Framework.Utilities.SMSSender;
using AuthenticationSystem.Domain.User;
using AuthenticationSystem.Services.Repositories;
using AuthenticationSystem.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UAParser;
using WebHost.Utilities;
using static AuthenticationSystem.Utilities.PasswordSecurity;

namespace JavidanHR.WebHost.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userService;
        private readonly IRoleRepository _roleService;
        private readonly RedisService _redisService;


        public AccountController(IUserRepository userService, IRoleRepository roleService, RedisService redisService)
        {
            _userService = userService;
            _roleService = roleService;
            _redisService = redisService;
        }

        [Route("Login")]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWithPassword(string phoneNumber, string password)
        {
            var ipAddress = GetClientIp();
            var userAgent = Request.Headers["User-Agent"].ToString();
            var deviceInfo = ParseUserAgent(userAgent);

            // ۱. اعتبارسنجی شماره
            if (string.IsNullOrWhiteSpace(phoneNumber) || !PhoneNumberValidator.IsValid(phoneNumber.SanitizeString()))
            {
                await LogFailedLoginAsync(null, phoneNumber, ipAddress, userAgent, deviceInfo, "InvalidPhoneNumber",
                    LoginMethod.Password);
                NotificationSystem.ShowNotification(TempData, "شماره تلفن نامعتبر",
                    "شماره تلفن وارد شده معتبر نیست.", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login");
            }

            var sanitizedPhone = phoneNumber.SanitizeString();
            var user = await _userService.GetUserByPhoneNumber(sanitizedPhone);

            // ۲. کاربر وجود ندارد یا حذف شده
            if (user == null || user.IsDeleted)
            {
                await LogFailedLoginAsync(null, sanitizedPhone, ipAddress, userAgent, deviceInfo, "UserNotFound",
                    LoginMethod.Password);
                NotificationSystem.ShowNotification(TempData, "اطلاعات ورود اشتباه است",
                    "شماره تلفن یا رمز عبور صحیح نمی‌باشد.", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login");
            }

            // ۳. حساب غیرفعال
            if (!user.IsActive)
            {
                await LogFailedLoginAsync(user.Id, sanitizedPhone, ipAddress, userAgent, deviceInfo, "AccountDisabled",
                    LoginMethod.Password);
                NotificationSystem.ShowNotification(TempData, "حساب غیرفعال",
                    "حساب کاربری شما غیرفعال است. با پشتیبانی تماس بگیرید.", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login");
            }

            // ۴. حساب قفل موقت (Brute Force Protection)
            if (user.IsLockedOut && user.LockoutEnd > DateTime.UtcNow)
            {
                var remaining = (user.LockoutEnd.Value - DateTime.UtcNow);
                await LogFailedLoginAsync(user.Id, sanitizedPhone, ipAddress, userAgent, deviceInfo, "AccountLocked",
                    LoginMethod.Password);
                NotificationSystem.ShowNotification(TempData, "حساب قفل شده",
                    $"به دلیل تلاش‌های ناموفق، حساب شما تا {remaining.Minutes + 1} دقیقه قفل است.",
                    ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login");
            }

            // اگر قفل منقضی شده بود → بازش کن
            if (user.IsLockedOut && user.LockoutEnd <= DateTime.UtcNow)
            {
                user.IsLockedOut = false;
                user.AccessFailedCount = 0;
                user.LockoutEnd = null;
            }

            // ۵. بررسی رمز عبور
            bool passwordValid = _userService.VerifyPassword(user.PasswordHash, password);

            if (!passwordValid)
            {
                user.AccessFailedCount += 1;
                user.LastLoginAt = DateTime.UtcNow; // حتی ناموفق هم آپدیت کن (برای مانیتورینگ)

                // قفل بعد از ۵ تلاش
                if (user.AccessFailedCount >= 5)
                {
                    user.IsLockedOut = true;
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                    await LogFailedLoginAsync(user.Id, sanitizedPhone, ipAddress, userAgent, deviceInfo,
                        "TooManyFailedAttempts", LoginMethod.Password);
                    NotificationSystem.ShowNotification(TempData, "حساب قفل شد",
                        "به دلیل تلاش‌های ناموفق زیاد، حساب شما ۱۵ دقیقه قفل شد.", ApplicationMessagesIcon.ErrorIcon);
                }
                else
                {
                    await LogFailedLoginAsync(user.Id, sanitizedPhone, ipAddress, userAgent, deviceInfo,
                        "WrongPassword", LoginMethod.Password);
                    NotificationSystem.ShowNotification(TempData, "رمز اشتباه",
                        $"رمز عبور اشتباه است ({user.AccessFailedCount}/5)", ApplicationMessagesIcon.ErrorIcon);
                }

                await _userService.Update(user);
                await _userService.SaveChanges();
                return RedirectToAction("Login");
            }

            // ۶. ورود موفق — ریست همه چیز
            user.AccessFailedCount = 0;
            user.IsLockedOut = false;
            user.LockoutEnd = null;
            user.LastLoginAt = DateTime.UtcNow;
            user.LastLoginIp = ipAddress;

            await _userService.Update(user);
            await _userService.SaveChanges();

            // ۷. دریافت پرمیشن‌ها
            var permissions = (await _roleService.GetUserPermissions(user.Id))
                .Select(x => x.Permission)
                .ToList();

            var permissionsJson = JsonSerializer.Serialize(permissions);

            // ۸. ساخت Claim ها
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.PhoneNumber),
                new Claim("permissions", permissionsJson),
                new Claim("fullName", user.FullName ?? ""),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var properties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);

            // ۹. لاگ ورود موفق
            await LogSuccessfulLoginAsync(user, ipAddress, userAgent, deviceInfo, LoginMethod.Password);

            NotificationSystem.ShowNotification(TempData, $"ورود موفقیت‌آمیز بود، {user.FullName ?? "کاربر "} عزیز خوش آمدید.",
                $"ورود موفقیت‌آمیز بود، {user.FullName ?? "کاربر عزیز"}", ApplicationMessagesIcon.SuccessIcon);

            return Redirect($"{Request.Scheme}://{Request.Host}{Request.PathBase}/");
        }


        [HttpPost]
        [Route("SendOtp")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendOtp(string phoneNumber)
        {
            var ipAddress = GetClientIp();
            var userAgent = Request.Headers["User-Agent"].ToString();
            var deviceInfo = ParseUserAgent(userAgent);

            // ۱. اعتبارسنجی شماره تلفن
            if (string.IsNullOrWhiteSpace(phoneNumber) || !PhoneNumberValidator.IsValid(phoneNumber.SanitizeString()))
            {
                await LogFailedLoginAsync(null, phoneNumber, ipAddress, userAgent, deviceInfo, "InvalidPhoneNumber",
                    LoginMethod.Otp);
                NotificationSystem.ShowNotification(TempData, "شماره تلفن نامعتبر",
                    "شماره تلفن وارد شده معتبر نیست.", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login");
            }

            var sanitizedPhone = phoneNumber.SanitizeString();

            // ۲. حتماً کاربر باید قبلاً ثبت شده باشد — ایجاد خودکار ممنوع!
            var user = await _userService.GetUserByPhoneNumber(sanitizedPhone);
            if (user == null || user.IsDeleted)
            {
                await LogFailedLoginAsync(null, sanitizedPhone, ipAddress, userAgent, deviceInfo, "UserNotFound",
                    LoginMethod.Otp);
                NotificationSystem.ShowNotification(TempData, "کاربر ثبت‌نام نشده",
                    "شماره تلفن وارد شده در سیستم ثبت نشده است. لطفاً ابتدا ثبت‌نام کنید.",
                    ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login");
            }

            // ۳. حساب غیرفعال
            if (!user.IsActive)
            {
                await LogFailedLoginAsync(user.Id, sanitizedPhone, ipAddress, userAgent, deviceInfo, "AccountDisabled",
                    LoginMethod.Otp);
                NotificationSystem.ShowNotification(TempData, "حساب غیرفعال",
                    "حساب کاربری شما غیرفعال است. با پشتیبانی تماس بگیرید.", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login");
            }

            // ۴. حساب قفل شده → حتی اجازه ارسال OTP ندیم!
            if (user.IsLockedOut && user.LockoutEnd > DateTime.UtcNow)
            {
                var remaining = (user.LockoutEnd.Value - DateTime.UtcNow);
                await LogFailedLoginAsync(user.Id, sanitizedPhone, ipAddress, userAgent, deviceInfo,
                    "AccountLocked_NoOtpAllowed", LoginMethod.Otp);
                NotificationSystem.ShowNotification(TempData, "حساب قفل شده",
                    $"حساب شما قفل است. تا {remaining.Minutes + 1} دقیقه دیگر نمی‌توانید کد دریافت کنید.",
                    ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login");
            }

            // ۵. محدودیت ارسال OTP (مثلاً ۳ بار در ۱۰ دقیقه)
            var recentOtps = await _userService.CountRecentOtpCodesForPhoneNumber(phoneNumber);

            if (recentOtps >= 3)
            {
                // اختیاری: قفل موقت برای ارسال OTP
                user.IsLockedOut = true;
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(10);
                await _userService.Update(user);
                await _userService.SaveChanges();

                await LogFailedLoginAsync(user.Id, sanitizedPhone, ipAddress, userAgent, deviceInfo,
                    "OtpRateLimitExceeded", LoginMethod.Otp);
                NotificationSystem.ShowNotification(TempData, "محدودیت ارسال",
                    "تعداد درخواست کد یکبار مصرف بیش از حد مجاز است. ۱۰ دقیقه صبر کنید.",
                    ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login");
            }

            // ۶. تولید و ارسال OTP
            var otp = await _roleService.GenerateOTPCode(user);
            var smsResult = await SMSSender.SendSmsAsync(sanitizedPhone, "otp", new { code = otp });

            if (!smsResult.IsSuccessful)
            {
                await LogFailedLoginAsync(user.Id, sanitizedPhone, ipAddress, userAgent, deviceInfo, "SmsSendFailed",
                    LoginMethod.Otp);
                NotificationSystem.ShowNotification(TempData, "خطای ارسال",
                    "ارسال پیامک با مشکل مواجه شد. دوباره تلاش کنید.", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login");
            }

            // ۷. لاگ ارسال موفق OTP
            await LogSuccessfulOtpRequestAsync(user, ipAddress, userAgent, deviceInfo);

            NotificationSystem.ShowNotification(TempData, "کد ارسال شد",
                $"کد یکبار مصرف به شماره {sanitizedPhone} ارسال شد.", ApplicationMessagesIcon.SuccessIcon);

            return Redirect($"/OtpConfirmation/{sanitizedPhone}");
        }


        [Route("OtpConfirmation/{phoneNumber}")]
        public async Task<IActionResult> OtpConfirmation(string phoneNumber)
        {
            var user = await _userService.GetUserByPhoneNumber(phoneNumber.SanitizeString());

            return View(user);
        }


        [Route("OtpConfirmation/{phoneNumber}")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> OtpConfirmation(string phoneNumber, string otpCode)
        {
            var ipAddress = GetClientIp();
            var userAgent = Request.Headers["User-Agent"].ToString();
            var deviceInfo = ParseUserAgent(userAgent);

            // ۱. اعتبارسنجی شماره تلفن
            if (string.IsNullOrWhiteSpace(phoneNumber) || !PhoneNumberValidator.IsValid(phoneNumber.SanitizeString()))
            {
                await LogFailedLoginAsync(null, phoneNumber, ipAddress, userAgent, deviceInfo, "InvalidPhoneNumber",
                    LoginMethod.Otp);
                NotificationSystem.ShowNotification(TempData, "شماره تلفن نامعتبر",
                    "شماره تلفن وارد شده معتبر نیست.", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login");
            }

            var sanitizedPhone = phoneNumber.SanitizeString();
            var user = await _userService.GetUserByPhoneNumber(sanitizedPhone);

            // ۲. کاربر وجود ندارد
            if (user == null || user.IsDeleted)
            {
                await LogFailedLoginAsync(null, sanitizedPhone, ipAddress, userAgent, deviceInfo, "UserNotFound",
                    LoginMethod.Otp);
                NotificationSystem.ShowNotification(TempData, "کاربر یافت نشد",
                    "کاربری با این شماره تلفن ثبت نشده است.", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login");
            }

            // ۳. حساب غیرفعال
            if (!user.IsActive)
            {
                await LogFailedLoginAsync(user.Id, sanitizedPhone, ipAddress, userAgent, deviceInfo, "AccountDisabled",
                    LoginMethod.Otp);
                NotificationSystem.ShowNotification(TempData, "حساب غیرفعال",
                    "حساب کاربری شما غیرفعال است. با پشتیبانی تماس بگیرید.", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login");
            }

            // ۴. حساب قفل شده (حتی با OTP هم نباید اجازه ورود بده!)
            if (user.IsLockedOut && user.LockoutEnd > DateTime.UtcNow)
            {
                var remaining = (user.LockoutEnd.Value - DateTime.UtcNow);
                await LogFailedLoginAsync(user.Id, sanitizedPhone, ipAddress, userAgent, deviceInfo, "AccountLocked",
                    LoginMethod.Otp);
                NotificationSystem.ShowNotification(TempData, "حساب قفل شده",
                    $"حساب شما تا {remaining.Minutes + 1} دقیقه دیگر قفل است.", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login");
            }

            // ۵. بررسی کد OTP
            var isValidOtp = await _roleService.VerifyOTPCode(otpCode.SanitizeString(), sanitizedPhone);

            if (!isValidOtp)
            {
                user.AccessFailedCount += 1;
                user.LastLoginAt = DateTime.UtcNow;

                // قفل حساب بعد از ۵ تلاش اشتباه در OTP هم!
                if (user.AccessFailedCount >= 5)
                {
                    user.IsLockedOut = true;
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);

                    await LogFailedLoginAsync(user.Id, sanitizedPhone, ipAddress, userAgent, deviceInfo,
                        "InvalidOtp_TooManyAttempts", LoginMethod.Otp);
                    NotificationSystem.ShowNotification(TempData, "حساب قفل شد",
                        "به دلیل تلاش‌های ناموفق زیاد (حتی با کد یکبار مصرف)، حساب شما ۱۵ دقیقه قفل شد.",
                        ApplicationMessagesIcon.ErrorIcon);
                }
                else
                {
                    await LogFailedLoginAsync(user.Id, sanitizedPhone, ipAddress, userAgent, deviceInfo, "InvalidOtp",
                        LoginMethod.Otp);
                    NotificationSystem.ShowNotification(TempData, "کد اشتباه",
                        $"کد یکبار مصرف اشتباه است ({user.AccessFailedCount}/5)", ApplicationMessagesIcon.ErrorIcon);
                }

                await _userService.Update(user);
                await _userService.SaveChanges();

                return View("OtpConfirmation", user); // برگرده به همون صفحه با خطا
            }

            // ۶. OTP صحیح → ورود موفق
            user.AccessFailedCount = 0;
            user.IsLockedOut = false;
            user.LockoutEnd = null;
            user.LastLoginAt = DateTime.UtcNow;
            user.LastLoginIp = ipAddress;

            await _userService.Update(user);
            await _userService.SaveChanges();

            // ۷. دریافت پرمیشن‌ها
            var permissions = (await _roleService.GetUserPermissions(user.Id))
                .Select(x => x.Permission)
                .ToList();

            var permissionsJson = JsonSerializer.Serialize(permissions);

            // ۸. ساخت Claim ها
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.PhoneNumber),
                new Claim("permissions", permissionsJson),
                new Claim("fullName", user.FullName ?? ""),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var properties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);

            // ۹. لاگ ورود موفق با OTP
            await LogSuccessfulLoginAsync(user, ipAddress, userAgent, deviceInfo, LoginMethod.Otp);

            NotificationSystem.ShowNotification(TempData, "خوش آمدید!",
                $"ورود با کد یکبار مصرف موفقیت‌آمیز بود، {user.FullName ?? "کاربر عزیز"}",
                ApplicationMessagesIcon.SuccessIcon);


            return string.IsNullOrWhiteSpace(user.PasswordHash)
                ? Redirect(
                    $"{Request.Scheme}://{Request.Host}{Request.PathBase}/Account/SetPassword/{user.PhoneNumber}")
                : Redirect($"{Request.Scheme}://{Request.Host}{Request.PathBase}/");
        }


        [Route("LogOut")]
        public async Task<IActionResult> LogOut()
        {
            //ITicketStore will delete user info from redis automatically when signing out.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            foreach (var cookieKey in Request.Cookies.Keys.Where(k => k.StartsWith("AspNetCore.")))
            {
                Response.Cookies.Delete(cookieKey);
            }

            return RedirectToAction("Login");
        }


        [HttpPost]
        [Route("ResendOTP")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ResendOtp([FromBody] ResendOtpRequest request)
        {
            var ipAddress = GetClientIp();
            var userAgent = Request.Headers["User-Agent"].ToString();
            var deviceInfo = ParseUserAgent(userAgent);

            var phoneNumber = request.PhoneNumber?.SanitizeString();

            // ۱. اعتبارسنجی شماره تلفن
            if (string.IsNullOrWhiteSpace(phoneNumber) || !PhoneNumberValidator.IsValid(phoneNumber))
            {
                await LogFailedLoginAsync(null, phoneNumber ?? "0", ipAddress, userAgent, deviceInfo,
                    "InvalidPhoneNumber_Resend", LoginMethod.Otp);
                return Json(new { success = false, result = "شماره تلفن وارد شده معتبر نیست." });
            }

            // ۲. کاربر باید وجود داشته باشه
            var user = await _userService.GetUserByPhoneNumber(phoneNumber);
            if (user == null || user.IsDeleted)
            {
                await LogFailedLoginAsync(null, phoneNumber, ipAddress, userAgent, deviceInfo, "UserNotFound_Resend",
                    LoginMethod.Otp);
                return Json(new { success = false, result = "کاربری با این شماره ثبت نشده است." });
            }

            // ۳. حساب غیرفعال
            if (!user.IsActive)
            {
                await LogFailedLoginAsync(user.Id, phoneNumber, ipAddress, userAgent, deviceInfo,
                    "AccountDisabled_Resend", LoginMethod.Otp);
                return Json(new { success = false, result = "حساب کاربری شما غیرفعال است." });
            }

            // ۴. حساب قفل شده → حتی Resend هم ممنوع!
            if (user.IsLockedOut && user.LockoutEnd > DateTime.UtcNow)
            {
                var remaining = (user.LockoutEnd.Value - DateTime.UtcNow).Minutes + 1;
                await LogFailedLoginAsync(user.Id, phoneNumber, ipAddress, userAgent, deviceInfo,
                    "AccountLocked_ResendBlocked", LoginMethod.Otp);
                return Json(new { success = false, result = $"حساب قفل است. {remaining} دقیقه دیگر تلاش کنید." });
            }

            // ۵. محدودیت سخت‌گیرانه Resend: حداکثر ۱ بار در ۱۲۰ ثانیه
            var lastResend = await _userService.GetLastRequestedOtpSent(phoneNumber);

            if (lastResend != null)
            {
                var waitSeconds = 120 - (int)(DateTime.UtcNow - lastResend.LoginAt).TotalSeconds;
                await LogFailedLoginAsync(user.Id, phoneNumber, ipAddress, userAgent, deviceInfo, "ResendTooSoon",
                    LoginMethod.Otp);
                return Json(new
                {
                    success = false,
                    result = $"لطفاً {waitSeconds} ثانیه صبر کنید و دوباره تلاش کنید."
                });
            }

            // ۶. محدودیت کلی ارسال OTP در ۱۰ دقیقه (۳ بار)
            var recentOtps = await _userService.CountRecentOtpCodesForPhoneNumber(phoneNumber);

            if (recentOtps >= 3)
            {
                // قفل موقت برای جلوگیری از اسپم
                user.IsLockedOut = true;
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(10);

                await _userService.Update(user);
                await _userService.SaveChanges();


                await LogFailedLoginAsync(user.Id, phoneNumber, ipAddress, userAgent, deviceInfo, "OtpSpamDetected",
                    LoginMethod.Otp);
                return Json(new
                {
                    success = false,
                    result = "تعداد درخواست بیش از حد مجاز است. ۱۰ دقیقه صبر کنید."
                });
            }

            // ۷. تمدید OTP قبلی یا ساخت جدید
            string otpCode;
            var currentOtp = await _roleService.GetOTPCodeByPhoneNumber(phoneNumber);

            if (currentOtp != null && currentOtp.ExpireDate > DateTime.UtcNow)
            {
                // فقط زمان انقضا رو تمدید کن (کد همون قبلیه)
                currentOtp.ExpireDate = DateTime.UtcNow.AddMinutes(2);
                _roleService.UpdateOTP(currentOtp);
                otpCode = currentOtp.HashedCode; // یا PlainCode اگه داری
            }
            else
            {
                // کد جدید بساز
                otpCode = await _roleService.GenerateOTPCode(user);
            }

            // ۸. ارسال پیامک
            var smsSuccess = await SMSSender.SendSmsAsync(phoneNumber, "otp", new { code = otpCode });

            if (!smsSuccess.IsSuccessful)
            {
                await LogFailedLoginAsync(user.Id, phoneNumber, ipAddress, userAgent, deviceInfo,
                    "SmsSendFailed_Resend", LoginMethod.Otp);
                return Json(new { success = false, result = "ارسال پیامک با مشکل مواجه شد." });
            }

            // ۹. لاگ موفق
            await LogSuccessfulOtpRequestAsync(user, ipAddress, userAgent, deviceInfo);

            return Json(new
            {
                success = true,
                result = "کد جدید با موفقیت ارسال شد."
            });
        }

        [Route("/Account/SetPassword/{phoneNumber}")]
        [Authorize]
        public async Task<IActionResult> SetPassword(string phoneNumber)
        {
            var user = await _userService.GetUserByPhoneNumber(phoneNumber);

            if (user == null)
            {
                NotificationSystem.ShowNotification(TempData, "کاربری با این مشخصات پیدا نشد", "",
                    ApplicationMessagesIcon.ErrorIcon);
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Login");
            }

            if (!string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                NotificationSystem.ShowNotification(TempData, "رمز عبور برای حساب کاربری شما قبلا تنظیم شده است", "",
                    ApplicationMessagesIcon.ErrorIcon);
                return Redirect("/");
            }

            ViewBag.PhoneNumber = phoneNumber;
            return View();
        }


        [HttpPost]
        [Authorize]
        [Route("/Account/SetPassword/{phoneNumber}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                NotificationSystem.ShowNotification(TempData, "خطا", "رمزهای عبور مطابقت ندارند.",
                    ApplicationMessagesIcon.ErrorIcon);
                return View();
            }

            if (newPassword.Length < 6)
            {
                NotificationSystem.ShowNotification(TempData, "خطا", "رمز عبور باید حداقل ۶ کاراکتر باشد.",
                    ApplicationMessagesIcon.ErrorIcon);
                return View();
            }

            var user = await _userService.GetUserByPhoneNumber(User.Identity.Name);
            if (user == null || !string.IsNullOrEmpty(user.PasswordHash))
            {
                return RedirectToAction("Index", "Home");
            }

            // هش کردن رمز (مثل همیشه)
            user.PasswordHash = PasswordSecurity.PasswordHasher.HashPassword(newPassword);
            user.LastPasswordChangedAt = DateTime.UtcNow;

            await _userService.Update(user);
            await _userService.SaveChanges();

            // لاگ مهم: کاربر اولین رمز عبور رو ست کرد
            var ip = GetClientIp();
            var ua = Request.Headers["User-Agent"].ToString();
            var device = ParseUserAgent(ua);

            var log = new UserLoginHistory
            {
                UserId = user.Id,
                PhoneNumber = user.PhoneNumber,
                LoginAt = DateTime.UtcNow,
                IpAddress = ip,
                UserAgent = ua,
                Browser = device.Browser,
                BrowserVersion = device.BrowserVersion,
                OperatingSystem = device.OS,
                DeviceType = device.DeviceType,
                IsMobile = device.IsMobile,
                Method = LoginMethod.Otp,
                IsSuccessful = true,
                FailureReason = "FirstPasswordSet"
            };
            await _userService.AddUserLoginHistory(log);

            NotificationSystem.ShowNotification(TempData,
                "رمز عبور با موفقیت تنظیم شد. از این پس می‌توانید با رمز هم وارد شوید.",
                "", ApplicationMessagesIcon.SuccessIcon);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CheckPasswordStrength([FromBody] PasswordCheckModel model)
        {
            var (score, message, strengthClass) = PasswordStrengthChecker.Evaluate(model.Password);
            return Json(new { score, message, strengthClass });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string phoneNumber)
        {
            var ipAddress = GetClientIp();
            var userAgent = Request.Headers["User-Agent"].ToString();
            var deviceInfo = ParseUserAgent(userAgent);

            // اعتبارسنجی شماره
            if (!PhoneNumberValidator.IsValid(phoneNumber.SanitizeString()))
            {
                await LogFailedLoginAsync(null, phoneNumber, ipAddress, userAgent, deviceInfo, "InvalidPhoneNumber", LoginMethod.ForgotPassword);
                NotificationSystem.ShowNotification(TempData, "شماره تلفن وارد شده معتبر نمی باشد", "شماره تلفن نامعتبر است.", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login");
            }

            var sanitizedPhone = phoneNumber.SanitizeString();
            var user = await _userService.GetUserByPhoneNumber(sanitizedPhone);

            if (user == null || user.IsDeleted)
            {
                await LogFailedLoginAsync(null, sanitizedPhone, ipAddress, userAgent, deviceInfo, "UserNotFound", LoginMethod.ForgotPassword);
                NotificationSystem.ShowNotification(TempData, "کاربری با این شماره همراه یافت نشد", "کاربری با این شماره یافت نشد.", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login");
            }

            if (!user.IsActive)
            {
                await LogFailedLoginAsync(user.Id, sanitizedPhone, ipAddress, userAgent, deviceInfo, "AccountDisabled", LoginMethod.ForgotPassword);
                NotificationSystem.ShowNotification(TempData, "حساب کاربری شما غیرفعال است", "حساب کاربری غیرفعال است.", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login");
            }

            if (user.IsLockedOut && user.LockoutEnd > DateTime.UtcNow)
            {
                await LogFailedLoginAsync(user.Id, sanitizedPhone, ipAddress, userAgent, deviceInfo, "AccountLocked", LoginMethod.ForgotPassword);
                NotificationSystem.ShowNotification(TempData, "به علت تکرار زیاد عملیات، حساب شما موقتاً قفل است ", "حساب شما موقتاً قفل است.", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login");
            }

            // محدودیت: ۳ درخواست در ۱۰ دقیقه از دیتابیس
            var recentAttempts = await _userService.CountRecentOtpCodesForResetPassword(sanitizedPhone);

            if (recentAttempts >= 3)
            {
                user.IsLockedOut = true;
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(10);
                await _userService.Update(user);

                await LogFailedLoginAsync(user.Id, sanitizedPhone, ipAddress, userAgent, deviceInfo, "ForgotPasswordRateLimit", LoginMethod.ForgotPassword);
                NotificationSystem.ShowNotification(TempData, "تعداد درخواست بیش از حد است. ۱۰ دقیقه صبر کنید", "تعداد درخواست بیش از حد است. ۱۰ دقیقه صبر کنید.", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login");
            }

            // تولید و ذخیره OTP در ردیس (۲ دقیقه اعتبار)
            var otp = new Random().Next(1000, 9999).ToString();
            var otpKey = $"forgot_otp:{sanitizedPhone}";
            await _redisService.SetStringAsync(otpKey, otp, TimeSpan.FromMinutes(2));

            // ارسال SMS
            var smsResult = await SMSSender.SendSmsAsync(sanitizedPhone, "otp", new { code = otp });

            if (!smsResult.IsSuccessful)
            {
                await LogFailedLoginAsync(user.Id, sanitizedPhone, ipAddress, userAgent, deviceInfo, "SmsSendFailed", LoginMethod.ForgotPassword);
                NotificationSystem.ShowNotification(TempData, "ارسال پیامک با مشکل مواجه شد", "ارسال پیامک با مشکل مواجه شد.", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login");
            }

            await LogSuccessfulOtpRequestAsync(user, ipAddress, userAgent, deviceInfo);
            TempData["ForgotPhone"] = sanitizedPhone;

            NotificationSystem.ShowNotification(TempData, $"کد تأیید به شماره {phoneNumber} ارسال شد", "کد تأیید به شماره شما ارسال شد.", ApplicationMessagesIcon.SuccessIcon);
            return RedirectToAction("ResetPassword");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> VerifyForgotPasswordOtp([FromBody] VerifyOtpDto dto)
        {
            var ipAddress = GetClientIp();
            var userAgent = Request.Headers["User-Agent"].ToString();
            var deviceInfo = ParseUserAgent(userAgent);

            var otpKey = $"forgot_otp:{dto.PhoneNumber}";
            var storedOtp = await _redisService.GetStringAsync(otpKey);

            if (storedOtp == dto.Otp)
            {
                await _redisService.RemoveAsync(otpKey); // یکبار مصرف
                return Json(new { success = true });
            }

            // تلاش ناموفق → لاگ + افزایش محدودیت
            await LogFailedLoginAsync(null, dto.PhoneNumber, ipAddress, userAgent, deviceInfo, "InvalidForgotOtp", LoginMethod.ForgotPassword);
            return Json(new { success = false, message = "کد اشتباه یا منقضی شده است" });
        }

        [HttpGet]
        [Route("Account/ResetPassword")]
        public IActionResult ResetPassword()
        {
            var phone = TempData["ForgotPhone"] as string;
            if (string.IsNullOrEmpty(phone))
            {
                NotificationSystem.ShowNotification(TempData, "خطا", "درخواست نامعتبر است.", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("Login");
            }

            ViewBag.PhoneNumber = phone;
            return View();
        }

        [HttpPost]
        [Route("Account/ResetPassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string phoneNumber, string newPassword, string confirmPassword)
        {
            var ipAddress = GetClientIp();
            var userAgent = Request.Headers["User-Agent"].ToString();
            var deviceInfo = ParseUserAgent(userAgent);

            // چک نهایی: OTP قبلاً استفاده شده یا منقضی شده؟
            var otpKey = $"forgot_otp:{phoneNumber}";
            if (!string.IsNullOrEmpty(await _redisService.GetStringAsync(otpKey)))
            {
                await LogFailedLoginAsync(null, phoneNumber, ipAddress, userAgent, deviceInfo, "DoubleResetAttempt", LoginMethod.ForgotPassword);
                return BadRequest("درخواست نامعتبر است.");
            }

            var user = await _userService.GetUserByPhoneNumber(phoneNumber);
            if (user == null) return NotFound();

            if (newPassword.Length < 8)
            {
                NotificationSystem.ShowNotification(TempData, "حداقل طول رمز عبور 8 کاراکتر می باشد", "رمزها مطابقت ندارند.", ApplicationMessagesIcon.ErrorIcon);
                return View();
            }

            if (newPassword != confirmPassword)
            {
                NotificationSystem.ShowNotification(TempData, "رمز عبور و تکرار آن مطابقت ندارند", "رمزها مطابقت ندارند.", ApplicationMessagesIcon.ErrorIcon);
                return View();
            }

            var (score, message, _) = PasswordStrengthChecker.Evaluate(newPassword);
            if (score < 70)
            {
                NotificationSystem.ShowNotification(TempData, $"رمز ضعیف | {message}", message, ApplicationMessagesIcon.ErrorIcon);
                return View();
            }

            user.PasswordHash = PasswordHasher.HashPassword(newPassword);
            user.LastPasswordChangedAt = DateTime.UtcNow;
            await _userService.Update(user);
            await _userService.SaveChanges();

            await LogSuccessfulPasswordReset(user,ipAddress,userAgent,deviceInfo);

            NotificationSystem.ShowNotification(TempData, "رمز عبور با موفقیت تغییر کرد", "رمز عبور با موفقیت تغییر کرد.", ApplicationMessagesIcon.SuccessIcon);
            return RedirectToAction("Login", "Account");
        }


        #region Helpers

        public class PasswordCheckModel
        {
            public string Password { get; set; }
        }

        public class VerifyOtpDto
        {
            public string PhoneNumber { get; set; }
            public string Otp { get; set; }
        }


        private async Task LogSuccessfulLoginAsync(Users user, string ip, string userAgent,
            (string Browser, string BrowserVersion, string OS, string DeviceType, bool IsMobile) device, LoginMethod method)
        {
            var history = new UserLoginHistory
            {
                UserId = user.Id,
                PhoneNumber = user.PhoneNumber,
                LoginAt = DateTime.UtcNow,
                IpAddress = ip,
                UserAgent = userAgent,
                Browser = device.Browser,
                BrowserVersion = device.BrowserVersion,
                OperatingSystem = device.OS,
                DeviceType = device.DeviceType,
                IsMobile = device.IsMobile,
                Method = method,
                IsSuccessful = true
            };

            var logHistoryResult = await _userService.AddUserLoginHistory(history);
            if (!logHistoryResult)
                Console.WriteLine($"Failed to log user login history to DB. userID : {user.Id} - username : {user.FullName} - dateTime : {DateTime.Now.ToShamsiWithTime()}");
        }

        private async Task LogFailedLoginAsync(long? userId, string phoneNumber, string ip, string userAgent,
            (string Browser, string BrowserVersion, string OS, string DeviceType, bool IsMobile) device,
            string reason, LoginMethod method)
        {
            var history = new UserLoginHistory
            {
                UserId = userId ?? 0,
                PhoneNumber = phoneNumber,
                LoginAt = DateTime.UtcNow,
                IpAddress = ip,
                UserAgent = userAgent,
                Browser = device.Browser,
                BrowserVersion = device.BrowserVersion,
                OperatingSystem = device.OS,
                DeviceType = device.DeviceType,
                IsMobile = device.IsMobile,
                Method = method,
                IsSuccessful = false,
                FailureReason = reason
            };

            var logHistoryResult = await _userService.AddUserLoginHistory(history);
            if (!logHistoryResult)
                Console.WriteLine($"Failed to log user login history to DB. userID : {userId} - username : {phoneNumber} - dateTime : {DateTime.Now.ToShamsiWithTime()}");
        }


        private async Task LogSuccessfulOtpRequestAsync(Users user, string ip, string userAgent,
            (string Browser, string BrowserVersion, string OS, string DeviceType, bool IsMobile) device)
        {
            var history = new UserLoginHistory
            {
                UserId = user.Id,
                PhoneNumber = user.PhoneNumber,
                LoginAt = DateTime.UtcNow,
                IpAddress = ip,
                UserAgent = userAgent,
                Browser = device.Browser,
                BrowserVersion = device.BrowserVersion,
                OperatingSystem = device.OS,
                DeviceType = device.DeviceType,
                IsMobile = device.IsMobile,
                Method = LoginMethod.Otp,
                IsSuccessful = true,
                FailureReason = "OtpRequested_Success"
            };

            var logHistoryResult = await _userService.AddUserLoginHistory(history);
            if (!logHistoryResult)
                Console.WriteLine($"Failed to log user otpRequest history to DB. userID : {user.Id} - username : {user.FullName} - dateTime : {DateTime.Now.ToShamsiWithTime()}");
        }


        private async Task LogSuccessfulPasswordReset(Users user, string ip, string userAgent,
            (string Browser, string BrowserVersion, string OS, string DeviceType, bool IsMobile) device)
        {
            var history = new UserLoginHistory
            {
                UserId = user.Id,
                PhoneNumber = user.PhoneNumber,
                LoginAt = DateTime.UtcNow,
                IpAddress = ip,
                UserAgent = userAgent,
                Browser = device.Browser,
                BrowserVersion = device.BrowserVersion,
                OperatingSystem = device.OS,
                DeviceType = device.DeviceType,
                IsMobile = device.IsMobile,
                Method = LoginMethod.Otp,
                IsSuccessful = true,
                FailureReason = "PasswordReset_Success"
            };

            var logHistoryResult = await _userService.AddUserLoginHistory(history);
            if (!logHistoryResult)
                Console.WriteLine($"Failed to log user Password Reset Success history to DB. userID : {user.Id} - username : {user.FullName} - dateTime : {DateTime.Now.ToShamsiWithTime()}");
        }



        // ─────────────────────────────────────────────────────────────────────
        // تشخیص مرورگر و دستگاه (با UAParser — NuGet: UAParser)
        // ─────────────────────────────────────────────────────────────────────

        private (string Browser, string BrowserVersion, string OS, string DeviceType, bool IsMobile) ParseUserAgent(string userAgent)
        {
            try
            {
                var uaParser = Parser.GetDefault();
                UAParser.ClientInfo c = uaParser.Parse(userAgent);

                return (
                    Browser: c.UA.Family,
                    BrowserVersion: $"{c.UA.Major}.{c.UA.Minor}",
                    OS: $"{c.OS.Family} {c.OS.Major}",
                    DeviceType: c.Device.Family == "Other" ? "Desktop" : c.Device.Family,
                    IsMobile: c.Device.IsSpider == false && (c.Device.Family.Contains("Mobile") || c.Device.Family.Contains("Tablet"))
                );
            }
            catch
            {
                return ("Unknown", "Unknown", "Unknown", "Unknown", false);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // گرفتن IP واقعی (پشت پراکسی هم کار می‌کنه)
        // ─────────────────────────────────────────────────────────────────────

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

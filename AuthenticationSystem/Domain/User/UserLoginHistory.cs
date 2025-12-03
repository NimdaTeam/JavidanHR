using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _0_Framework.EntityBase;

namespace AuthenticationSystem.Domain.User
{
    public class UserLoginHistory : EntityBase
    {
        public long UserId { get; set; }
        public string PhoneNumber { get; set; } = null!;

        public DateTime LoginAt { get; set; } = DateTime.UtcNow;

        public string IpAddress { get; set; } = null!;
        public string? Country { get; set; }
        public string? City { get; set; }

        public string? UserAgent { get; set; }
        public string? Browser { get; set; }
        public string? BrowserVersion { get; set; }
        public string? OperatingSystem { get; set; }
        public string? DeviceType { get; set; }        // Mobile, Desktop, Tablet
        public bool IsMobile { get; set; }

        public LoginMethod Method { get; set; }        // Password OR OTP
        public bool IsSuccessful { get; set; }
        public string? FailureReason { get; set; }     // WrongPassword, AccountLocked, etc.

        public virtual Users User { get; set; } = null!;
    }

    public enum LoginMethod
    {
        Password = 1,
        Otp = 2,
        ForgotPassword = 3
    }
}

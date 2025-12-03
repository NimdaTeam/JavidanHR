using System.ComponentModel.DataAnnotations;
using _0_Framework.EntityBase;
using AuthenticationSystem.Domain.Role;

namespace AuthenticationSystem.Domain.User
{
    public class Users:EntityBase
    {
        public string PhoneNumber { get; set; }
        public string? FullName { get; set; }
        public string? FatherName { get; set; }
        public string? NationalCode { get; set; }
        public bool IsActive { get; set; }

        // === Password ===
        public string? PasswordHash { get; set; } = null!;
        public DateTime? LastPasswordChangedAt { get; set; }

        // === Account Lockout ===
        public int AccessFailedCount { get; set; } = 0;
        public bool IsLockedOut { get; set; } = false;
        public DateTime? LockoutEnd { get; set; }

        // === Login & user manner ===
        public DateTime? LastLoginAt { get; set; }
        public string? LastLoginIp { get; set; }

        #region Relations
        public virtual ICollection<UserRoles> UserRoles { get; set; } = new HashSet<UserRoles>();
        public virtual ICollection<OtpCodes> OtpCodes { get; set; } = new HashSet<OtpCodes>();
        #endregion
    }
}

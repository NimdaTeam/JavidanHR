using System.ComponentModel.DataAnnotations.Schema;
using _0_Framework.EntityBase;

namespace AuthenticationSystem.Domain.User
{
    public class OtpCodes : EntityBase
    {
        [ForeignKey("Users")]
        public long UserId { get; set; }
        public string PhoneNumber { get; set; }
        public string HashedCode { get; set; }
        public DateTime ExpireDate { get; set; }
        public bool IsUsed { get; set; }
        public string? Purpose { get; set; } = null;

        public DateTime? UseDate { get; set; } = null;

        #region Relations
        public virtual Users? Users { get; set; }
        #endregion
    }
}

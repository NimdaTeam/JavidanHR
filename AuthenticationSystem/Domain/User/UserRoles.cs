using System.ComponentModel.DataAnnotations.Schema;
using _0_Framework.EntityBase;
using AuthenticationSystem.Domain.Role;

namespace AuthenticationSystem.Domain.User
{
    public class UserRoles:EntityBase
    {
        [ForeignKey("Users")]
        public long UserId { get; set; }

        [ForeignKey("Roles")]
        public long RoleId { get; set; }

        #region Relations

        
        public virtual Users? Users { get; set; }

        public virtual Roles? Roles { get; set; }

        #endregion
    }
}

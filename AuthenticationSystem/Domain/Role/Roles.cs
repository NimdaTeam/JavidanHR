using _0_Framework.EntityBase;
using AuthenticationSystem.Domain.RolePermission;
using AuthenticationSystem.Domain.User;

namespace AuthenticationSystem.Domain.Role
{
    public class Roles : EntityBase
    {
        public string Name { get; set; }


        #region Relations
        public virtual ICollection<Users> Users { get; set; } = new HashSet<Users>();
        public virtual ICollection<RolePermissions> RolePermissions { get; set; } = new HashSet<RolePermissions>();
        public virtual ICollection<UserRoles> UserRoles { get; set; } = new HashSet<UserRoles>();
        #endregion
    }
}

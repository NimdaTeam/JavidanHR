using System.ComponentModel.DataAnnotations.Schema;
using _0_Framework.EntityBase;
using AuthenticationSystem.Domain.Role;

namespace AuthenticationSystem.Domain.RolePermission
{
    public class RolePermissions:EntityBase
    {

        [ForeignKey("Roles")]
        public long RoleId { get; set; }

        public long Permission { get; set; }


        #region Relations
        public virtual Roles? Roles { get; set; }
        #endregion
    }
}

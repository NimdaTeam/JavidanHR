using AuthenticationSystem.Domain.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationSystem.Infrastructure
{
    public static class DbInitializer
    {
        public static void Seed(AuthenticationSystemContext context)
        {
            if (!context.Roles.Any(x => x.Name == "مدیریت")) {
                context.Roles.Add(new Domain.Role.Roles()
                {
                    Name = "مدیریت"                    
                });
            }

            if (!context.Users.Any(x=>x.PhoneNumber == "09397812171"))
            {
                context.Users.Add(new Users { 
                FullName = "محمد محسنی",
                PhoneNumber = "09397812171"
                });
            }
            context.SaveChanges();

            var role = context.Roles.FirstOrDefault(x=>x.Name == "مدیریت");

            if (role != null)
            {
                foreach (var project in SystemPermissions.SystemPermissions.AllSystemPermissions)
                {
                    foreach(var p in project.Permissions)
                    {
                        context.RolePermissions.Add(new Domain.RolePermission.RolePermissions()
                        {
                            RoleId = role.Id,
                            Permission = p.Permission
                        });
                    }
                }

                context.SaveChanges();
            }

            var user = context.Users.FirstOrDefault(x => x.PhoneNumber == "09397812171");
            if (user != null && role != null ) {
                if (!context.UserRoles.Any(x => x.UserId == user.Id && x.RoleId == role.Id))
                {
                    context.UserRoles.Add(new UserRoles()
                    {
                        UserId = user.Id,
                        RoleId = role.Id
                    });
                    context.SaveChanges();
                }
            }
        }
    }

}

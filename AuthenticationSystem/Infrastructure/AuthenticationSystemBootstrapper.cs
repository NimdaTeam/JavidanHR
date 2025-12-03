using _0_Framework.DTO;
using AuthenticationSystem.Services;
using AuthenticationSystem.Services.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuthenticationSystem.Infrastructure
{
    public static class AuthenticationSystemBootstrapper
    {
        public static void Configure(IServiceCollection services, string connectionString)
        {

            
            services.AddTransient<IRoleRepository, RoleService>();
            services.AddTransient<IUserRepository, UserService>();

            services.AddDbContext<AuthenticationSystemContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
        }

        public static List<PermissionGroupViewModel> SystemPermissionExposer()
        {
            var permissions = SystemPermissions.SystemPermissions.AllSystemPermissions;

            return permissions;
        }
    }
}

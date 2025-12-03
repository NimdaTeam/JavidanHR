using HrSystem.Application.Interfaces;
using HrSystem.Application.Services;
using HrSystem.Domain.Interfaces;
using HrSystem.Infrastructure.Context;
using HrSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HrSystem.Infrastructure.Bootstrapper
{
    public static class HrSystemBootstrapper
    {
        public static void Configure(IServiceCollection services, string connectionString)
        {

            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            
            services.AddDbContext<HrSystemContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
        }
    }
}

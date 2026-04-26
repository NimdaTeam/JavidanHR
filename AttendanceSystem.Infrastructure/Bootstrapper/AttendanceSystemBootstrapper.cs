using AttendanceSystem.Application.Interfaces;
using AttendanceSystem.Application.Services;
using AttendanceSystem.Domain.Interfaces;
using AttendanceSystem.Infrastructure.Context;
using AttendanceSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AttendanceSystem.Infrastructure.Bootstrapper
{
    public static class AttendanceSystemBootstrapper
    {
        public static void Configure(IServiceCollection services, string connectionString)
        {

            services.AddScoped<IManualAttendanceRequestRepository, ManualAttendanceRequestRepository>();
            services.AddScoped<IManualAttendanceRequestService, ManualAttendanceRequestService>();
            
            services.AddDbContext<AttendanceSystemContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
        }
    }
}

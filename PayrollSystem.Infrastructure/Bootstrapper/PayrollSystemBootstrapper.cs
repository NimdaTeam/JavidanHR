using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PayrollSystem.Application.Interfaces;
using PayrollSystem.Application.Services;
using PayrollSystem.Domain.Interfaces.CalculationLog;
using PayrollSystem.Domain.Interfaces.Contract;
using PayrollSystem.Domain.Interfaces.PayItem;
using PayrollSystem.Domain.Interfaces.PaySlip;
using PayrollSystem.Domain.Interfaces.Workshop;
using PayrollSystem.Infrastructure.MappingProfiles.Workshop;
using PayrollSystem.Infrastructure.Persistence.Context;
using PayrollSystem.Infrastructure.Repositories;
using PayrollSystem.Infrastructure.Repositories.CalculationLog;
using PayrollSystem.Infrastructure.Repositories.Contract;
using PayrollSystem.Infrastructure.Repositories.PayItem;
using PayrollSystem.Infrastructure.Repositories.PaySlip;
using PayrollSystem.Infrastructure.Repositories.Workshop;
using PayrollSystem.Application.DTOs;
using PayrollSystem.Domain.Entities.Workshop;

namespace PayrollSystem.Infrastructure.Bootstrapper
{
    public static class PayrollSystemBootstrapper
    {
        public static void Configure(IServiceCollection services, string connectionString)
        {
            #region Repositories
            services.AddScoped<ICalculationLogRepository, CalculationLogRepository>();

            services.AddScoped<IContractRepository, ContractRepository>();

            services.AddScoped<IPayItemRepository, PayItemRepository>();

            services.AddScoped<IPaySlipRepository, PaySlipRepository>();

            services.AddScoped<IWorkshopRepository, WorkshopRepository>();
            #endregion

            #region Services
            services.AddScoped<ICalculationLogService, CalculationLogService>();

            services.AddScoped<IContractService, ContractService>();

            services.AddScoped<IPayItemService, PayItemService>();

            services.AddScoped<IPaySlipService, PaySlipService>();

            services.AddScoped<IWorkshopService, WorkshopService>();
            #endregion

            //auto mapper profiles
            services.AddAutoMapper(cfg =>
            {
                cfg.CreateMap<Workshop, UpdateWorkshopDto>();
            });

            services.AddDbContext<PayrollSystemContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
        }
    }
}
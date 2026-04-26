using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using _0_Framework.Utilities.Security;
using HrSystem.Application.Interfaces;
using PayrollSystem.Application.DTOs;
using PayrollSystem.Application.Interfaces;
using PayrollSystem.Domain.Entities.Workshop;
using PayrollSystem.Domain.Interfaces.Workshop;

namespace PayrollSystem.Application.Services
{
    public class WorkshopService : IWorkshopService
    {
        private readonly IWorkshopRepository _workshopRepo;
        private readonly IEmployeeService _employeeService;

        public WorkshopService(IWorkshopRepository workshopRepo, IEmployeeService employeeService)
        {
            _workshopRepo = workshopRepo;
            _employeeService = employeeService;
        }

        public async Task<WorkshopDto?> CreateWorkshopAsync(CreateWorkshopDto dto, CancellationToken cancellationToken = default)
        {
            // Validate unique code
            var isUnique = await _workshopRepo.IsCodeUniqueAsync(dto.Code, null, cancellationToken);
            if (!isUnique)
                throw new InvalidOperationException($"Workshop code '{dto.Code}' already exists.");

            var workshop = new Workshop(dto.Code, dto.Name, dto.EmployerName, dto.Address, dto.EmployeeInsuranceRate, dto.EmployerInsuranceRate, dto.UnEmploymentInsuranceRate, dto.PeymanRow, dto.AccountNumber);

            var result = await _workshopRepo.ExecuteInTransactionAsync(async () =>
             {
                 await _workshopRepo.AddAsync(workshop, cancellationToken);
                 return await _workshopRepo.SaveChangesAsync(cancellationToken);
             }, cancellationToken);

            return result ? MapToDto(workshop) : null;
        }

        public async Task<WorkshopDto?> UpdateWorkshopAsync(UpdateWorkshopDto dto, CancellationToken cancellationToken = default)
        {
            var workshop = await _workshopRepo.GetAsync(dto.Id, cancellationToken);
            if (workshop == null) return null;

            // If code changed, validate uniqueness
            if (workshop.Code != dto.Code)
            {
                var isUnique = await _workshopRepo.IsCodeUniqueAsync(dto.Code, dto.Id, cancellationToken);
                if (!isUnique)
                    throw new InvalidOperationException($"Workshop code '{dto.Code}' already exists.");
                workshop.UpdateCode(dto.Code.SanitizeString());
            }

            if (workshop.Name != dto.Name)
            {
                var isNameExist = await _workshopRepo.ExistsAsync(x => x.Name == dto.Name.SanitizeString() && x.Id != dto.Id, cancellationToken);

                if (isNameExist)
                    throw new InvalidOperationException($"Workshop name '{dto.Name}' already exists.");

                workshop.UpdateName(dto.Name.SanitizeString());
            }

            workshop.UpdateEmployerName(dto.EmployerName.SanitizeString());
            workshop.UpdateAddress(dto.Address.SanitizeString());
            workshop.UpdateInsuranceRate(dto.EmployeeInsuranceRate, dto.EmployerInsuranceRate, dto.UnEmploymentInsuranceRate);
            workshop.UpdatePeymanRow(dto.PeymanRow);
            workshop.UpdateAccountNumber(dto.AccountNumber);


            var result = await _workshopRepo.ExecuteInTransactionAsync(async () =>
            {
                await _workshopRepo.UpdateAsync(workshop, cancellationToken);
                return await _workshopRepo.SaveChangesAsync(cancellationToken);
            }, cancellationToken);

            return result ? MapToDto(workshop) : null;
        }

        public async Task<bool> DeleteWorkshopAsync(long id, CancellationToken cancellationToken = default)
        {
            var workshop = await GetWorkshopByIdAsync(id, cancellationToken);
            if (workshop is null)
                return false;

            workshop.SoftDelete();

            var result = await _workshopRepo.ExecuteInTransactionAsync(async () =>
             {
                 await _workshopRepo.UpdateAsync(workshop, cancellationToken);
                 return await _workshopRepo.SaveChangesAsync(cancellationToken);
             }, cancellationToken);

            return result;
        }

        public async Task<Workshop?> GetWorkshopByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            var workshop = await _workshopRepo.GetAsync(id, cancellationToken);
            return workshop;
        }

        public async Task<WorkshopDto?> GetWorkshopByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            var workshop = await _workshopRepo.GetByCodeAsync(code, cancellationToken);
            return workshop != null ? MapToDto(workshop) : null;
        }

        public async Task<List<WorkshopDto>> GetAllWorkshopsAsync(CancellationToken cancellationToken = default)
        {
            var workshops = await _workshopRepo.GetAllAsync(cancellationToken);

            var dto = workshops.Select(MapToDto).ToList();

            var tasks = dto.Select(async x =>
            {
                x.EmployeesCount = await GetEmployeeCount(x.Id);
            });

            await Task.WhenAll(tasks);

            return dto ;
        }

        public async Task<bool> IsExistWorkshopToAdd(string code, string name)
        {
            return await _workshopRepo.ExistsAsync(x => x.Code == code.SanitizeString() && x.Name == name.SanitizeString());
        }

        public async Task<bool> IsExistWorkshopToUpdate(long id, string code, string name)
        {
            return await _workshopRepo.ExistsAsync(x =>
                x.Code == code.SanitizeString() && x.Name == name.SanitizeString() && x.Id != id);
        }

        public async Task<int> GetWorkshopEmployeeCountAsync(long id)
        {
            return await GetEmployeeCount(id);
        }

        private async Task<int> GetEmployeeCount(long id)
        {
            return await _employeeService.GetWorkshopEmployeeCount(id);
        }

        private static WorkshopDto MapToDto(Workshop w) => new()
        {
            Id = w.Id,
            Code = w.Code,
            PeymanRow = w.PeymanRow,
            Name = w.Name,
            EmployerName = w.EmployerName,
            Address = w.Address,
            EmployeeInsuranceRate = w.EmployeeInsuranceRate,
            EmployerInsuranceRate = w.EmployerInsuranceRate,
            UnEmploymentInsuranceRate = w.UnEmploymentInsuranceRate,
            AccountNumber = w.AccountNumber
        };
    }
}
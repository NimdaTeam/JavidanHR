// IWorkshopAppService.cs
using PayrollSystem.Application.DTOs;
using PayrollSystem.Domain.Entities.Workshop;

namespace PayrollSystem.Application.Interfaces
{
    /// <summary>
    /// Application service for workshop management.
    /// </summary>
    public interface IWorkshopService
    {
        Task<WorkshopDto?> CreateWorkshopAsync(CreateWorkshopDto dto, CancellationToken cancellationToken = default);
        Task<WorkshopDto?> UpdateWorkshopAsync(UpdateWorkshopDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteWorkshopAsync(long id, CancellationToken cancellationToken = default);
        Task<Workshop?> GetWorkshopByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<WorkshopDto?> GetWorkshopByCodeAsync(string code, CancellationToken cancellationToken = default);
        Task<List<WorkshopDto>> GetAllWorkshopsAsync(CancellationToken cancellationToken = default);

        Task<bool> IsExistWorkshopToAdd(string code, string name);
        Task<bool> IsExistWorkshopToUpdate(long id ,string code, string name);

        Task<int> GetWorkshopEmployeeCountAsync(long id);
    }
}
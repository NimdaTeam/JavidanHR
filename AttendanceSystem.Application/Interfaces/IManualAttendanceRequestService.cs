using _0_Framework.DTO;
using AttendanceSystem.Domain.Entities;
using System.Linq.Expressions;

namespace AttendanceSystem.Application.Interfaces
{
    public interface IManualAttendanceRequestService
    {
        // Get
        Task<ManualAttendanceRequest?> GetById(long id);
        Task<ManualAttendanceRequest?> GetByIdAsNoTracking(long id);
        Task<List<ManualAttendanceRequest>> GetAll();
        Task<List<ManualAttendanceRequest>> GetByCondition(Expression<Func<ManualAttendanceRequest, bool>> expression);
        Task<ManualAttendanceRequest?> SingleOrDefaultByCondition(Expression<Func<ManualAttendanceRequest, bool>> expression);
        Task<PaginatedList<ManualAttendanceRequest>> GetAllPaginated(int pageNumber, int pageSize);
        Task<List<ManualAttendanceRequest>> GetByUserId(long userId);
        Task<List<ManualAttendanceRequest>> GetPendingRequests();
        Task<List<ManualAttendanceRequest>> GetByDateRange(DateTime startDate, DateTime endDate);

        // Check
        Task<bool> IsExist(long id);
        Task<bool> IsExist(long userId, DateTime workDate, List<string> times);
        Task<bool> IsPending(long id);

        // Create
        Task<bool> Add(ManualAttendanceRequest entity);
        Task<ManualAttendanceRequest?> AddAndReturn(ManualAttendanceRequest entity);

        // Update
        Task<bool> Update(ManualAttendanceRequest entity);
        Task<bool> Approve(long requestId, long approvedByUserId);
        Task<bool> Reject(long requestId, long rejectedByUserId, string reason);

        // Delete
        Task<bool> Delete(long id);
        Task<bool> Delete(ManualAttendanceRequest entity);
    }
}
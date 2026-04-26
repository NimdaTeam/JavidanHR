using _0_Framework.DTO;
using AttendanceSystem.Application.Interfaces;
using AttendanceSystem.Domain.Entities;
using AttendanceSystem.Domain.Interfaces;
using System.Linq.Expressions;

namespace AttendanceSystem.Application.Services
{
    public class ManualAttendanceRequestService : IManualAttendanceRequestService
    {
        private readonly IManualAttendanceRequestRepository _repository;

        public ManualAttendanceRequestService(IManualAttendanceRequestRepository repository)
        {
            _repository = repository;
        }

        #region Get Methods

        public async Task<ManualAttendanceRequest?> GetById(long id)
        {
            if (id <= 0)
                return null;

            return await _repository.GetAsync(id);
        }

        public async Task<ManualAttendanceRequest?> GetByIdAsNoTracking(long id)
        {
            if (id <= 0)
                return null;

            return await _repository.GetAsNoTrackingAsync(id);
        }

        public async Task<List<ManualAttendanceRequest>> GetAll()
        {
            return await _repository.GetAllWithIncludesAsync();
        }

        public async Task<List<ManualAttendanceRequest>> GetByCondition(
            Expression<Func<ManualAttendanceRequest, bool>> expression)
        {
            return await _repository.GetAllByConditionAsync(expression);
        }

        public async Task<ManualAttendanceRequest?> SingleOrDefaultByCondition(
            Expression<Func<ManualAttendanceRequest, bool>> expression)
        {
            return await _repository.SingleOrDefaultByConditionAsync(expression);
        }

        public async Task<PaginatedList<ManualAttendanceRequest>> GetAllPaginated(
            int pageNumber,
            int pageSize)
        {
            if (pageNumber <= 0)
                pageNumber = 1;

            if (pageSize is <= 0 or > 100)
                pageSize = 10;

            return await _repository.GetAllPaginatedAsync(
                pageNumber,
                pageSize,
                orderBy: x => x.OrderByDescending(r => r.CreatedAt));
        }

        public async Task<List<ManualAttendanceRequest>> GetByUserId(long userId)
        {
            return await _repository.GetAllByConditionAsync(x => x.UserId == userId);
        }

        public async Task<List<ManualAttendanceRequest>> GetPendingRequests()
        {
            return await _repository.GetAllByConditionAsync(x => x.Status == RequestStatus.Pending);
        }

        public async Task<List<ManualAttendanceRequest>> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            return await _repository.GetAllByConditionAsync(x =>
                x.AttendanceDate >= startDate &&
                x.AttendanceDate <= endDate);
        }

        #endregion

        #region Check Methods

        public async Task<bool> IsExist(long id)
        {
            return await _repository.ExistsAsync(x => x.Id == id);
        }

        public async Task<bool> IsExist(long userId, DateTime workDate, List<string> times)
        {
            return await _repository.ExistsAsync(x =>
                x.UserId == userId &&
                x.AttendanceDate == workDate &&
                x.AttendanceTimes == times);
        }

        public async Task<bool> IsPending(long id)
        {
            var request = await GetById(id);
            return request?.Status == RequestStatus.Pending;
        }

        #endregion

        #region Create Methods

        public async Task<bool> Add(ManualAttendanceRequest entity)
        {
            // ✅ اعتبارسنجی
            if (!ValidateEntity(entity))
                return false;

            // ✅ بررسی تکراری نبودن
            var exists = await IsExist(entity.UserId, entity.AttendanceDate, entity.AttendanceTimes);
            if (exists)
                return false;

            return await _repository.ExecuteInTransactionAsync(async () =>
            {
                await _repository.AddAsync(entity);
                return await _repository.SaveChangesAsync();
            });
        }

        public async Task<ManualAttendanceRequest?> AddAndReturn(ManualAttendanceRequest entity)
        {
            if (!ValidateEntity(entity))
                return null;

            var exists = await IsExist(entity.UserId, entity.AttendanceDate, entity.AttendanceTimes);
            if (exists)
                return null;


            await _repository.AddAsync(entity);
            var result = await _repository.SaveChangesAsync();
            return result ? entity : null;
        }

        #endregion

        #region Update Methods

        public async Task<bool> Update(ManualAttendanceRequest entity)
        {
            // ✅ اعتبارسنجی
            if (!ValidateEntity(entity))
                return false;

            // ✅ بررسی وجود رکورد
            var existing = await GetById(entity.Id);
            if (existing == null)
                return false;

            // ✅ بررسی وضعیت (فقط درخواست‌های رد شده قابل ویرایش هستند)
            if (existing.Status != RequestStatus.Rejected)
                return false;

            return await _repository.ExecuteInTransactionAsync(async () =>
            {
                await _repository.UpdateAsync(entity);
                return await _repository.SaveChangesAsync();
            });
        }

        public async Task<bool> Approve(long requestId, long approvedByUserId)
        {
            var request = await GetById(requestId);
            if (request == null || request.Status != RequestStatus.Pending)
                return false;

            request.Status = RequestStatus.Approved;
            request.ApprovedByUserId = approvedByUserId;
            request.ApprovedAt = DateTime.Now;

            return await _repository.ExecuteInTransactionAsync(async () =>
            {
                await _repository.UpdateAsync(request);
                return await _repository.SaveChangesAsync();
            });
        }

        public async Task<bool> Reject(long requestId, long rejectedByUserId, string reason)
        {
            var request = await GetById(requestId);
            if (request is not { Status: RequestStatus.Pending })
                return false;

            request.Status = RequestStatus.Rejected;
            request.RejectedByUserId = rejectedByUserId;
            request.RejectedAt = DateTime.Now;
            request.RejectionReason = reason;

            return await _repository.ExecuteInTransactionAsync(async () =>
            {
                await _repository.UpdateAsync(request);
                return await _repository.SaveChangesAsync();
            });
        }

        #endregion

        #region Delete Methods

        public async Task<bool> Delete(long id)
        {
            // ✅ بررسی وجود رکورد
            var request = await GetById(id);
            if (request == null)
                return false;

            // ✅ فقط درخواست‌های رد شده یا در انتظار را می‌توان حذف کرد
            if (request.Status == RequestStatus.Approved)
                return false;

            return await _repository.ExecuteInTransactionAsync(async () =>
            {
                request.SoftDelete();

                await _repository.UpdateAsync(request);
                return await _repository.SaveChangesAsync();
            });
        }

        public async Task<bool> Delete(ManualAttendanceRequest entity)
        {
            return await Delete(entity.Id);
        }

        #endregion

        #region Private Methods

        private static bool ValidateEntity(ManualAttendanceRequest entity)
        {
            if (entity.UserId <= 0)
                return false;

            if (entity.AttendanceDate == default)
                return false;

            if (!entity.AttendanceTimes.Any())
                return false;

            return true;
        }

        #endregion
    }
}
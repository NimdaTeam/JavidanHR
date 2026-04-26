using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _0_Framework.GenericRepositoy.Interface;
using AttendanceSystem.Domain.Entities;

namespace AttendanceSystem.Domain.Interfaces
{
    public interface IManualAttendanceRequestRepository : IRepository<long,ManualAttendanceRequest>
    {
        Task<bool> ApproveRequest(long requestId,long userId, string username);
        Task<bool> RejectRequest(long requestId,long userId, string username,string? rejectDescription);
    }
}

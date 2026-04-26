using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _0_Framework.GenericRepositoy.Service;
using AttendanceSystem.Domain.Entities;
using AttendanceSystem.Domain.Interfaces;
using AttendanceSystem.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Infrastructure.Repositories
{
    public class ManualAttendanceRequestRepository:RepositoryService<long,ManualAttendanceRequest>,IManualAttendanceRequestRepository
    {
        private readonly AttendanceSystemContext _context;

        public ManualAttendanceRequestRepository(AttendanceSystemContext context) : base(context)
        {
            _context = context;
        }

        public Task<bool> ApproveRequest(long requestId, long userId, string username)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RejectRequest(long requestId, long userId, string username, string? rejectDescription)
        {
            throw new NotImplementedException();
        }
    }
}

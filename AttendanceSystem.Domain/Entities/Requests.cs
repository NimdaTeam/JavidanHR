using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _0_Framework.EntityBase;

namespace AttendanceSystem.Domain.Entities
{
    public abstract class Request : EntityBase
    {
        public long UserId { get; set; }
        [MaxLength(100)]
        public string Username { get; set; }
        public RequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        // Approval info
        public long? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }

        // Rejection info
        public bool IsRejected { get; set; }
        public string? RejectionReason { get; set; }
        public long? RejectedByUserId { get; set; }
        public DateTime? RejectedAt { get; set; }
    }

    public enum RequestStatus
    {
        Pending,
        Approved,
        Rejected
    }
}

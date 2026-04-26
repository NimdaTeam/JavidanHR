using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceSystem.Domain.Entities
{
    public class LeaveRequest : Request
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public LeaveType LeaveType { get; set; }
        public int TotalDays { get; set; }
        [MaxLength(10000)]
        public string? Reason { get; set; }
    }

    public enum LeaveType
    {
        Annual,
        Sick,
        Unpaid,
        Emergency,
        Maternity,
        Paternity
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceSystem.Domain.Entities
{
    public class ManualAttendanceRequest : Request
    {
        public DateTime AttendanceDate { get; set; }
        public List<string> AttendanceTimes { get; set; } =  [];
        [MaxLength(10000)]
        public string? Reason { get; set; }
    }
}

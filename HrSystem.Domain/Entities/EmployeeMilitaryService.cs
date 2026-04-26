using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _0_Framework.EntityBase;

namespace HrSystem.Domain.Entities
{
    public class EmployeeMilitaryService:EntityBase
    {
        public long EmployeeId { get; set; }
        public MilitaryStatus Status { get; set; } // Ended, Exempt, None (برای خانم‌ها)
        public DateTime? ServiceEndDate { get; set; }
        public string? ExemptionReason { get; set; }
        public Employee Employee { get; set; } = null!;
    }

    public enum MilitaryStatus { None, Ended, Exempt }
}

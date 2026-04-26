using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _0_Framework.EntityBase;

namespace HrSystem.Domain.Entities
{
    public class EmployeeMaritalInfo:EntityBase
    {
        public long EmployeeId { get; set; }
        public MaritalStatus MaritalStatus { get; set; }
        public DateTime? MarriageDate { get; set; }
        public Employee Employee { get; set; } = null!;
    }
}

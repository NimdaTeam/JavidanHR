using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _0_Framework.EntityBase;

namespace HrSystem.Domain.Entities
{
    public class EmployeeLoan:EntityBase
    {
        public long EmployeeId { get; set; }
        public decimal Amount { get; set; }
        public string BorrowerName { get; set; } = null!;     // ممکنه به نام دیگری باشه
        public string? Guarantors { get; set; }
        public DateTime? SettlementDate { get; set; }
        public Employee Employee { get; set; } = null!;
    }
}

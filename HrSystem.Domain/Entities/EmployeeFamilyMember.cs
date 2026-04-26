using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _0_Framework.EntityBase;

namespace HrSystem.Domain.Entities
{
    public class EmployeeFamilyMember:EntityBase
    {
        public long EmployeeId { get; set; }
        public string FullName { get; set; } = null!;
        public string? FathersName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Relation { get; set; } = null!; // همسر، فرزند، پدر، مادر و ...
        public string? AddressOrWorkplace { get; set; }
        public Employee Employee { get; set; } = null!;
    }
}

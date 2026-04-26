using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _0_Framework.EntityBase;

namespace HrSystem.Domain.Entities
{
    public class EmployeeTraining:EntityBase
    {
        public long EmployeeId { get; set; }
        public string CourseName { get; set; } = null!;
        public string Institute { get; set; } = null!;
        public int? Hours { get; set; }
        public string? CertificateNumberAndDate { get; set; }
        public string? Notes { get; set; }
        public Employee Employee { get; set; } = null!;
    }
}

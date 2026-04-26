using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _0_Framework.EntityBase;

namespace HrSystem.Domain.Entities
{
    public class EmployeeWorkExperience:EntityBase
    {
        public long EmployeeId { get; set; }
        public WorkExperienceType Type { get; set; } // JavidanInternal, External, SelfEmployed
        public string Organization { get; set; } = null!;
        public string Position { get; set; } = null!;
        public string? DirectManager { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool HasInsurance { get; set; }
        public string? TerminationReason { get; set; }
        public string? Notes { get; set; }
        public Employee Employee { get; set; } = null!;
    }

    public enum WorkExperienceType { JavidanInternal, External, SelfEmployed }
}

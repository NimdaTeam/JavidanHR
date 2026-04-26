using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _0_Framework.EntityBase;

namespace HrSystem.Domain.Entities
{
    public class EmployeeEducation:EntityBase
    {
        public long EmployeeId { get; set; }
        public EducationLevel Level { get; set; }
        public string FieldOfStudy { get; set; } = null!;
        public int? StartYear { get; set; }
        public int? EndYear { get; set; }
        public string? InstituteName { get; set; }
        public string? InstituteCity { get; set; }
        public string? InstituteAddress { get; set; }
        public Employee Employee { get; set; } = null!;
    }
}

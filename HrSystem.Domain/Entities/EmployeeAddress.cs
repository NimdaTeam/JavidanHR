using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _0_Framework.EntityBase;

namespace HrSystem.Domain.Entities
{
    public class EmployeeAddress:EntityBase
    {
        public long EmployeeId { get; set; }
        public HousingStatus HousingStatus { get; set; } // Personal, Rental
        public string? PersonalAddress { get; set; }
        public string? RentalAddress { get; set; }
        public Employee Employee { get; set; } = null!;
    }

    public enum HousingStatus { Personal, Rental }
}

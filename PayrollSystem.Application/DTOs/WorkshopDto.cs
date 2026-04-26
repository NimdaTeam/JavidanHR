// WorkshopDto.cs
namespace PayrollSystem.Application.DTOs
{
    /// <summary>
    /// Data transfer object for Workshop.
    /// </summary>
    public class WorkshopDto
    {
        public long Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? PeymanRow { get; set; }
        public string Name { get; set; } = string.Empty;
        public string EmployerName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public long EmployeeInsuranceRate { get; set; }
        public long EmployerInsuranceRate { get; set; }
        public long UnEmploymentInsuranceRate { get; set; }

        public string? AccountNumber { get; set; }

        public long? EmployeesCount { get; set; }
    }

    /// <summary>
    /// Command to create a new workshop.
    /// </summary>
    public class CreateWorkshopDto
    {
        public string Code { get; set; } = string.Empty;
        public string? PeymanRow { get; set; }
        public string Name { get; set; } = string.Empty;
        public string EmployerName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public long EmployeeInsuranceRate { get; set; }
        public long EmployerInsuranceRate { get; set; }
        public long UnEmploymentInsuranceRate { get; set; }

        public string? AccountNumber { get; set; }
    }

    /// <summary>
    /// Command to update an existing workshop.
    /// </summary>
    public class UpdateWorkshopDto
    {
        public long Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? PeymanRow { get; set; }
        public string Name { get; set; } = string.Empty;
        public string EmployerName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public long EmployeeInsuranceRate { get; set; }
        public long EmployerInsuranceRate { get; set; }
        public long UnEmploymentInsuranceRate { get; set; }

        public string? AccountNumber { get; set; }
    }
}
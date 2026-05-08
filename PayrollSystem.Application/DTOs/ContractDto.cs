// ContractDto.cs
using PayrollSystem.Domain.Entities.Contract;
using System.ComponentModel.DataAnnotations;

namespace PayrollSystem.Application.DTOs
{
    /// <summary>
    /// Data transfer object for Contract.
    /// </summary>
    public class ContractDto
    {
        public long Id { get; set; }
        public long EmployeeId { get; set; }
        public string? EmployeeName { get; set; } 
        public long WorkshopId { get; set; }
        public string? WorkshopName { get; set; }
        public DateTime ValidFromDate { get; set; }
        public DateTime? ValidToDate { get; set; }
        public bool IsActive { get; set; }
        public ContractStatus? Status { get; set; }
        public List<ContractPayItemDto> PayItems { get; set; } = [];
    }

    /// <summary>
    /// DTO for a pay item assigned to a contract (with a fixed value for UserInput items).
    /// </summary>
    public class ContractPayItemDto
    {
        public long PayItemId { get; set; }
        public string PayItemName { get; set; } = string.Empty;
        public decimal? Value { get; set; }
    }

    /// <summary>
    /// Command to create a new contract.
    /// </summary>
    public class CreateContractDto
    {
        public long EmployeeId { get; set; }
        public long WorkshopId { get; set; }
        public DateTime ValidFromDate { get; set; }
        public DateTime? ValidToDate { get; set; }
        public List<AssignPayItemToContractDto> PayItems { get; set; } = [];
    }

    /// <summary>
    /// Command to assign a pay item to a contract.
    /// </summary>
    public class AssignPayItemToContractDto
    {
        public long ContractId { get; set; }
        public long PayItemId { get; set; }
        public decimal? Value { get; set; } // optional for UserInput items
    }


    /// <summary>
    /// DTO for updating contract
    /// </summary>
    public class UpdateContractDto
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "تاریخ شروع الزامی است")]
        public DateTime ValidFromDate { get; set; }

        public DateTime? ValidToDate { get; set; }
    }
}



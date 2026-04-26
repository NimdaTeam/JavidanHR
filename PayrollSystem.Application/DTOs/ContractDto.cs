// ContractDto.cs
namespace PayrollSystem.Application.DTOs
{
    /// <summary>
    /// Data transfer object for Contract.
    /// </summary>
    public class ContractDto
    {
        public long Id { get; set; }
        public long EmployeeId { get; set; }
        public long WorkshopId { get; set; }
        public DateTime ValidFromDate { get; set; }
        public DateTime? ValidToDate { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<ContractPayItemDto> PayItems { get; set; } = new();
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
}
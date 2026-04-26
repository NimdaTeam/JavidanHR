// PaySlipDto.cs (already partially given, now complete)
namespace PayrollSystem.Application.DTOs
{
    /// <summary>
    /// Data transfer object for PaySlip.
    /// </summary>
    public class PaySlipDto
    {
        public long Id { get; set; }
        public long EmployeeId { get; set; }
        public long ContractId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetPay { get; set; }
        public List<PaySlipItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// DTO for a single line in a pay slip.
    /// </summary>
    public class PaySlipItemDto
    {
        public long PayItemId { get; set; }
        public string PayItemName { get; set; } = string.Empty;
        public decimal CalculatedValue { get; set; }
        public decimal? ManualOverrideValue { get; set; }
        public decimal FinalValue { get; set; }
    }

    /// <summary>
    /// Command to generate a new pay slip.
    /// </summary>
    public class GeneratePaySlipCommand
    {
        public long EmployeeId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public DateTime IssueDate { get; set; }
    }

    /// <summary>
    /// Command to override a pay slip item's value.
    /// </summary>
    public class OverridePaySlipItemCommand
    {
        public long PaySlipId { get; set; }
        public long PayItemId { get; set; }
        public decimal NewValue { get; set; }
    }
}
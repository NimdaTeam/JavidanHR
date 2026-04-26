// CalculationLogDto.cs
namespace PayrollSystem.Application.DTOs
{
    /// <summary>
    /// DTO for calculation log entry.
    /// </summary>
    public class CalculationLogDto
    {
        public long Id { get; set; }
        public long PaySlipId { get; set; }
        public long PayItemId { get; set; }
        public string InputValuesJson { get; set; } = string.Empty;
        public string FormulaUsed { get; set; } = string.Empty;
        public decimal ResultValue { get; set; }
        public DateTime CalculatedAt { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
using _0_Framework.EntityBase;
using Ardalis.GuardClauses;
using PayrollSystem.Domain.Common;

namespace PayrollSystem.Domain.Entities.Contract.ContractPayItem;

/// <summary>
/// Represents a pay item associated with a contract
/// Supports both database pay items (via PayItemId) and system pay items (via SystemCode)
/// </summary>
public class ContractPayItem : EntityBase
{
    // Foreign key to Contract
    public long ContractId { get; private set; }

    // Foreign key to PayItem (nullable for system items)
    public long? PayItemId { get; private set; }

    // System code for system-defined pay items (nullable for database items)
    [MaxLength(100)]
    public string? SystemCode { get; private set; }

    // Value for user input items
    public decimal? Value { get; private set; }

    // Constructor for database pay items
    internal ContractPayItem(long contractId, long payItemId, decimal? value = null)
    {
        Guard.Against.NegativeOrZero(contractId, nameof(contractId));
        Guard.Against.NegativeOrZero(payItemId, nameof(payItemId));

        ContractId = contractId;
        PayItemId = payItemId;
        SystemCode = null;
        Value = value;
    }

    // Constructor for system pay items
    internal ContractPayItem(long contractId, string systemCode, decimal? value = null)
    {
        Guard.Against.NegativeOrZero(contractId, nameof(contractId));
        Guard.Against.NullOrWhiteSpace(systemCode, nameof(systemCode));

        if (!IsValidSystemCode(systemCode))
            throw new ArgumentException("کد سیستمی وارد شده معتبر نیست");

        ContractId = contractId;
        PayItemId = null;
        SystemCode = systemCode;
        Value = value;
    }

    /// <summary>
    /// Updates the value for user-input pay items
    /// </summary>
    public void UpdateValue(decimal? newValue)
    {
        Value = newValue;
    }

    private bool IsValidSystemCode(string systemCode)
    {
        return PayItemConstants.IsCodeExist(systemCode);
    }

    /// <summary>
    /// Indicates whether this pay item is a system-defined item
    /// </summary>
    public bool IsSystemItem => !string.IsNullOrWhiteSpace(SystemCode) && IsValidSystemCode(SystemCode);

    /// <summary>
    /// Indicates whether this pay item is a database item
    /// </summary>
    public bool IsDatabaseItem => PayItemId.HasValue;

    /// <summary>
    /// Gets the unique identifier for this pay item (either PayItemId or SystemCode)
    /// </summary>
    public string GetIdentifier() => IsSystemItem ? SystemCode! : PayItemId!.ToString();
}

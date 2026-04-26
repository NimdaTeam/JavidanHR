using _0_Framework.EntityBase;
using Ardalis.GuardClauses;
using System.Net;
using _0_Framework.Utilities.Security;

namespace PayrollSystem.Domain.Entities.Workshop;

public class Workshop : EntityBase
{
    public string Code { get; private set; }
    public string? PeymanRow { get; private set; }
    public string Name { get; private set; }
    public string EmployerName { get; private set; }
    public string Address { get; private set; }

    //insurance
    public long EmployeeInsuranceRate { get; private set; }
    public long EmployerInsuranceRate { get; private set; }
    public long UnEmploymentInsuranceRate { get; private set; }

    public string? AccountNumber { get; private set; }


    private Workshop() { }

    public Workshop(
        string code,
        string name,
        string employerName,
        string address,
        long employeeInsuranceRate,
        long employerInsuranceRate,
        long unemploymentInsuranceRate,
        string? peymanRow = null,
        string? accountNumber = null)
    {
        UpdateCode(code);
        UpdateName(name);
        UpdateEmployerName(employerName);
        UpdateAddress(address);
        UpdateInsuranceRate(employeeInsuranceRate, employerInsuranceRate, unemploymentInsuranceRate);

        PeymanRow = peymanRow?.SanitizeString();
        AccountNumber = accountNumber?.SanitizeString();
    }

    // ---------------------- Guards & Setters ----------------------

    public void UpdateCode(string code)
    {
        Guard.Against.NullOrWhiteSpace(code);
        Guard.Against.OutOfRange(code.Length, nameof(code), 1, 10);
        Code = code.SanitizeString();
    }

    public void UpdateName(string name)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Name = name.SanitizeString();
    }

    public void UpdateEmployerName(string employer)
    {
        Guard.Against.NullOrWhiteSpace(employer);
        EmployerName = employer.SanitizeString();
    }

    public void UpdateAddress(string address)
    {
        Guard.Against.NullOrWhiteSpace(address);

        Address = address.SanitizeString();
    }

    public void UpdatePeymanRow(string? peymanRow)
    {
        if (!string.IsNullOrWhiteSpace(peymanRow))
            PeymanRow = peymanRow.SanitizeString();
    }


    public void UpdateAccountNumber(string? accountNumber)
    {
        if (!string.IsNullOrWhiteSpace(accountNumber))
            AccountNumber = accountNumber.SanitizeString();
    }

    public void UpdateInsuranceRate(long employeeRate, long employerRate, long unEmploymentRate)
    {
        Guard.Against.OutOfRange(employeeRate, nameof(employeeRate), 0, 100);
        Guard.Against.OutOfRange(employerRate, nameof(employerRate), 0, 100);
        Guard.Against.OutOfRange(unEmploymentRate, nameof(unEmploymentRate), 0, 100);

        EmployeeInsuranceRate = employeeRate;
        EmployerInsuranceRate = employerRate;
        UnEmploymentInsuranceRate = unEmploymentRate;
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PayrollSystem.Domain.Entities.PayItem;

namespace PayrollSystem.Application.Interfaces
{
    public interface IPayItemFormulaService
    {
        Task<PayItemFormula?> GetPayItemFormulaByIdAsync(long id);

        Task<PayItemFormula?> GetActivePayItemFormulaByPayItemId();
    }
}

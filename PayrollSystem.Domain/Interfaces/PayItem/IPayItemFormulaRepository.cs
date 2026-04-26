using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _0_Framework.GenericRepositoy.Interface;
using PayrollSystem.Domain.Entities.PayItem;

namespace PayrollSystem.Domain.Interfaces.PayItem
{
    public interface IPayItemFormulaRepository:IRepository<long,PayItemFormula>
    {
    }
}

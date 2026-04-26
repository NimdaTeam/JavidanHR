using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _0_Framework.GenericRepositoy.Service;
using Microsoft.EntityFrameworkCore;
using PayrollSystem.Domain.Entities.PayItem;
using PayrollSystem.Infrastructure.Persistence.Context;

namespace PayrollSystem.Infrastructure.Repositories.PayItem
{
    public class PayItemFormulaRepository:RepositoryService<long,PayItemFormula>
    {
        private readonly PayrollSystemContext _context;

        public PayItemFormulaRepository(PayrollSystemContext context) : base(context)
        {
            _context = context;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _0_Framework.GenericRepositoy.Service;
using HrSystem.Domain.Entities;
using HrSystem.Domain.Interfaces;
using HrSystem.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Infrastructure.Repositories
{
    public class EmployeeRepository:RepositoryService<long,Employee>,IEmployeeRepository
    {
        private readonly HrSystemContext _context;

        public EmployeeRepository(HrSystemContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<string?>> GetAllDepartments()
        {
            return (await _context.Employees
                .Where(x => !string.IsNullOrWhiteSpace(x.Department))
                .Select(x => x.Department)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync());
        }

        public async Task<List<string?>> GetAllPositions()
        {
            return (await _context.Employees
                .Where(x => !string.IsNullOrWhiteSpace(x.Position))
                .Select(x => x.Position)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync());
        }
    }
}

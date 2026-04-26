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

        public async Task<List<Employee>> GetAllEmployees()
        {
            return await _context.Employees
                .Include(x => x.FamilyMembers)
                .Include(x => x.AddressInfo)
                .Include(x => x.MilitaryService)
                .Include(x => x.MaritalInfo)
                .Include(x => x.Loans)
                .Include(x => x.Education)
                .Include(x => x.Trainings)
                .Include(x => x.WorkExperiences)
                .ToListAsync();
        }

        public async Task<Employee?> GetEmployeeForEdit(long empId)
        {
            return await _context.Employees
                .Include(x => x.FamilyMembers)
                .Include(x => x.AddressInfo)
                .Include(x => x.MilitaryService)
                .Include(x => x.MaritalInfo)
                .Include(x => x.Loans)
                .Include(x => x.Education)
                .Include(x => x.Trainings)
                .Include(x => x.WorkExperiences)
                .FirstOrDefaultAsync(x=>x.Id == empId);
        }

        public async Task<EmployeeAddress?> GetEmployeeAddress(long empId)
        {
            return await _context.EmployeeAddresses.FirstOrDefaultAsync(x => x.EmployeeId == empId);
        }

        public async Task<EmployeeEducation?> GetEmployeeEducation(long empId)
        {
            return await _context.EmployeeEducations.FirstOrDefaultAsync(x => x.EmployeeId == empId);
        }

        public async Task<List<EmployeeFamilyMember>> GetEmployeeFamilyMembers(long empId)
        {
            return await _context.EmployeeFamilyMembers.Where(x => x.EmployeeId == empId).ToListAsync();
        }

        public async Task<List<EmployeeLoan>> GetEmployeeLoans(long empId)
        {
            return await _context.EmployeeLoans.Where(x => x.EmployeeId == empId).ToListAsync();
        }

        public async Task<EmployeeMaritalInfo?> GetEmployeeMaritalInfo(long empId)
        {
            return await _context.EmployeeMaritalInfos.FirstOrDefaultAsync(x => x.EmployeeId == empId);
        }

        public async Task<EmployeeMilitaryService?> GetEmployeeMilitaryService(long empId)
        {
            return await _context.EmployeeMilitaryServices.FirstOrDefaultAsync(x => x.EmployeeId == empId);
        }

        public async Task<List<EmployeeTraining>> GetEmployeeTrainings(long empId)
        {
            return await _context.EmployeeTrainings.Where(x => x.EmployeeId == empId).ToListAsync();
        }

        public async Task<List<EmployeeWorkExperience>> GetEmployeeWorkExperiences(long empId)
        {
            return await _context.EmployeeWorkExperiences.Where(x => x.EmployeeId == empId).ToListAsync();
        }

        public async Task<int> GetWorkshopEmployeeCount(long workshopId)
        {
            return await _context.Employees.CountAsync(x => x.WorkShopId == workshopId);
        }
    }
}

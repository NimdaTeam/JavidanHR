using HrSystem.Application.DTO;
using HrSystem.Application.Interfaces;
using HrSystem.Domain.Entities;
using HrSystem.Domain.Interfaces;
using System.Linq.Expressions;
using _0_Framework.Extensions;
using _0_Framework.Utilities.Helpers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace HrSystem.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<Employee?> GetById(long id)
        {
            return await _employeeRepository.Get(id);
        }

        public async Task<List<Employee>> GetAll()
        {
            return await _employeeRepository.GetAll();
        }

        public async Task<List<Employee>> GetByCondition(Expression<Func<Employee, bool>> expression)
        {
            return await _employeeRepository.GetAllByCondition(expression);
        }

        public async Task<bool> IsExist(string employeeCode, string nationalCode)
        {
            return await _employeeRepository.Exists(x => x.EmployeeCode == employeeCode || x.NationalCode == nationalCode);
        }

        public async Task<bool> IsExistForUpdate(string employeeCode, string nationalCode, long id)
        {
            return await _employeeRepository.Exists(x => (x.EmployeeCode == employeeCode || x.NationalCode == nationalCode) && x.Id != id);
        }

        public async  Task<bool> Add(Employee entity)
        {
            try
            {
                var status = await _employeeRepository.Create(entity);
                status = await _employeeRepository.SaveChanges();
                return status;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public async Task<bool> Update(Employee entity)
        {
            try
            {
                var status = await _employeeRepository.Update(entity);
                status = await _employeeRepository.SaveChanges();
                return status;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public async Task<bool> Delete(long id)
        {
            try
            {
                var entity = await GetById(id);

                if (entity is null)
                    return false;

                var status = await _employeeRepository.Delete(id);
                status = await _employeeRepository.SaveChanges();
                return status;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

        }

        public async  Task<bool> Delete(Employee entity)
        {
            try
            {
                var status = await Delete(entity.Id);
                return status;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public async Task<List<string?>> GetAllDepartments()
        {
            return await _employeeRepository.GetAllDepartments();
        }

        public async Task<List<string?>> GetAllPositions()
        {
            return await _employeeRepository.GetAllPositions();
        }

        public async Task<List<EmployeeListItemDTO>> GetAllEmployeesList()
        {
            var employees = await _employeeRepository.GetAll();

            var result =  employees
                .OrderByDescending(e => e.Id)
                .Select(e => new EmployeeListItemDTO
                {
                    Id = e.Id,
                    EmployeeCode = e.EmployeeCode,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    ProfileImageUrl = e.ProfileImageUrl,

                    Department = e.Department,
                    Position = e.Position,
                    CooperationType = e.CooperationType,
                    IsActive = e.IsActive,
                    MaritalStatus = e.MaritalStatus,

                    PhoneNumber = e.PhoneNumber,
                    Email = e.Email,
                    Address = e.Address,
                    HireDate = e.HireDate,
                    BaseSalary = e.BaseSalary ?? 0,
                    PaymentMethod = e.PaymentMethod,
                    BankName = e.BankName,
                    InsuranceNumber = e.InsuranceNumber,
                    HealthStatus = e.HealthStatus,
                    SkillLevel = e.SkillLevel,
                    Notes = e.Notes
                })
                .ToList();

            return result;
        }

        public async Task<EmployeeDetailsVM> GetEmployeeDetails(long empId)
        {
            var employee = await _employeeRepository.Get(empId);

            if (employee is null)
                return new EmployeeDetailsVM();

            return employee.MapTo<EmployeeDetailsVM>();
        }
    }
}

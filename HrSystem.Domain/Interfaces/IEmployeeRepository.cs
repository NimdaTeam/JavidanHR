using _0_Framework.GenericRepositoy.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HrSystem.Domain.Entities;

namespace HrSystem.Domain.Interfaces
{
    public interface IEmployeeRepository: IRepository<long, Employee>
    {
        Task<List<string?>> GetAllDepartments();
        Task<List<string?>> GetAllPositions();
        Task<List<Employee>> GetAllEmployees();

        Task<Employee?> GetEmployeeForEdit(long empId);
        Task<EmployeeAddress?> GetEmployeeAddress(long empId);
        Task<EmployeeEducation?> GetEmployeeEducation(long empId);
        Task<List<EmployeeFamilyMember>> GetEmployeeFamilyMembers(long empId);
        Task<List<EmployeeLoan>> GetEmployeeLoans(long empId);
        Task<EmployeeMaritalInfo?> GetEmployeeMaritalInfo(long empId);
        Task<EmployeeMilitaryService?> GetEmployeeMilitaryService(long empId);
        Task<List<EmployeeTraining>> GetEmployeeTrainings(long empId);
        Task<List<EmployeeWorkExperience>> GetEmployeeWorkExperiences(long empId);

        Task<int> GetWorkshopEmployeeCount(long workshopId);
    }
}

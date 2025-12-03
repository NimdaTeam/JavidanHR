using System.Linq.Expressions;
using HrSystem.Application.DTO;
using HrSystem.Domain.Entities;

namespace HrSystem.Application.Interfaces
{
    public interface IEmployeeService
    {
        Task<Employee?> GetById(long id);
        Task<List<Employee>> GetAll();
        Task<List<Employee>> GetByCondition(Expression<Func<Employee,bool>> expression);

        Task<bool> IsExist(string employeeCode, string nationalCode);
        Task<bool> IsExistForUpdate(string employeeCode, string nationalCode, long id);

        Task<bool> Add(Employee entity);
        Task<bool> Update(Employee entity);
        Task<bool> Delete(long id);
        Task<bool> Delete(Employee entity);


        Task<List<string?>> GetAllDepartments();
        Task<List<string?>> GetAllPositions();
        Task<List<EmployeeListItemDTO>> GetAllEmployeesList();
        Task<EmployeeDetailsVM> GetEmployeeDetails(long empId);
    }
}

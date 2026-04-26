using System.Linq.Expressions;
using HrSystem.Application.DTO;
using HrSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HrSystem.Application.Interfaces
{
    public interface IEmployeeService
    {
        Task<Employee?> GetById(long id);
        Task<List<Employee>> GetAll();
        Task<List<Employee>> GetByCondition(Expression<Func<Employee, bool>> expression);
        Task<Employee?> SingleOrDefaultByCondition(Expression<Func<Employee, bool>> expression);

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
        Task<Employee?> GetEmployeeForEdit(long empId);

        Step1PersonalVM GetEmployeeData_Step1(Employee emp);
        Task<bool> AddNewEmployee_Step1(Step1PersonalVM model);
        Task<bool> UpdateEmployee_Step1(Employee employee, Step1PersonalVM emp);

        Task<Step2EducationVM?> GetStep2EducationData(long empId);
        Task<bool> UpdateEmployeeEducationInformation_Step2(Employee emp, Step2EducationVM model);

        Step3WorkExperienceVM GetStep3WorkExperienceData(Employee employee);
        Task<bool> UpdateEmployeeWorkExperience_Step3(Employee employee, Step3WorkExperienceVM model);

        Step4EmploymentVM GetStep4EmploymentData_Step4(Employee employee);
        Task<bool> UpdateEmployeeEmploymentData_Step4(Employee employee, Step4EmploymentVM model);
        
        Step5FinancialVM GetFinancialData_Step5(Employee employee);
        Task<bool> UpdateEmployeeFinancialData_Step5(Employee employee, Step5FinancialVM model);

        Task<EmployeeAddress?> GetEmployeeAddress(long empId);
        Task<EmployeeEducation?> GetEmployeeEducation(long empId);
        Task<List<EmployeeFamilyMember>> GetEmployeeFamilyMembers(long empId);
        Task<List<EmployeeLoan>> GetEmployeeLoans(long empId);
        Task<EmployeeMaritalInfo?> GetEmployeeMaritalInfo(long empId);
        Task<EmployeeMilitaryService?> GetEmployeeMilitaryService(long empId);
        Task<List<EmployeeTraining>> GetEmployeeTrainings(long empId);
        Task<List<EmployeeWorkExperience>> GetEmployeeWorkExperiences(long empId);

        CompleteEmployeeProfileVM MapEmployeeToCompleteProfileVm(Employee emp);

        EmployeeDetailsVM MapEmployeeToDetailsVm(Employee emp);


        Task<int> GetWorkshopEmployeeCount(long workshopId);
    }
}

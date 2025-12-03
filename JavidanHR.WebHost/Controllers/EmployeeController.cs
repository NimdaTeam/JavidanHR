using HrSystem.Application.DTO;
using HrSystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using System.Runtime.CompilerServices;
using _0_Framework.FileUploader;
using _0_Framework.Utilities.Helpers;
using _0_Framework.Utilities.NotificationSystem;
using _0_Framework.Utilities.Pagination;
using _0_Framework.Utilities.Security;
using AuthenticationSystem.SystemPermissions;
using DNTPersianUtils.Core;
using HrSystem.Application.common.Extensions;
using HrSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using WebHost.Helpers.GlobalHelpers;
using WebHost.PageSecurity;
using WebHost.Utilities;

namespace JavidanHR.WebHost.Controllers
{
    [Route("Employee")]
    [Authorize]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IFileUploadService _fileUploadService;

        public EmployeeController(IEmployeeService employeeService, IFileUploadService fileUploadService)
        {
            _employeeService = employeeService;
            _fileUploadService = fileUploadService;
        }

        [Route("All")]
        [Permission(SystemPermissions.EmployeesList)]
        public async Task<IActionResult> AllEmployees(string searchQuery = "", int page = 1, string department = "", string position = "", MaritalStatus? maritalStatus = null, CooperationType? cooperationType = null)
        {
            var model = await _employeeService.GetAllEmployeesList();

            ViewBag.TotalCount = model.Count;


            model = SearchHelper.Search(model,
                searchQuery.SanitizeString(),
                x => x.FullName
            ).ToList();

            if (!string.IsNullOrWhiteSpace(department))
                model = model.Where(x => x.Department == department).ToList();

            if (!string.IsNullOrWhiteSpace(position))
                model = model.Where(x => x.Position == position).ToList();

            if (maritalStatus is not null)
                model = model.Where(x => x.MaritalStatus == maritalStatus).ToList();

            if (cooperationType is not null)
                model = model.Where(x => x.CooperationType == cooperationType).ToList();

            var paginatedModel = PaginationHelper.Paginate(new PaginationRequest<EmployeeListItemDTO>()
            {
                CurrentPage = page,
                ModelList = model,
                SearchQuery = searchQuery.SanitizeString()
            });

            ViewBag.Departments = await _employeeService.GetAllDepartments();
            ViewBag.Positions = await _employeeService.GetAllPositions();
            return View(paginatedModel);
        }

        [Route("AddEmployee")]
        [Permission(SystemPermissions.CreateEmployee)]
        public async Task<IActionResult> AddEmployee(bool isUpdating = false, long? employeeId = null)
        {
            Step1PersonalVM model;

            if (isUpdating && employeeId > 0)
            {
                var emp = await _employeeService.GetById(employeeId.Value);
                if (emp is null)
                {
                    NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                    return this.RedirectToReferrer();
                }

                ViewData["EmployeeTitle"] = $"{emp.FullName} ({emp.EmployeeCode})";

                model = new Step1PersonalVM()
                {
                    NationalCode = emp.NationalCode ?? "",
                    CurrentImage = emp.ProfileImageUrl ?? "",
                    BirthDate = emp.BirthDate,
                    ChildrenCount = emp.ChildrenCount,
                    EmployeeCode = emp.EmployeeCode,
                    FirstName = emp.FirstName,
                    Gender = emp.Gender,
                    LastName = emp.LastName,
                    MaritalStatus = emp.MaritalStatus,
                    Id = emp.Id,
                    IsUpdating = true,
                    PersianBirthDateStringify = emp.BirthDate.ToShamsi()
                };

                return View(model);
            }

            model = new Step1PersonalVM();
            return View(model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("AddEmployee")]
        [Permission(SystemPermissions.CreateEmployee)]
        public async Task<IActionResult> AddEmployee([FromForm] Step1PersonalVM emp)
        {
            if (!emp.NationalCode.IsValidNationalCode())
            {
                NotificationSystem.ShowNotification(TempData, "کد ملی وارد شده معتبر نمی باشد", "", ApplicationMessagesIcon.ErrorIcon);
                return View(emp);
            }

            if (!emp.IsUpdating)
            {
                if (await _employeeService.IsExist(emp.EmployeeCode.SanitizeString(),
                        emp.NationalCode.SanitizeString()))
                {
                    NotificationSystem.ShowNotification(TempData, ApplicationMessages.DuplicateValueTitle, "", ApplicationMessagesIcon.ErrorIcon);
                    return View(emp);
                }

                var uploadedImageName = "";
                if (emp.ProfileImage is not null)
                {
                    uploadedImageName = await _fileUploadService.UploadFileAsync(emp.ProfileImage, "Employees", 2,
                        [".jpg", ".png", ".jpeg", ".webp"]);
                }

                var birthDate = emp.PersianBirthDateStringify.ToGregorianDateTime().Value;

                var status = await _employeeService.Add(new Employee()
                {
                    EmployeeCode = emp.EmployeeCode.SanitizeString(),
                    FirstName = emp.FirstName.SanitizeString(),
                    LastName = emp.LastName.SanitizeString(),
                    NationalCode = emp.NationalCode.SanitizeString(),
                    BirthDate = birthDate,
                    Gender = emp.Gender,
                    MaritalStatus = emp.MaritalStatus,
                    ChildrenCount = emp.ChildrenCount,
                    ProfileImageUrl = uploadedImageName ?? ""
                });

                if (status)
                {
                    NotificationSystem.ShowNotification(TempData, ApplicationMessages.OperationSuccessful, "",
                        ApplicationMessagesIcon.SuccessIcon);
                }
                else
                {
                    NotificationSystem.ShowNotification(TempData, ApplicationMessages.OperationFailed, "",
                        ApplicationMessagesIcon.ErrorIcon);
                }

                return RedirectToAction("AllEmployees");
            }
            else
            {
                var employee = await _employeeService.GetById(emp.Id.Value);

                if (employee is null)
                {
                    NotificationSystem.ShowNotification(TempData,ApplicationMessages.NotFound,"",ApplicationMessagesIcon.ErrorIcon);
                    return RedirectToAction("AllEmployees");
                }

                if (await _employeeService.IsExistForUpdate(emp.EmployeeCode.SanitizeString(),
                        emp.NationalCode.SanitizeString(),employee.Id))
                {
                    NotificationSystem.ShowNotification(TempData, ApplicationMessages.DuplicateValueTitle, "", ApplicationMessagesIcon.ErrorIcon);
                    return View(emp);
                }

                var uploadedImageName = "";
                if (emp.ProfileImage is not null)
                {
                    uploadedImageName = await _fileUploadService.UploadFileAsync(emp.ProfileImage, "Employees", 2,
                        [".jpg", ".png", ".jpeg", ".webp"]);

                    employee.ProfileImageUrl =uploadedImageName;
                }

                var deleteOldPhotoStatus =  _fileUploadService.DeleteFile(emp.CurrentImage, "Employees");
                if (deleteOldPhotoStatus)
                {
                    Console.WriteLine($"Error in deleting file: {emp.CurrentImage}");
                }

                var birthDate = emp.PersianBirthDateStringify.ToGregorianDateTime().Value;

                employee.EmployeeCode = emp.EmployeeCode.SanitizeString();
                employee.FirstName = emp.FirstName.SanitizeString();
                employee.LastName = emp.LastName.SanitizeString();
                employee.NationalCode = emp.NationalCode.SanitizeString();
                employee.BirthDate = birthDate;
                employee.Gender = emp.Gender;
                employee.MaritalStatus = emp.MaritalStatus;
                employee.ChildrenCount = emp.ChildrenCount;
                

                var status = await _employeeService.Update(employee);

                if (status)
                {
                    NotificationSystem.ShowNotification(TempData, ApplicationMessages.OperationSuccessful, "",
                        ApplicationMessagesIcon.SuccessIcon);
                }
                else
                {
                    NotificationSystem.ShowNotification(TempData, ApplicationMessages.OperationFailed, "",
                        ApplicationMessagesIcon.ErrorIcon);
                }

                return RedirectToAction("AllEmployees");
            }
        }


        [Route("ContactInformation/{employeeId}")]
        [Permission(SystemPermissions.EditEmployeeContactInfo)]
        public async Task<IActionResult> AddContactInformationToEmployee(long employeeId)
        {
            var emp = await _employeeService.GetById(employeeId);
            if (emp == null)
            {
                NotificationSystem
                    .ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);

                return this.RedirectToReferrer();
            }


            var model = new Step2ContactVM
            {
                Id = emp.Id,
                PhoneNumber = emp.PhoneNumber ?? "",
                Email = emp.Email ?? "",
                Address = emp.Address ?? "",
                EmergencyContactName = emp.EmergencyContactName ?? "",
                EmergencyContactPhone = emp.EmergencyContactPhone ?? ""
            };

            ViewData["EmployeeTitle"] = $"{emp.FullName} ({emp.EmployeeCode})";

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ContactInformation/{employeeId}")]
        [Permission(SystemPermissions.EditEmployeeContactInfo)]
        public async Task<IActionResult> AddContactInformationToEmployee([FromForm] Step2ContactVM cv)
        {
            var emp = await _employeeService.GetById(cv.Id);
            if (emp is null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                return this.RedirectToReferrer();
            }

            emp.PhoneNumber = cv.PhoneNumber.SanitizeString();
            emp.Email = cv.Email.SanitizeString();
            emp.Address = cv.Address.SanitizeString();
            emp.EmergencyContactName = cv.EmergencyContactName.SanitizeString();
            emp.EmergencyContactPhone = cv.EmergencyContactPhone.SanitizeString();

            var status = await _employeeService.Update(emp);

            if (status)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.OperationSuccessful, "",
                    ApplicationMessagesIcon.SuccessIcon);
            }
            else
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.OperationFailed, "",
                    ApplicationMessagesIcon.ErrorIcon);
            }

            return RedirectToAction("AllEmployees");
        }



        [Route("OrganizationalInformation/{employeeId}")]
        [Permission(SystemPermissions.EditEmployeeOrganizationalInfo)]
        public async Task<IActionResult> AddOrganizationalInformationToEmployee(long employeeId)
        {
            var emp = await _employeeService.GetById(employeeId);
            if (emp == null)
            {
                NotificationSystem
                    .ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);

                return this.RedirectToReferrer();
            }


            var model = new Step3OrganizationalVM()
            {
                Id = emp.Id,
                EducationLevel = emp.EducationLevel ?? new EducationLevel(),
                ContractType = emp.ContractType ?? new ContractType(),
                CooperationType = emp.CooperationType ?? new CooperationType(),
                Department = emp.Department ?? "",
                FieldOfStudy = emp.FieldOfStudy ?? "",
                HireDate = emp.HireDate ?? DateTime.MinValue,
                Position = emp.Position ?? "",
                HireDatePersianStringify = emp.HireDate is not null ? emp.HireDate.ToShamsi() : ""
            };

            ViewData["EmployeeTitle"] = $"{emp.FullName} ({emp.EmployeeCode})";

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("OrganizationalInformation/{employeeId}")]
        [Permission(SystemPermissions.EditEmployeeOrganizationalInfo)]
        public async Task<IActionResult> AddOrganizationalInformationToEmployee([FromForm] Step3OrganizationalVM ov)
        {
            var emp = await _employeeService.GetById(ov.Id);
            if (emp is null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                return this.RedirectToReferrer();
            }

            var hireDate = ov.HireDatePersianStringify.ToGregorianDateTime().Value;

            emp.Department = ov.Department.SanitizeString();
            emp.Position = ov.Position.SanitizeString();
            emp.EducationLevel = ov.EducationLevel;
            emp.FieldOfStudy = ov.FieldOfStudy.SanitizeString();
            emp.ContractType = ov.ContractType;
            emp.CooperationType = ov.CooperationType;
            emp.HireDate = hireDate;

            var status = await _employeeService.Update(emp);

            if (status)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.OperationSuccessful, "",
                    ApplicationMessagesIcon.SuccessIcon);
            }
            else
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.OperationFailed, "",
                    ApplicationMessagesIcon.ErrorIcon);
            }

            return RedirectToAction("AllEmployees");
        }



        [Route("FinancialInformation/{employeeId}")]
        [Permission(SystemPermissions.EditEmployeeFinancialInfo)]
        public async Task<IActionResult> AddFinancialInformationToEmployee(long employeeId)
        {
            var emp = await _employeeService.GetById(employeeId);
            if (emp == null)
            {
                NotificationSystem
                    .ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);

                return this.RedirectToReferrer();
            }

            var model = new Step4FinancialVM()
            {
                Id = emp.Id,
                CardNumber = emp.CardNumber ?? "",
                AccountNumber = emp.AccountNumber ?? "",
                BankName = emp.BankName ?? "",
                BaseSalary = emp.BaseSalary ?? 0,
                Benefits = emp.Benefits ?? 0,
                PaymentMethod = emp.PaymentMethod ?? new PaymentMethod()
            };

            ViewData["EmployeeTitle"] = $"{emp.FullName} ({emp.EmployeeCode})";

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("FinancialInformation/{employeeId}")]
        [Permission(SystemPermissions.EditEmployeeFinancialInfo)]
        public async Task<IActionResult> AddFinancialInformationToEmployee([FromForm] Step4FinancialVM fv)
        {
            var emp = await _employeeService.GetById(fv.Id);
            if (emp is null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                return this.RedirectToReferrer();
            }

            emp.BaseSalary = fv.BaseSalary;
            emp.Benefits = fv.Benefits;
            emp.BankName = fv.BankName.SanitizeString();
            emp.AccountNumber = fv.AccountNumber.SanitizeString();
            emp.CardNumber = fv.CardNumber.SanitizeString();
            emp.PaymentMethod = fv.PaymentMethod;

            var status = await _employeeService.Update(emp);

            if (status)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.OperationSuccessful, "",
                    ApplicationMessagesIcon.SuccessIcon);
            }
            else
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.OperationFailed, "",
                    ApplicationMessagesIcon.ErrorIcon);
            }

            return RedirectToAction("AllEmployees");
        }


        [Route("AdditionalInformation/{employeeId}")]
        [Permission(SystemPermissions.EditEmployeeAdditionalInfo)]
        public async Task<IActionResult> AddAdditionalInformationToEmployee(long employeeId)
        {
            var emp = await _employeeService.GetById(employeeId);
            if (emp == null)
            {
                NotificationSystem
                    .ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);

                return this.RedirectToReferrer();
            }

            var model = new Step5AdditionalVM()
            {
                Id = emp.Id,
                AssignedProjects = emp.AssignedProjects ?? "",
                Certificates = emp.Certificates ?? "",
                CompletedTrainings = emp.CompletedTrainings ?? "",
                HealthStatus = emp.HealthStatus ?? new HealthStatus(),
                InsuranceNumber = emp.InsuranceNumber ?? "",
                Notes = emp.Notes ?? "",
                SkillLevel = emp.SkillLevel ?? ""
            };

            ViewData["EmployeeTitle"] = $"{emp.FullName} ({emp.EmployeeCode})";

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("AdditionalInformation/{employeeId}")]
        [Permission(SystemPermissions.EditEmployeeAdditionalInfo)]
        public async Task<IActionResult> AddAdditionalInformationToEmployee([FromForm] Step5AdditionalVM av)
        {
            var emp = await _employeeService.GetById(av.Id);
            if (emp is null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                return this.RedirectToReferrer();
            }

            emp.InsuranceNumber = av.InsuranceNumber.SanitizeString();
            emp.HealthStatus = av.HealthStatus;
            emp.SkillLevel = av.SkillLevel.SanitizeString();
            emp.CompletedTrainings = av.CompletedTrainings.SanitizeString();
            emp.Certificates = av.Certificates.SanitizeString();
            emp.AssignedProjects = av.AssignedProjects.SanitizeString();
            emp.Notes = av.Notes.SanitizeString();

            var status = await _employeeService.Update(emp);

            if (status)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.OperationSuccessful, "",
                    ApplicationMessagesIcon.SuccessIcon);
            }
            else
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.OperationFailed, "",
                    ApplicationMessagesIcon.ErrorIcon);
            }

            return RedirectToAction("AllEmployees");
        }

        [Route("Delete/{id}")]
        [Permission(SystemPermissions.DeleteEmployee)]
        public async Task<IActionResult> Delete(long id)
        {
            var emp = await _employeeService.GetById(id);
            if (emp == null)
            {
                NotificationSystem.ShowNotification(TempData,ApplicationMessages.NotFound,"",ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction("AllEmployees");
            }

            var status = await _employeeService.Delete(emp);

            if (status)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.OperationSuccessful, "",
                    ApplicationMessagesIcon.SuccessIcon);
            }
            else
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.OperationFailed, "",
                    ApplicationMessagesIcon.ErrorIcon);
            }

            return RedirectToAction("AllEmployees");

        }


        [Route("EmployeeDetails/{empId}")]
        [Permission(SystemPermissions.ViewEmployeeDetails)]
        public async Task<IActionResult> EmployeeDetails(long empId)
        {
            var emp = await _employeeService.GetById(empId);
            if (emp is null)
            {
                NotificationSystem.ShowNotification(TempData,ApplicationMessages.NotFound,"",ApplicationMessagesIcon.ErrorIcon);
                return this.RedirectToReferrer();
            }

            var details = await _employeeService.GetEmployeeDetails(empId);

            return View(details);
        }

        [Route("PrintProfile/{empId}")]
        [Permission(SystemPermissions.PrintEmployeeProfile)]
        public async Task<IActionResult> PrintProfile(long empId)
        {
            var emp = await _employeeService.GetById(empId);
            if (emp is null)
            {
                NotificationSystem.ShowNotification(TempData, ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                return this.RedirectToReferrer();
            }

            var details = await _employeeService.GetEmployeeDetails(empId);

            return View(details);
        }
    }
}

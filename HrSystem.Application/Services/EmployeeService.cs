using _0_Framework.Extensions;
using _0_Framework.FileUploader;
using _0_Framework.Utilities.Helpers;
using _0_Framework.Utilities.NotificationSystem;
using _0_Framework.Utilities.Security;
using AngleSharp.Dom;
using AutoMapper;
using DNTPersianUtils.Core;
using HrSystem.Application.common.Extensions;
using HrSystem.Application.DTO;
using HrSystem.Application.Interfaces;
using HrSystem.Domain.Entities;
using HrSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Linq.Expressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace HrSystem.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<EmployeeService> _logger;
        private readonly IFileUploadService _fileUploadService;

        public EmployeeService(IEmployeeRepository employeeRepository, ILogger<EmployeeService> logger, IFileUploadService fileUploadService)
        {
            _employeeRepository = employeeRepository;
            _logger = logger;
            _fileUploadService = fileUploadService;
        }

        public async Task<Employee?> GetById(long id)
        {
            return await _employeeRepository.GetAsync(id);
        }

        public async Task<List<Employee>> GetAll()
        {
            return await _employeeRepository.GetAllAsync();
        }

        public async Task<List<Employee>> GetByCondition(Expression<Func<Employee, bool>> expression)
        {
            return await _employeeRepository.GetAllByConditionAsync(expression);
        }

        public async Task<List<Employee>> GetEmployeesByWorkshopIdAsync(long workshopId, CancellationToken cancellationToken = default)
        {
            return await GetByCondition(x => x.WorkShopId == workshopId);
        }

        public async Task<Employee?> SingleOrDefaultByCondition(Expression<Func<Employee, bool>> expression)
        {
            return await _employeeRepository.SingleOrDefaultByConditionAsync(expression);
        }

        public async Task<bool> IsExist(string employeeCode, string nationalCode)
        {
            return await _employeeRepository.ExistsAsync(x => x.EmployeeCode == employeeCode || x.NationalCode == nationalCode);
        }

        public async Task<bool> IsExistForUpdate(string employeeCode, string nationalCode, long id)
        {
            return await _employeeRepository.ExistsAsync(x => (x.EmployeeCode == employeeCode || x.NationalCode == nationalCode) && x.Id != id);
        }

        public async Task<bool> Add(Employee entity)
        {
            return await _employeeRepository.ExecuteInTransactionAsync(async () =>
            {
                await _employeeRepository.AddAsync(entity);
                var saveResult = await _employeeRepository.SaveChangesAsync();
                return saveResult;
            });
        }

        public async Task<bool> Update(Employee entity)
        {
            return await _employeeRepository.ExecuteInTransactionAsync(async () =>
            {
                await _employeeRepository.UpdateAsync(entity);
                var saveResult = await _employeeRepository.SaveChangesAsync();
                return saveResult;
            });
        }

        public async Task<bool> Delete(long id)
        {
            var entity = await GetById(id);

            if (entity is null)
                return false;

            return await _employeeRepository.ExecuteInTransactionAsync(async () =>
            {
                entity.SoftDelete();

                await _employeeRepository.UpdateAsync(entity);
                var saveResult = await _employeeRepository.SaveChangesAsync();
                return saveResult;
            });
        }

        public async Task<bool> Delete(Employee entity)
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
            var employees = await _employeeRepository.GetAllEmployees();

            var result = employees
                .OrderByDescending(e => e.Id)
                .Select(e => new EmployeeListItemDTO
                {
                    Id = e.Id,
                    EmployeeCode = e.EmployeeCode,
                    FullName = $"{e.FirstName} {e.LastName}",
                    ProfileImageUrl = e.ProfileImageUrl,
                    Department = e.Department,
                    Position = e.Position,
                    IsActive = e.IsActive,
                    CooperationType = e.CooperationType ?? new CooperationType(),
                    MaritalStatus = e.MaritalInfo?.MaritalStatus ?? MaritalStatus.Single,
                    Step1Completed = !string.IsNullOrEmpty(e.FirstName) && !string.IsNullOrEmpty(e.NationalCode),
                    Step2Completed = e.Education != null || e.Trainings.Any(),
                    Step3Completed = e.WorkExperiences.Any(),
                    Step4Completed = e is { Department: not null, Position: not null, HireDate: not null },
                    Step5Completed = e is { BaseSalary: not null, PaymentMethod: not null },
                    IsInformationConfirmed = e.IsProfileCompletedByEmployee
                })
                .ToList();

            return result;
        }

        public async Task<EmployeeDetailsVM> GetEmployeeDetails(long empId)
        {
            var employee = await _employeeRepository.GetEmployeeForEdit(empId);

            if (employee is null)
                return new EmployeeDetailsVM();

            return MapEmployeeToDetailsVm(employee);
        }

        public async Task<Employee?> GetEmployeeForEdit(long empId)
        {
            return await _employeeRepository.GetEmployeeForEdit(empId);
        }

        public Step1PersonalVM GetEmployeeData_Step1(Employee emp)
        {
            try
            {
                var model = new Step1PersonalVM
                {
                    Id = emp.Id,
                    WorkshopId = emp.WorkShopId,
                    IsUpdating = true,
                    EmployeeCode = emp.EmployeeCode,
                    FirstName = emp.FirstName ?? "",
                    LastName = emp.LastName ?? "",
                    PreviousLastName = emp.PreviousLastName,
                    FathersName = emp.FathersName ?? "",
                    Nickname = emp.Nickname,
                    NationalCode = emp.NationalCode ?? "",
                    IdNumber = emp.IdNumber,
                    IdIssuePlace = emp.IdIssuePlace,
                    BirthPlace = emp.BirthPlace,
                    PersianBirthDateStringify = emp.BirthDate.ToShamsi(),
                    MobilePhone = emp.MobilePhone ?? "",
                    HomePhone = emp.HomePhone,
                    Gender = emp.Gender,
                    MilitaryStatus = emp.MilitaryService?.Status ?? MilitaryStatus.None,
                    ServiceEndDatePersian = emp.MilitaryService?.ServiceEndDate?.ToShamsi() ?? "",
                    ExemptionReason = emp.MilitaryService?.ExemptionReason ?? "",
                    MaritalStatus = emp.MaritalInfo?.MaritalStatus ?? MaritalStatus.Single,
                    MarriageDatePersian = emp.MaritalInfo?.MarriageDate?.ToShamsi() ?? "",
                    HousingStatus = emp.AddressInfo?.HousingStatus ?? HousingStatus.Personal,
                    PersonalAddress = emp.AddressInfo?.PersonalAddress ?? "",
                    RentalAddress = emp.AddressInfo?.RentalAddress ?? "",
                    FamilyMembers = emp.FamilyMembers?.Select(f => new FamilyMemberVM
                    {
                        FullName = f.FullName ?? "",
                        FathersName = f.FathersName ?? "",
                        PersianBirthDate = f.BirthDate?.ToShamsi() ?? "",
                        Relation = f.Relation ?? "",
                        AddressOrWorkplace = f.AddressOrWorkplace ?? "",
                        Id = f.Id
                    }).ToList() ?? [],
                    CurrentImage = emp.ProfileImageUrl ?? ""
                };
                return model;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }

        public async Task<bool> AddNewEmployee_Step1(Step1PersonalVM model)
        {
            try
            {
                var birthDate = model.PersianBirthDateStringify.ToGregorianDateTime();
                if (birthDate is null)
                    return false;

                var uploadedImageName = model.CurrentImage;
                if (model.ProfileImage is not null)
                {
                    uploadedImageName = await _fileUploadService.UploadFileAsync(model.ProfileImage, "Employees", 2,
                        [".jpg", ".png", ".jpeg", ".webp"]);
                }

                var status = await Add(new Employee()
                {
                    UserId = model.UserId ?? 0,
                    EmployeeCode = model.EmployeeCode.SanitizeString(),
                    FirstName = model.FirstName.SanitizeString(),
                    LastName = model.LastName.SanitizeString(),
                    FathersName = model.FathersName.SanitizeString(),
                    NationalCode = model.NationalCode.SanitizeString(),
                    BirthDate = birthDate.Value,
                    Gender = model.Gender,
                    ProfileImageUrl = uploadedImageName ?? "",
                    BirthPlace = model.BirthPlace?.SanitizeString() ?? "",
                    PreviousLastName = model.PreviousLastName?.SanitizeString() ?? "",
                    Nickname = model.Nickname?.SanitizeString() ?? "",
                    MobilePhone = model.MobilePhone?.SanitizeString() ?? "",
                    IdIssuePlace = model.IdIssuePlace?.SanitizeString() ?? "",
                    HomePhone = model.HomePhone?.SanitizeString() ?? "",
                    WorkShopId = model.WorkshopId
                });

                if (!status)
                {
                    return false;
                }

                var createdEmployee = await
                    SingleOrDefaultByCondition(x => x.NationalCode == model.NationalCode.SanitizeString());

                if (createdEmployee is null)
                    return false;


                //military Service
                createdEmployee.MilitaryService = new EmployeeMilitaryService()
                {
                    EmployeeId = createdEmployee.Id,
                    ExemptionReason = model.ExemptionReason?.SanitizeString() ?? "",
                    ServiceEndDate = model.ServiceEndDatePersian?.ToGregorianDateTime() ?? null,
                    Status = model.MilitaryStatus
                };

                //Address Info
                createdEmployee.AddressInfo = new EmployeeAddress()
                {
                    EmployeeId = createdEmployee.Id,
                    HousingStatus = model.HousingStatus,
                    PersonalAddress = model.PersonalAddress?.SanitizeString() ?? "",
                    RentalAddress = model.RentalAddress?.SanitizeString() ?? ""
                };


                //Marital Info
                createdEmployee.MaritalInfo = new EmployeeMaritalInfo()
                {
                    EmployeeId = createdEmployee.Id,
                    MaritalStatus = model.MaritalStatus,
                    MarriageDate = model.MarriageDatePersian?.ToGregorianDateTime() ?? null
                };
                var familyMembers = new List<EmployeeFamilyMember>();
                model.FamilyMembers.ForEach(x => familyMembers.Add(new EmployeeFamilyMember()
                {
                    AddressOrWorkplace = x.AddressOrWorkplace.SanitizeString(),
                    BirthDate = x.PersianBirthDate.ToGregorianDateTime() ?? null,
                    EmployeeId = createdEmployee.Id,
                    FathersName = x.FathersName.SanitizeString(),
                    FullName = x.FullName.SanitizeString(),
                    Relation = x.Relation.SanitizeString()
                }));

                createdEmployee.FamilyMembers = familyMembers;

                var updateStatus = await Update(createdEmployee);

                return updateStatus;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return false;
            }
        }

        public async Task<bool> UpdateEmployee_Step1(Employee employee, Step1PersonalVM emp)
        {
            try
            {
                var birthDate = emp.PersianBirthDateStringify.ToGregorianDateTime();
                if (birthDate is null)
                    return false;

                var uploadedImageName = emp.CurrentImage;
                if (emp.ProfileImage is not null)
                {
                    uploadedImageName = await _fileUploadService.UploadFileAsync(emp.ProfileImage, "Employees", 2,
                        [".jpg", ".png", ".jpeg", ".webp"]);
                }

                var deleteOldPhotoStatus = _fileUploadService.DeleteFile(emp.CurrentImage, "Employees");
                if (!deleteOldPhotoStatus)
                {
                    _logger.LogError($"Error occured in Deleting Employee Photo: {emp.CurrentImage} - Id: {emp.Id}");
                }




                employee.EmployeeCode = emp.EmployeeCode.SanitizeString();
                employee.FirstName = emp.FirstName.SanitizeString();
                employee.LastName = emp.LastName.SanitizeString();
                employee.FathersName = emp.FathersName.SanitizeString();
                employee.NationalCode = emp.NationalCode.SanitizeString();
                employee.BirthDate = birthDate.Value;
                employee.Gender = emp.Gender;
                employee.ProfileImageUrl = uploadedImageName ?? "";
                employee.BirthPlace = emp.BirthPlace?.SanitizeString() ?? "";
                employee.PreviousLastName = emp.PreviousLastName?.SanitizeString() ?? "";
                employee.Nickname = emp.Nickname?.SanitizeString() ?? "";
                employee.MobilePhone = emp.MobilePhone?.SanitizeString() ?? "";
                employee.IdIssuePlace = emp.IdIssuePlace?.SanitizeString() ?? "";
                employee.HomePhone = emp.HomePhone?.SanitizeString() ?? "";

                employee.WorkShopId = emp.WorkshopId;



                //military Service
                if (employee.MilitaryService is not null)
                {
                    employee.MilitaryService.EmployeeId = employee.Id;
                    employee.MilitaryService.ExemptionReason = emp.ExemptionReason?.SanitizeString() ?? "";
                    employee.MilitaryService.ServiceEndDate = emp.ServiceEndDatePersian?.ToGregorianDateTime() ?? null;
                    employee.MilitaryService.Status = emp.MilitaryStatus;
                }
                else
                {
                    employee.MilitaryService = new EmployeeMilitaryService()
                    {
                        EmployeeId = employee.Id,
                        ExemptionReason = emp.ExemptionReason?.SanitizeString() ?? "",
                        ServiceEndDate = emp.ServiceEndDatePersian?.ToGregorianDateTime() ?? null,
                        Status = emp.MilitaryStatus
                    };
                }


                //Address Info
                if (employee.AddressInfo is not null)
                {
                    employee.AddressInfo.EmployeeId = employee.Id;
                    employee.AddressInfo.HousingStatus = emp.HousingStatus;
                    employee.AddressInfo.PersonalAddress = emp.PersonalAddress?.SanitizeString() ?? "";
                    employee.AddressInfo.RentalAddress = emp.RentalAddress?.SanitizeString() ?? "";
                }
                else
                {
                    employee.AddressInfo = new EmployeeAddress()
                    {
                        EmployeeId = employee.Id,
                        HousingStatus = emp.HousingStatus,
                        PersonalAddress = emp.PersonalAddress?.SanitizeString() ?? "",
                        RentalAddress = emp.RentalAddress?.SanitizeString() ?? ""
                    };
                }


                //Marital Info
                if (employee.MaritalInfo is not null)
                {
                    employee.MaritalInfo.EmployeeId = employee.Id;
                    employee.MaritalInfo.MaritalStatus = emp.MaritalStatus;
                    employee.MaritalInfo.MarriageDate = emp.MarriageDatePersian?.ToGregorianDateTime() ?? null;
                }
                else
                {
                    employee.MaritalInfo = new EmployeeMaritalInfo()
                    {
                        EmployeeId = employee.Id,
                        MaritalStatus = emp.MaritalStatus,
                        MarriageDate = emp.MarriageDatePersian?.ToGregorianDateTime() ?? null
                    };
                }

                //family members
                if (emp.FamilyMembers.Any())
                {
                    var newMembersIds = emp.FamilyMembers.Where(t => t.Id.HasValue).Select(t => t.Id.Value).ToList();
                    var membersToRemove = employee.FamilyMembers.Where(t => !newMembersIds.Contains(t.Id)).ToList();
                    foreach (var t in membersToRemove)
                        employee.FamilyMembers.Remove(t);

                    foreach (var item in emp.FamilyMembers)
                    {
                        if (item.Id is > 0)
                        {
                            var existing = employee.FamilyMembers.FirstOrDefault(t => t.Id == item.Id.Value);
                            if (existing != null)
                            {
                                existing.FullName = item.FullName.SanitizeString();
                                existing.AddressOrWorkplace = item.AddressOrWorkplace.SanitizeString();
                                existing.BirthDate = item.PersianBirthDate.ToGregorianDateTime() ?? null;
                                existing.FathersName = item.FathersName?.SanitizeString();
                                existing.Relation = item.Relation?.SanitizeString() ?? "";
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(item.FullName))
                        {
                            employee.FamilyMembers.Add(new EmployeeFamilyMember()
                            {
                                FullName = item.FullName.SanitizeString(),
                                AddressOrWorkplace = item.AddressOrWorkplace.SanitizeString(),
                                BirthDate = item.PersianBirthDate.ToGregorianDateTime() ?? null,
                                FathersName = item.FathersName?.SanitizeString(),
                                Relation = item.Relation?.SanitizeString() ?? "",
                            });
                        }
                    }
                }

                var updateStatus = await Update(employee);

                return updateStatus;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return false;
            }
        }

        //step 2
        public async Task<Step2EducationVM?> GetStep2EducationData(long empId)
        {
            try
            {
                var employee = await GetEmployeeForEdit(empId);

                if (employee is null)
                    return null;

                var model = new Step2EducationVM
                {
                    EmployeeId = employee.Id,
                    IsUpdating = true,
                    AcademicLevel = employee.Education?.Level,
                    FieldOfStudy = employee.Education?.FieldOfStudy,
                    StartYearPersian = employee.Education?.StartYear?.ToString(),
                    EndYearPersian = employee.Education?.EndYear?.ToString(),
                    InstituteName = employee.Education?.InstituteName,
                    InstituteCity = employee.Education?.InstituteCity,
                    InstituteAddress = employee.Education?.InstituteAddress,
                    Trainings = employee.Trainings.Select(t => new EmployeeTrainingVM
                    {
                        Id = t.Id,
                        CourseName = t.CourseName,
                        Institute = t.Institute,
                        Hours = t.Hours?.ToString(),
                        CertificateNumberAndDate = t.CertificateNumberAndDate,
                        Notes = t.Notes
                    }).ToList()
                };

                return model;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return null;
            }
        }
        public async Task<bool> UpdateEmployeeEducationInformation_Step2(Employee emp, Step2EducationVM model)
        {
            try
            {
                // === بروزرسانی تحصیلات رسمی ===
                emp.Education ??= new EmployeeEducation { EmployeeId = emp.Id };

                emp.Education.Level = model.AcademicLevel ?? EducationLevel.BelowDiploma;
                emp.Education.FieldOfStudy = model.FieldOfStudy?.SanitizeString() ?? "";
                emp.Education.StartYear = !string.IsNullOrEmpty(model.StartYearPersian)
                    ? int.Parse(model.StartYearPersian)
                    : null;
                emp.Education.EndYear =
                    !string.IsNullOrEmpty(model.EndYearPersian) ? int.Parse(model.EndYearPersian) : null;
                emp.Education.InstituteName = model.InstituteName?.SanitizeString();
                emp.Education.InstituteCity = model.InstituteCity?.SanitizeString();
                emp.Education.InstituteAddress = model.InstituteAddress?.SanitizeString();

                // === بروزرسانی دوره‌های آموزشی ===
                if (model.Trainings.Any())
                {
                    var newTrainingIds = model.Trainings.Where(t => t.Id.HasValue).Select(t => t.Id.Value).ToList();
                    var trainingsToRemove = emp.Trainings.Where(t => !newTrainingIds.Contains(t.Id)).ToList();
                    foreach (var t in trainingsToRemove)
                        emp.Trainings.Remove(t);

                    foreach (var item in model.Trainings)
                    {
                        if (item.Id is > 0)
                        {
                            var existing = emp.Trainings.FirstOrDefault(t => t.Id == item.Id.Value);
                            if (existing != null)
                            {
                                existing.CourseName = item.CourseName.SanitizeString();
                                existing.Institute = item.Institute.SanitizeString();
                                existing.Hours = !string.IsNullOrEmpty(item.Hours) ? int.Parse(item.Hours) : null;
                                existing.CertificateNumberAndDate = item.CertificateNumberAndDate?.SanitizeString();
                                existing.Notes = item.Notes?.SanitizeString();
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(item.CourseName) && !string.IsNullOrWhiteSpace(item.Institute))
                        {
                            emp.Trainings.Add(new EmployeeTraining
                            {
                                CourseName = item.CourseName.SanitizeString(),
                                Institute = item.Institute.SanitizeString(),
                                Hours = !string.IsNullOrEmpty(item.Hours) ? int.Parse(item.Hours) : null,
                                CertificateNumberAndDate = item.CertificateNumberAndDate?.SanitizeString(),
                                Notes = item.Notes?.SanitizeString()
                            });
                        }


                    }
                }

                var success = await Update(emp);
                _logger.LogInformation($"Saved {emp.Id} - {emp.UserId} - {emp.FirstName} {emp.LastName} Education data with result : {success}");

                return success;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return false;
            }

            return false;
        }

        public Step3WorkExperienceVM GetStep3WorkExperienceData(Employee employee)
        {
            try
            {
                var model = new Step3WorkExperienceVM
                {
                    EmployeeId = employee.Id,
                    WorkExperiences = employee.WorkExperiences.Select(w => new WorkExperienceVM
                    {
                        Id = w.Id,
                        Type = w.Type,
                        Organization = !string.IsNullOrWhiteSpace(w.Organization) ? w.Organization.SanitizeString() : "صندوق قرض الحسنه جاویدان",
                        Position = w.Position,
                        DirectManager = w.DirectManager,
                        StartDatePersian = w.StartDate.ToShamsi(),
                        EndDatePersian = w.EndDate?.ToShamsi() ?? "",
                        HasInsurance = w.HasInsurance,
                        TerminationReason = w.TerminationReason,
                        Notes = w.Notes
                    }).OrderByDescending(x => x.StartDatePersian).ToList()
                };

                return model;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }

        public async Task<bool> UpdateEmployeeWorkExperience_Step3(Employee employee, Step3WorkExperienceVM model)
        {
            try
            {
                if (!model.WorkExperiences.Any())
                    return true;

                var isStartDatesValid = model.WorkExperiences.Select(item => item.StartDatePersian.ToGregorianDateTime().HasValue).ToList();

                if (isStartDatesValid.Any(x => x == false))
                    return false;

                // حذف تجربیات حذف‌شده توسط کاربر
                var newIds = model.WorkExperiences.Where(x => x.Id.HasValue).Select(x => x.Id.Value).ToList();
                var toRemove = employee.WorkExperiences.Where(x => !newIds.Contains(x.Id)).ToList();
                foreach (var item in toRemove)
                    employee.WorkExperiences.Remove(item);

                // اضافه یا بروزرسانی
                foreach (var item in model.WorkExperiences)
                {
                    var startDate = item.StartDatePersian.ToGregorianDateTime();
                    if (!startDate.HasValue)
                    {
                        return false;
                    }
                    var endDate = string.IsNullOrEmpty(item.EndDatePersian) ? (DateTime?)null : item.EndDatePersian.ToGregorianDateTime();

                    if (item.Id is > 0)
                    {
                        var existing = employee.WorkExperiences.FirstOrDefault(x => x.Id == item.Id.Value);
                        if (existing != null)
                        {
                            existing.Type = item.Type;
                            existing.Organization = !string.IsNullOrWhiteSpace(item.Organization) ? item.Organization.SanitizeString() : "صندوق قرض الحسنه جاویدان";
                            existing.Position = item.Position.SanitizeString();
                            existing.DirectManager = item.DirectManager?.SanitizeString();
                            existing.StartDate = startDate.Value;
                            existing.EndDate = endDate;
                            existing.HasInsurance = item.HasInsurance;
                            existing.TerminationReason = item.TerminationReason?.SanitizeString();
                            existing.Notes = item.Notes?.SanitizeString();
                        }
                    }
                    else
                    {
                        employee.WorkExperiences.Add(new EmployeeWorkExperience
                        {
                            Type = item.Type,
                            Organization = !string.IsNullOrWhiteSpace(item.Organization) ? item.Organization.SanitizeString() : "صندوق قرض الحسنه جاویدان",
                            Position = item.Position.SanitizeString(),
                            DirectManager = item.DirectManager?.SanitizeString(),
                            StartDate = startDate.Value,
                            EndDate = endDate,
                            HasInsurance = item.HasInsurance,
                            TerminationReason = item.TerminationReason?.SanitizeString(),
                            Notes = item.Notes?.SanitizeString()
                        });
                    }
                }

                var success = await Update(employee);

                return success;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return false;
            }
        }

        public Step4EmploymentVM GetStep4EmploymentData_Step4(Employee employee)
        {
            try
            {
                var model = new Step4EmploymentVM
                {
                    EmployeeId = employee.Id,
                    Department = employee.Department ?? "",
                    Position = employee.Position ?? "",
                    ContractType = employee.ContractType,
                    CooperationType = employee.CooperationType,
                    HireDatePersian = employee.HireDate?.ToShamsi() ?? "",
                    InsuranceNumber = employee.InsuranceNumber,
                    HealthStatus = employee.HealthStatus,
                    AnnualLeaveDays = employee.AnnualLeaveDays,
                    UsedLeaveDays = employee.UsedLeaveDays,
                    PerformanceScore = employee.PerformanceScore,
                    RetirementStatus = employee.RetirementStatus,
                    RetirementDatePersian = employee.RetirementDate?.ToShamsi() ?? "",
                    IsActive = employee.IsActive,
                    TerminationReason = employee.TerminationReason,
                    TerminationDatePersian = employee.TerminationDate?.ToShamsi() ?? "",
                    Management = employee.Management??"",
                    Unit = employee.Unit??""
                };

                return model;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }

        public async Task<bool> UpdateEmployeeEmploymentData_Step4(Employee employee, Step4EmploymentVM model)
        {
            try
            {
                employee.Management = model.Management.SanitizeString();
                employee.Department = model.Department.SanitizeString();
                employee.Unit = model.Unit.SanitizeString();
                employee.Position = model.Position.SanitizeString();
                employee.ContractType = model.ContractType;
                employee.CooperationType = model.CooperationType;
                employee.HireDate = model.HireDatePersian.ToGregorianDateTime();
                employee.InsuranceNumber = model.InsuranceNumber?.SanitizeString();
                employee.HealthStatus = model.HealthStatus;
                employee.AnnualLeaveDays = model.AnnualLeaveDays;
                employee.UsedLeaveDays = model.UsedLeaveDays;
                employee.PerformanceScore = model.PerformanceScore;
                employee.RetirementStatus = model.RetirementStatus;
                employee.RetirementDate = string.IsNullOrEmpty(model.RetirementDatePersian) ? null : model.RetirementDatePersian.ToGregorianDateTime();
                employee.IsActive = model.IsActive;
                employee.TerminationReason = model.IsActive ? null : model.TerminationReason?.SanitizeString();
                employee.TerminationDate = model.IsActive ? null : (string.IsNullOrEmpty(model.TerminationDatePersian) ? null : model.TerminationDatePersian.ToGregorianDateTime());

                var success = await Update(employee);

                return success;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return false;
            }
        }

        public Step5FinancialVM GetFinancialData_Step5(Employee employee)
        {
            try
            {
                var model = new Step5FinancialVM
                {
                    EmployeeId = employee.Id,
                    BaseSalary = employee.BaseSalary.ToString(),
                    Benefits = employee.Benefits.ToString(),
                    BankName = employee.BankName,
                    AccountNumber = employee.AccountNumber,
                    CardNumber = employee.CardNumber,
                    PaymentMethod = employee.PaymentMethod,
                    Loans = employee.Loans.Select(l => new EmployeeLoanVM
                    {
                        Id = l.Id,
                        Amount = l.Amount.ToString(CultureInfo.CurrentCulture),
                        BorrowerName = l.BorrowerName,
                        Guarantors = l.Guarantors,
                        SettlementDatePersian = l.SettlementDate?.ToShamsi() ?? ""
                    }).ToList()
                };

                return model;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }

        public async Task<bool> UpdateEmployeeFinancialData_Step5(Employee employee, Step5FinancialVM model)
        {
            try
            {
                var baseSalary = decimal.Parse(model.BaseSalary?.SanitizeString().ToEnglishNumbers()?.Replace("٬", "") ?? "0");
                var benefits = decimal.Parse(model.Benefits?.SanitizeString().ToEnglishNumbers().Replace("٬", "") ?? "0");


                employee.BaseSalary = baseSalary;
                employee.Benefits = benefits;
                employee.BankName = model.BankName?.SanitizeString();
                employee.AccountNumber = model.AccountNumber?.SanitizeString();
                employee.CardNumber = model.CardNumber?.SanitizeString();
                employee.PaymentMethod = model.PaymentMethod;

                // مدیریت وام‌ها
                if (model.Loans.Any())
                {
                    var newIds = model.Loans.Where(x => x.Id.HasValue).Select(x => x.Id.Value).ToList();
                    var toRemove = employee.Loans.Where(x => !newIds.Contains(x.Id)).ToList();
                    foreach (var loan in toRemove) employee.Loans.Remove(loan);

                    foreach (var item in model.Loans)
                    {
                        var amount = decimal.Parse(item.Amount?.SanitizeString().ToEnglishNumbers().Replace("٬", "") ?? "0");
                        if (item.Id is > 0)
                        {
                            var existing = employee.Loans.FirstOrDefault(x => x.Id == item.Id.Value);
                            if (existing != null)
                            {

                                existing.Amount = amount;
                                existing.BorrowerName = item.BorrowerName.SanitizeString();
                                existing.Guarantors = item.Guarantors?.SanitizeString();
                                existing.SettlementDate = string.IsNullOrEmpty(item.SettlementDatePersian) ? null : item.SettlementDatePersian.ToGregorianDateTime();
                            }
                        }
                        else if (amount > 0 && !string.IsNullOrWhiteSpace(item.BorrowerName))
                        {
                            employee.Loans.Add(new EmployeeLoan
                            {
                                Amount = amount,
                                BorrowerName = item.BorrowerName.SanitizeString(),
                                Guarantors = item.Guarantors?.SanitizeString(),
                                SettlementDate = string.IsNullOrEmpty(item.SettlementDatePersian) ? null : item.SettlementDatePersian.ToGregorianDateTime()
                            });
                        }
                    }

                }


                var success = await Update(employee);

                return success;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return false;
            }
        }

        public async Task<EmployeeAddress?> GetEmployeeAddress(long empId)
        {
            return await _employeeRepository.GetEmployeeAddress(empId);
        }

        public async Task<EmployeeEducation?> GetEmployeeEducation(long empId)
        {
            return await _employeeRepository.GetEmployeeEducation(empId);
        }

        public async Task<List<EmployeeFamilyMember>> GetEmployeeFamilyMembers(long empId)
        {
            return await _employeeRepository.GetEmployeeFamilyMembers(empId);
        }

        public async Task<List<EmployeeLoan>> GetEmployeeLoans(long empId)
        {
            return await _employeeRepository.GetEmployeeLoans(empId);
        }

        public async Task<EmployeeMaritalInfo?> GetEmployeeMaritalInfo(long empId)
        {
            return await _employeeRepository.GetEmployeeMaritalInfo(empId);
        }

        public async Task<EmployeeMilitaryService?> GetEmployeeMilitaryService(long empId)
        {
            return await _employeeRepository.GetEmployeeMilitaryService(empId);
        }

        public async Task<List<EmployeeTraining>> GetEmployeeTrainings(long empId)
        {
            return await _employeeRepository.GetEmployeeTrainings(empId);
        }

        public async Task<List<EmployeeWorkExperience>> GetEmployeeWorkExperiences(long empId)
        {
            return await _employeeRepository.GetEmployeeWorkExperiences(empId);
        }

        public CompleteEmployeeProfileVM MapEmployeeToCompleteProfileVm(Employee emp)
        {
            if (emp == null) return new CompleteEmployeeProfileVM();

            return new CompleteEmployeeProfileVM
            {
                EmployeeId = emp.Id,

                // مشخصات فردی
                FirstName = emp.FirstName ?? "",
                LastName = emp.LastName ?? "",
                FathersName = emp.FathersName ?? "",
                Nickname = emp.Nickname ?? "",
                PreviousLastName = emp.PreviousLastName ?? "",
                IdNumber = emp.IdNumber ?? "",
                IdIssuePlace = emp.IdIssuePlace ?? "",
                BirthPlace = emp.BirthPlace ?? "",
                NationalCode = emp.NationalCode ?? "",
                PersianBirthDate = emp.BirthDate.ToShamsi(),
                MobilePhone = emp.MobilePhone ?? "",
                HomePhone = emp.HomePhone ?? "",
                Gender = emp.Gender,

                // نظام وظیفه
                MilitaryStatus = emp.MilitaryService?.Status ?? MilitaryStatus.None,
                ServiceEndDatePersian = emp.MilitaryService?.ServiceEndDate?.ToShamsi() ?? "",
                ExemptionReason = emp.MilitaryService?.ExemptionReason ?? "",

                // تحصیلات
                EducationLevel = emp.Education?.Level ?? EducationLevel.BelowDiploma,
                FieldOfStudy = emp.Education?.FieldOfStudy ?? "",
                StartYear = emp.Education?.StartYear,
                EndYear = emp.Education?.EndYear,
                InstituteName = emp.Education?.InstituteName ?? "",
                InstituteCity = emp.Education?.InstituteCity ?? "",
                InstituteAddress = emp.Education?.InstituteAddress ?? "",

                // وضعیت تأهل
                MaritalStatus = emp.MaritalInfo?.MaritalStatus ?? MaritalStatus.Single,
                MarriageDatePersian = emp.MaritalInfo?.MarriageDate?.ToShamsi() ?? "",

                // آدرس و مسکن
                HousingStatus = emp.AddressInfo?.HousingStatus ?? HousingStatus.Personal,
                PersonalAddress = emp.AddressInfo?.PersonalAddress ?? "",
                RentalAddress = emp.AddressInfo?.RentalAddress ?? "",

                // اعضای خانواده
                FamilyMembers = emp.FamilyMembers?.Select(f => new FamilyMemberVM
                {
                    FullName = f.FullName ?? "",
                    FathersName = f.FathersName ?? "",
                    PersianBirthDate = f.BirthDate?.ToShamsi() ?? "",
                    Relation = f.Relation ?? "",
                    AddressOrWorkplace = f.AddressOrWorkplace ?? ""
                }).ToList() ?? new List<FamilyMemberVM>(),

                // دوره‌های آموزشی
                Trainings = emp.Trainings?.Select(t => new TrainingVM
                {
                    Id = t.Id,
                    CourseName = t.CourseName ?? "",
                    Institute = t.Institute ?? "",
                    Hours = t.Hours,
                    CertificateNumberAndDate = t.CertificateNumberAndDate ?? "",
                    Notes = t.Notes ?? ""
                }).ToList() ?? new List<TrainingVM>(),

                // سوابق کاری
                WorkExperiences = emp.WorkExperiences?.Select(w => new WorkExperienceVM
                {
                    Id = w.Id,
                    Type = w.Type,
                    Organization = w.Organization ?? "",
                    Position = w.Position ?? "",
                    DirectManager = w.DirectManager ?? "",
                    StartDatePersian = w.StartDate.ToShamsi(),
                    EndDatePersian = w.EndDate?.ToShamsi() ?? "",
                    HasInsurance = w.HasInsurance,
                    TerminationReason = w.TerminationReason ?? "",
                    Notes = w.Notes ?? ""
                }).ToList() ?? new List<WorkExperienceVM>(),

                // تسهیلات
                Loans = emp.Loans?.Select(l => new LoanVM
                {
                    Id = l.Id,
                    Amount = l.Amount,
                    BorrowerName = l.BorrowerName ?? "",
                    Guarantors = l.Guarantors ?? "",
                    SettlementDatePersian = l.SettlementDate?.ToShamsi() ?? ""
                }).ToList() ?? new List<LoanVM>()
            };
        }

        public EmployeeDetailsVM MapEmployeeToDetailsVm(Employee emp)
        {
            if (emp == null)
                return new EmployeeDetailsVM();

            var address = "";
            if (string.IsNullOrWhiteSpace(emp.AddressInfo?.PersonalAddress) &&
                string.IsNullOrWhiteSpace(emp.AddressInfo?.RentalAddress))
            {
                address = "-";
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(emp.AddressInfo.PersonalAddress))
                {
                    address += $"{emp.AddressInfo.PersonalAddress} (ملک شخصی)";
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(emp.AddressInfo?.RentalAddress))
                    {
                        if (!string.IsNullOrWhiteSpace(emp.AddressInfo?.PersonalAddress))
                        {
                            address += $" - {emp.AddressInfo.RentalAddress} (ملک استیجاری)";
                        }

                        address = $"{emp.AddressInfo?.RentalAddress} (ملک استیجاری)" ?? "-";
                    }
                }

            }


            return new EmployeeDetailsVM
            {
                UserId = emp.UserId,
                IsProfileCompletedByEmployee = emp.IsProfileCompletedByEmployee,
                Id = emp.Id,
                EmployeeCode = emp.EmployeeCode ?? "",
                FirstName = emp.FirstName ?? "",
                LastName = emp.LastName ?? "",
                ProfileImageUrl = emp.ProfileImageUrl,
                FatherName = emp.FathersName ?? "-",
                MilitaryServiceStatus = emp.MilitaryService?.Status.ToPersian() ?? "-",

                HomeAddress = address,

                // اطلاعات هویتی
                NationalCode = emp.NationalCode ?? "",
                BirthDate = emp.BirthDate,
                Gender = emp.Gender,

                MobilePhone = emp.MobilePhone,
                HomePhone = emp.HomePhone,

                // وضعیت تأهل و خانواده
                MaritalStatus = emp.MaritalInfo?.MaritalStatus,


                FamilyMembers = emp.FamilyMembers ?? new List<EmployeeFamilyMember>(),


                // تحصیلات
                Education = emp.Education,
                EducationLevel = emp.Education?.Level,
                FieldOfStudy = emp.Education?.FieldOfStudy,

                // دوره‌های آموزشی
                Trainings = emp.Trainings ?? new List<EmployeeTraining>(),

                // سوابق کاری
                WorkExperiences = emp.WorkExperiences?.OrderByDescending(w => w.StartDate).ToList()
                                 ?? new List<EmployeeWorkExperience>(),

                // اطلاعات سازمانی و استخدامی
                Management = emp.Management,
                Department = emp.Department,
                Unit = emp.Unit,
                Position = emp.Position,
                ContractType = emp.ContractType,
                CooperationType = emp.CooperationType,

                HireDate = emp.HireDate,


                // اطلاعات مالی
                BaseSalary = emp.BaseSalary,
                Benefits = emp.Benefits,

                BankName = emp.BankName,
                AccountNumber = emp.AccountNumber,
                CardNumber = emp.CardNumber,
                PaymentMethod = emp.PaymentMethod,


                // بیمه و سلامت
                InsuranceNumber = emp.InsuranceNumber,
                HealthStatus = emp.HealthStatus,


                // مرخصی
                AnnualLeaveDays = emp.AnnualLeaveDays,
                UsedLeaveDays = emp.UsedLeaveDays,


                // عملکرد و بازنشستگی
                PerformanceScore = emp.PerformanceScore,
                RetirementStatus = emp.RetirementStatus,
                RetirementDate = emp.RetirementDate,

                // وضعیت فعالیت
                IsActive = emp.IsActive,
                TerminationReason = emp.TerminationReason,
                TerminationDate = emp.TerminationDate,

                // وام‌ها
                Loans = emp.Loans ?? new List<EmployeeLoan>(),


                MaritalInfo = emp.MaritalInfo,
                PersonalAddress = emp.AddressInfo?.PersonalAddress ?? "-",
                RentalAddress = emp.AddressInfo?.RentalAddress ?? "-",
                FathersName = emp.FathersName ?? "-",
                ExemptionReason = emp.MilitaryService?.ExemptionReason ?? "-",
                ServiceEndDatePersian = emp.MilitaryService?.ServiceEndDate.ToShamsi() ?? "-",
                MilitaryStatus = emp.MilitaryService?.Status ?? new MilitaryStatus(),
                MilitaryStatusDisplay = emp.MilitaryService?.Status.ToPersian() ?? "-",
                MarriageDatePersian = emp.MaritalInfo?.MarriageDate.ToShamsi() ?? "-",
                Notes = "-",
                BirthPlace = emp.BirthPlace ?? "-",
                EducationEndYear = emp.Education?.EndYear.ToString() ?? "-",
                EducationInstituteCity = emp.Education?.InstituteCity ?? "-",
                EducationInstituteName = emp.Education?.InstituteName ?? "-",
                EducationStartYear = emp.Education?.StartYear.ToString() ?? "-",
                HousingStatus = emp.AddressInfo?.HousingStatus ?? new HousingStatus(),
                HousingStatusDisplay = emp.AddressInfo?.HousingStatus.ToPersian() ?? "-",
                IdIssuePlace = emp.IdIssuePlace ?? "-",
                IdNumber = emp.IdNumber ?? "-",
                Nickname = emp.Nickname ?? "-",
                PreviousLastName = emp.PreviousLastName ?? "-"
            };
        }

        public async Task<int> GetWorkshopEmployeeCount(long workshopId)
        {
            return await _employeeRepository.GetWorkshopEmployeeCount(workshopId);
        }
    }
}

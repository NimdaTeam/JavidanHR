using _0_Framework.Utilities.NotificationSystem;
using _0_Framework.Utilities.Pagination;
using AuthenticationSystem.Services.Repositories;
using HrSystem.Application.Interfaces;
using JavidanHR.WebHost.Utilities.ReturnUrlFilter;
using Microsoft.AspNetCore.Mvc;
using PayrollSystem.Application.DTOs;
using PayrollSystem.Application.Interfaces;
using WebHost.Helpers.GlobalHelpers;

namespace JavidanHR.WebHost.Controllers.PayrollSystem.Contract
{
    /// <summary>
    /// Controller for managing employee contracts
    /// </summary>
    [Route("Payroll/Contract")]
    public class ContractController : BaseController
    {
        private readonly IContractService _contractService;
        private readonly IPayItemService _payItemService;
        private readonly IWorkshopService _workshopService;
        private readonly IEmployeeService _employeeService;
        private readonly IUserRepository _userService;
        private readonly IRequestContextAccessor _requestContext;
        private readonly ILogger<ContractController> _logger;

        public ContractController(IContractService contractService, IPayItemService payItemService, IWorkshopService workshopService, IEmployeeService employeeService, IUserRepository userService, IRequestContextAccessor requestContext, ILogger<ContractController> logger)
        {
            _contractService = contractService;
            _payItemService = payItemService;
            _workshopService = workshopService;
            _employeeService = employeeService;
            _userService = userService;
            _requestContext = requestContext;
            _logger = logger;
        }

        /// <summary>
        /// Display paginated list of all contracts
        /// </summary>
        [HttpGet]
        [Route("All")]
        public async Task<IActionResult> AllContracts(
            string searchQuery,
            string statusFilter,
            int page = 1,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var contracts = await _contractService.GetAllContractsWithDtoAsync(cancellationToken);

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    contracts = SearchHelper.Search(
                        contracts,
                        searchQuery,
                        x => x.EmployeeName,
                        x => x.WorkshopName
                    ).ToList();
                }

                // Apply status filter
                if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
                {
                    contracts = contracts.Where(c => c.Status.ToString() == statusFilter).ToList();
                }


                var paginatedResult = PaginationHelper.Paginate(new PaginationRequest<ContractDto>()
                {
                    CurrentPage = page,
                    ModelList = contracts,
                    SearchQuery = searchQuery
                });

                // Pass filter to view
                ViewBag.StatusFilter = statusFilter;
                ViewBag.TotalActive = contracts.Count(c => c.IsActive);
                ViewBag.TotalInactive = contracts.Count(c => !c.IsActive);
                ViewBag.TotalTerminated = contracts.Count(c => c.Status == global::PayrollSystem.Domain.Entities.Contract.ContractStatus.Terminated);

                return View(paginatedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contracts list");
                ShowNotification(ApplicationMessages.ErrorOccurred, ApplicationMessagesIcon.ErrorIcon);
                return View(new PaginationResult<ContractDto>());
            }
        }

        /// <summary>
        /// Display form for creating new contract
        /// </summary>
        [HttpGet]
        [Route("Add")]
        public async Task<IActionResult> AddContract(CancellationToken cancellationToken = default)
        {
            try
            {
                await PrepareViewBagForDropdowns(cancellationToken);
                return View(new CreateContractDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading add contract page");
                ShowNotification(ApplicationMessages.ErrorOccurred, ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction(nameof(AllContracts));
            }
        }

        /// <summary>
        /// Create new contract
        /// </summary>
        [HttpPost]
        [Route("Add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddContract(
            CreateContractDto model,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await PrepareViewBagForDropdowns(cancellationToken);
                    ShowNotification(ApplicationMessages.MalformedInput, ApplicationMessagesIcon.ErrorIcon);
                    return View(model);
                }

                var result = await _contractService.CreateContractAsync(model, cancellationToken);

                if (result != null && model.PayItems.Any())
                {
                    foreach (var pi in model.PayItems)
                    {
                        pi.ContractId = result.Id;
                        await _contractService.AssignPayItemToContractAsync(pi, cancellationToken);
                    }
                }

                if (result != null)
                {
                    ShowNotification(ApplicationMessages.RecordCreated);
                    return RedirectToAction(nameof(EditContract), new { id = result.Id });
                }

                ShowNotification(ApplicationMessages.ErrorOccurred, ApplicationMessagesIcon.ErrorIcon);
                await PrepareViewBagForDropdowns(cancellationToken);
                return View(model);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("overlap"))
            {
                _logger.LogWarning(ex, "Contract date overlap for employee {EmployeeId}", model.EmployeeId);
                ShowNotification("تاریخ قرارداد با قرارداد فعال دیگری تداخل دارد", ApplicationMessagesIcon.ErrorIcon);
                await PrepareViewBagForDropdowns(cancellationToken);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contract");
                ShowNotification(ApplicationMessages.ErrorOccurred, ApplicationMessagesIcon.ErrorIcon);
                await PrepareViewBagForDropdowns(cancellationToken);
                return View(model);
            }
        }

        /// <summary>
        /// Display form for editing contract
        /// </summary>
        [HttpGet]
        [Route("Edit/{id}")]
        public async Task<IActionResult> EditContract(long id, CancellationToken cancellationToken = default)
        {
            try
            {
                var contract = await _contractService.GetContractByIdAsync(id, cancellationToken);
                if (contract == null)
                {
                    ShowNotification(ApplicationMessages.NotFound, ApplicationMessagesIcon.ErrorIcon);
                    return RedirectToAction(nameof(AllContracts));
                }

                await PrepareViewBagForDropdowns(cancellationToken);

                // Load all available pay items for assignment
                var allPayItems = await _payItemService.GetAllPayItemsAsync(cancellationToken);
                ViewBag.AvailablePayItems = allPayItems.Where(p => p.IsActive).ToList();

                return View(contract);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contract {ContractId}", id);
                ShowNotification(ApplicationMessages.ErrorOccurred, ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction(nameof(AllContracts));
            }
        }

        /// <summary>
        /// Update contract basic information
        /// </summary>
        [HttpPost]
        [Route("Update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateContract(
            UpdateContractDto model,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ShowNotification(ApplicationMessages.MalformedInput, ApplicationMessagesIcon.ErrorIcon);
                    return RedirectToAction(nameof(EditContract), new { id = model.Id });
                }

                var result = await _contractService.UpdateContractAsync(model, cancellationToken);

                if (result)
                {
                    ShowNotification(ApplicationMessages.RecordUpdated);
                }
                else
                {
                    ShowNotification(ApplicationMessages.ErrorOccurred, ApplicationMessagesIcon.ErrorIcon);
                }

                return RedirectToAction(nameof(EditContract), new { id = model.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contract {ContractId}", model.Id);
                ShowNotification(ApplicationMessages.ErrorOccurred, ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction(nameof(EditContract), new { id = model.Id });
            }
        }

        /// <summary>
        /// Assign pay item to contract
        /// </summary>
        [HttpPost]
        [Route("AssignPayItem")]
        public async Task<IActionResult> AssignPayItem(
            [FromBody] AssignPayItemToContractDto model,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _contractService.AssignPayItemToContractAsync(model, cancellationToken);

                if (result)
                {
                    return Json(new { success = true, message = "آیتم حقوقی با موفقیت اضافه شد" });
                }

                return Json(new { success = false, message = "خطا در افزودن آیتم حقوقی" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning pay item to contract");
                return Json(new { success = false, message = "خطا در افزودن آیتم حقوقی" });
            }
        }

        /// <summary>
        /// Remove pay item from contract
        /// </summary>
        [HttpPost]
        [Route("RemovePayItem")]
        public async Task<IActionResult> RemovePayItem(
            long contractId,
            long payItemId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _contractService.RemovePayItemFromContractAsync(
                    contractId,
                    payItemId,
                    cancellationToken);

                if (result)
                {
                    return Json(new { success = true, message = "آیتم حقوقی حذف شد" });
                }

                return Json(new { success = false, message = "خطا در حذف آیتم حقوقی" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing pay item from contract");
                return Json(new { success = false, message = "خطا در حذف آیتم حقوقی" });
            }
        }

        /// <summary>
        /// Activate contract
        /// </summary>
        [HttpPost]
        [Route("Activate/{id}")]
        public async Task<IActionResult> ActivateContract(long id, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _contractService.ActivateContractAsync(id, cancellationToken);

                if (result)
                {
                    ShowNotification("قرارداد فعال شد", ApplicationMessagesIcon.SuccessIcon);
                }
                else
                {
                    ShowNotification("خطا در فعال‌سازی قرارداد", ApplicationMessagesIcon.ErrorIcon);
                }

                return RedirectToAction(nameof(EditContract), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating contract {ContractId}", id);
                ShowNotification(ApplicationMessages.ErrorOccurred, ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction(nameof(EditContract), new { id });
            }
        }

        /// <summary>
        /// Deactivate contract
        /// </summary>
        [HttpPost]
        [Route("Deactivate/{id}")]
        public async Task<IActionResult> DeactivateContract(long id, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _contractService.DeactivateContractAsync(id, cancellationToken);

                if (result)
                {
                    ShowNotification("قرارداد غیرفعال شد");
                }
                else
                {
                    ShowNotification("خطا در غیرفعال‌سازی قرارداد", ApplicationMessagesIcon.ErrorIcon);
                }

                return RedirectToAction(nameof(EditContract), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating contract {ContractId}", id);
                ShowNotification(ApplicationMessages.ErrorOccurred, ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction(nameof(EditContract), new { id });
            }
        }

        /// <summary>
        /// Terminate contract
        /// </summary>
        [HttpPost]
        [Route("Terminate/{id}")]
        public async Task<IActionResult> TerminateContract(
            long id,
            DateTime? terminationDate = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _contractService.TerminateContractAsync(
                    id,
                    terminationDate ?? DateTime.Now,
                    cancellationToken);

                if (result)
                {
                    ShowNotification("قرارداد خاتمه یافت", ApplicationMessagesIcon.ErrorIcon);
                }
                else
                {
                    ShowNotification("خطا در خاتمه قرارداد", ApplicationMessagesIcon.ErrorIcon);
                }

                return RedirectToAction(nameof(AllContracts));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error terminating contract {ContractId}", id);
                ShowNotification(ApplicationMessages.OperationFailed, ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction(nameof(EditContract), new { id });
            }
        }

        /// <summary>
        /// Delete contract (soft delete)
        /// </summary>
        [HttpPost]
        [Route("Delete/{id}")]
        public async Task<IActionResult> DeleteContract(long id, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _contractService.DeleteContractAsync(id, cancellationToken);

                if (result)
                {
                    return Json(new { success = true, message = ApplicationMessages.RecordDeleted });
                }

                return Json(new { success = false, message = ApplicationMessages.OperationFailed });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contract {ContractId}", id);
                return Json(new { success = false, message = ApplicationMessages.OperationFailed });
            }
        }

        /// <summary>
        /// Get workshop by employee (AJAX)
        /// </summary>
        [HttpGet]
        [Route("GetEmployeeWorkshop/{employeeId}")]
        public async Task<IActionResult> GetEmployeeWorkshop(long employeeId, CancellationToken cancellationToken = default)
        {
            try
            {
                var employee = await _employeeService.GetById(employeeId);

                if (employee == null)
                {
                    return Json(new { success = false, message = "کارمند یافت نشد" });
                }

                var workshop = await _workshopService.GetWorkshopByIdAsync(employee.WorkShopId??0,cancellationToken);

                if (workshop == null)
                {
                    return Json(new { success = false, message = "کارگاه یافت نشد" });
                }

                return Json(new
                {
                    success = true,
                    workshopId = workshop.Id,
                    workshopName = workshop.Name,
                    workshopCode = workshop.Code
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employee workshop");
                return Json(new { success = false, message = "خطا در دریافت اطلاعات کارگاه" });
            }
        }

        /// <summary>
        /// Get pay item details (AJAX)
        /// </summary>
        [HttpGet]
        [Route("GetPayItemDetails/{payItemId}")]
        public async Task<IActionResult> GetPayItemDetails(long payItemId, CancellationToken cancellationToken = default)
        {
            try
            {
                var payItem = await _payItemService.GetPayItemByIdAsync(payItemId, cancellationToken);

                if (payItem == null)
                {
                    return Json(new { success = false, message = "آیتم حقوقی یافت نشد" });
                }

                return Json(new
                {
                    success = true,
                    id = payItem.Id,
                    name = payItem.Name,
                    systemCode = payItem.SystemCode,
                    type = payItem.Type,
                    dataType = payItem.DataType,
                    requiresUserInput = payItem.DataType == global::PayrollSystem.Domain.Entities.PayItem.PayItem.PayItemDataType.UserInput
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pay item details");
                return Json(new { success = false, message = "خطا در دریافت اطلاعات آیتم حقوقی" });
            }
        }

        /// <summary>
        /// Prepare dropdowns for views
        /// </summary>
        private async Task PrepareViewBagForDropdowns(CancellationToken cancellationToken)
        {
            var employees = await _employeeService.GetAll();
            var workshops = await _workshopService.GetAllWorkshopsAsync(cancellationToken);
            var allPayItems = await _payItemService.GetAllValidCodesAsync();
            ViewBag.AvailablePayItems = allPayItems.Where(p => p.IsActive).ToList();

            ViewBag.Employees = employees.Where(e => e.IsActive).ToList();
            ViewBag.Workshops = workshops.ToList();
        }
    }
}

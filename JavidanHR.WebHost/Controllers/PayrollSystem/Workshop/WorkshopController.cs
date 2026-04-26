using _0_Framework.Utilities.NotificationSystem;
using _0_Framework.Utilities.Pagination;
using _0_Framework.Utilities.Security;
using AutoMapper;
using JavidanHR.WebHost.Utilities.ReturnUrlFilter;
using Microsoft.AspNetCore.Mvc;
using PayrollSystem.Application.DTOs;
using PayrollSystem.Application.Interfaces;
using WebHost.Helpers.GlobalHelpers;

namespace JavidanHR.WebHost.Controllers.PayrollSystem.Workshop
{
    [Route("payroll/workshop")]
    public class WorkshopController : BaseController
    {
        private readonly IWorkshopService _workshopService;
        private readonly IMapper _mapper;
        private readonly ILogger<WorkshopController> _logger;
        private readonly IRequestContextAccessor _ctx;

        public WorkshopController(IWorkshopService workshopService, IMapper mapper, ILogger<WorkshopController> logger, IRequestContextAccessor ctx)
        {
            _workshopService = workshopService;
            _mapper = mapper;
            _logger = logger;
            _ctx = ctx;
        }

        [Route("all")]
        public async Task<IActionResult> AllWorkshops(string searchQuery = "", int page = 1)
        {
            var workshops = await _workshopService.GetAllWorkshopsAsync();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                workshops = SearchHelper.Search(
                    workshops,
                    searchQuery,
                    x => x.Name,
                    x => x.Code,
                    x => x.EmployerName
                ).ToList();
            }

            var paginatedModel = PaginationHelper.Paginate(new PaginationRequest<WorkshopDto>()
            {
                CurrentPage = page,
                ModelList = workshops,
                SearchQuery = searchQuery
            });

            return View(paginatedModel);
        }

        [Route("add")]
        public IActionResult AddWorkshop()
        {
            return View();
        }

        [Route("add")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddWorkshop(CreateWorkshopDto model)
        {
            if (!ModelState.IsValid)
            {
                ShowNotification(ApplicationMessages.MalformedInput, "", ApplicationMessagesIcon.ErrorIcon);
                return SmartRedirect(_ctx.Context.ReturnUrl);
            }

            var createdEntity = await _workshopService.CreateWorkshopAsync(model);

            if (createdEntity == null)
            {
                ShowNotification(ApplicationMessages.OperationFailed, "", ApplicationMessagesIcon.ErrorIcon);
                return SmartRedirect(_ctx.Context.ReturnUrl);
            }

            ShowNotification(ApplicationMessages.OperationSuccessful, "", ApplicationMessagesIcon.SuccessIcon);
            return SmartRedirect(_ctx.Context.ReturnUrl);
        }

        [Route("update/{workshopId}")]
        public async Task<IActionResult> UpdateWorkshop(long workshopId)
        {
            var workshop = await _workshopService.GetWorkshopByIdAsync(workshopId);
            if (workshop is not null)
            {
                var dto = _mapper.Map<UpdateWorkshopDto>(workshop);
                return View(dto);
            }

            ShowNotification(ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
            return SmartRedirect(_ctx.Context.ReturnUrl);
        }

        [HttpPost]
        [Route("update/{workshopId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateWorkshop(UpdateWorkshopDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ShowNotification(ApplicationMessages.MalformedInput, "", ApplicationMessagesIcon.ErrorIcon);
                    return View(model);
                }

                var updateResult = await _workshopService.UpdateWorkshopAsync(model);
                if (updateResult is null)
                {
                    ShowNotification(ApplicationMessages.OperationFailed, "", ApplicationMessagesIcon.ErrorIcon);
                    return View(model);
                }

                ShowNotification(ApplicationMessages.OperationSuccessful, "", ApplicationMessagesIcon.SuccessIcon);
                return RedirectToAction("AllWorkshops");
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return RedirectToAction("AllWorkshops");
            }
        }

        [Route("delete/{id}")]
        public async Task<IActionResult> DeleteWorkshop(long id)
        {
            var workshop = await _workshopService.GetWorkshopByIdAsync(id);
            if (workshop is null)
            {
                ShowNotification(ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                return SmartRedirect(_ctx.Context.ReturnUrl);
            }

            if (await _workshopService.GetWorkshopEmployeeCountAsync(workshop.Id) > 0)
            {
                ShowNotification("امکان حذف کارگاهی که کارمند به آن اختصاص داده شده است، وجود ندارد.", "", ApplicationMessagesIcon.ErrorIcon);
                return SmartRedirect(_ctx.Context.ReturnUrl);
            }

            var deleteResult = await _workshopService.DeleteWorkshopAsync(workshop.Id);

            if (deleteResult)
            {
                ShowNotification(ApplicationMessages.OperationSuccessful);
                return SmartRedirect(_ctx.Context.ReturnUrl);
            }

            ShowNotification(ApplicationMessages.OperationFailed, "", ApplicationMessagesIcon.ErrorIcon);
            return SmartRedirect(_ctx.Context.ReturnUrl);
        }
    }
}

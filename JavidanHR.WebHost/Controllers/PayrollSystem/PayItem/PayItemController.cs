using _0_Framework.Utilities.NotificationSystem;
using _0_Framework.Utilities.Pagination;
using JavidanHR.WebHost.Utilities.ReturnUrlFilter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PayrollSystem.Application.DTOs;
using PayrollSystem.Application.Interfaces;
using PayrollSystem.Application.Utilities;
using PayrollSystem.Domain.Common;

namespace JavidanHR.WebHost.Controllers.PayrollSystem.PayItem
{
    [Route("Payroll/PayItem")]
    public class PayItemController : BaseController
    {
        private readonly IPayItemService _payItemService;
        private readonly IRequestContextAccessor _ctx;
        private readonly ILogger<PayItemController> _logger;

        public PayItemController(IPayItemService payItemService, IRequestContextAccessor ctx, ILogger<PayItemController> logger)
        {
            _payItemService = payItemService;
            _ctx = ctx;
            _logger = logger;
        }

        [Route("All")]
        public async Task<IActionResult> AllPayItems(string searchQuery, int page = 1)
        {
            var result = await _payItemService.GetAllValidCodesAsync();
            if (!string.IsNullOrWhiteSpace(searchQuery))
                result = result.Where(x => x.Name.Contains(searchQuery)).ToList();

            var paginatedModel = PaginationHelper.Paginate(new PaginationRequest<PayItemConstants.PayItemDto>()
            {
                CurrentPage = page,
                ModelList = result,
                SearchQuery = searchQuery
            });

            return View(paginatedModel);
        }

        [Route("Add")]
        public IActionResult AddPayItem()
        {
            PrepareViewBagForDropdowns();
            return View(new CreatePayItemDto() { FormulaValidFromDate = DateTime.Now });
        }

        [HttpPost("add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPayItem(CreatePayItemDto model)
        {

            if (model.DataType == global::PayrollSystem.Domain.Entities.PayItem.PayItem.PayItemDataType.Formula)
            {
                if (string.IsNullOrWhiteSpace(model.Formula))
                {
                    ModelState.AddModelError(nameof(model.Formula), "در صورت انتخاب نوع داده «فرمول»، وارد کردن فرمول الزامی است");
                }

                if (!model.FormulaValidFromDate.HasValue)
                    ModelState.AddModelError(nameof(model.FormulaValidFromDate), "تاریخ شروع اعتبار فرمول الزامی است");
            }

            if (!ModelState.IsValid)
            {
                ShowNotification(ApplicationMessages.MalformedInput, "", ApplicationMessagesIcon.ErrorIcon);

                PrepareViewBagForDropdowns();
                return View(model);
            }

            try
            {
                var result = await _payItemService.CreatePayItemAsync(model);
                if (result == null)
                {
                    ShowNotification("خطا در ایجاد عامل حقوقی، لطفا مجددا امتحان نمایید.","",ApplicationMessagesIcon.ErrorIcon);
                    return View(model);
                }

                ShowNotification($"عامل حقوقی «{model.Name}» با موفقیت ایجاد شد");
                return RedirectToAction("AllPayItems");
            }
            catch (InvalidOperationException ex)
            {
                ShowNotification(ex.Message,"",ApplicationMessagesIcon.ErrorIcon);
                _logger.LogError(ex.Message);
            }
            catch (ArgumentException ex)
            {
                ShowNotification(ex.Message, "", ApplicationMessagesIcon.ErrorIcon);
                _logger.LogError(ex.Message);
            }

            PrepareViewBagForDropdowns();
            return View(model);
        }

        // ══════════════════════════════════════════════════════════════
        // ویرایش — روتر مرکزی (تشخیص سیستمی vs سفارشی)
        // ══════════════════════════════════════════════════════════════

        [HttpGet("Update/{id:long}")]
        public async Task<IActionResult> UpdatePayItem(long id)
        {
            var item = await _payItemService.GetPayItemByIdAsync(id);
            if (item is null)
            {
                ShowNotification(ApplicationMessages.NotFound, "", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction(nameof(AllPayItems));
            }

            PrepareViewBagForDropdowns();

            // عوامل سیستمی → View مخصوص
            if (!item.IsCustom)
                return View("UpdateSystemPayItem", BuildSystemEditDto(item));

            // عوامل سفارشی → View قبلی
            var formula = await _payItemService.GetActiveFormulaAsync((long)item.Id!);
            return View(new UpdatePayItemDto
            {
                Id = (long)item.Id!,
                Name = item.Name,
                SystemCode = item.SystemCode,
                Type = item.Type,
                DataType = item.DataType,
                IsInsured = item.IsInsured,
                IsTaxable = item.IsTaxable,
                IsEditable = item.IsEditable,
                IsActive = item.IsActive,
                SortOrder = item.SortOrder,
                IsCustom = item.IsCustom,
                Formula = formula?.Formula,
                FormulaValidFromDate = formula?.ValidFromDate
            });
        }
        [HttpPost]
        [Route("update/{id:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePayItem(UpdatePayItemDto model)
        {
            var item = await _payItemService.GetPayItemByIdAsync(model.Id);
            if (item == null)
            {
                ModelState.AddModelError(nameof(model.Name),"اطلاعات وارد شده صحیح نیست، رکورد یافت نشد.");
            }

            if (model.DataType == global::PayrollSystem.Domain.Entities.PayItem.PayItem.PayItemDataType.Formula)
            {
                if (string.IsNullOrWhiteSpace(model.Formula))
                {
                    ModelState.AddModelError(nameof(model.Formula), "در صورت انتخاب نوع داده «فرمول»، وارد کردن فرمول الزامی است");
                }

                if (!model.FormulaValidFromDate.HasValue)
                    ModelState.AddModelError(nameof(model.FormulaValidFromDate), "تاریخ شروع اعتبار فرمول الزامی است");
            }

            if (!ModelState.IsValid)
            {
                ShowNotification(ApplicationMessages.MalformedInput, "", ApplicationMessagesIcon.ErrorIcon);

                PrepareViewBagForDropdowns();
                return View(model);
            }

            try
            {
                var result = await _payItemService.UpdatePayItemAsync(model);
                if (result == null)
                {
                    ShowNotification("خطا در ویرایش عامل حقوقی، لطفا مجددا امتحان نمایید.", "", ApplicationMessagesIcon.ErrorIcon);
                    return View(model);
                }

                ShowNotification($"عامل حقوقی «{model.Name}» با موفقیت ویرایش شد");
                return RedirectToAction("AllPayItems");
            }
            catch (InvalidOperationException ex)
            {
                ShowNotification(ex.Message, "", ApplicationMessagesIcon.ErrorIcon);
                _logger.LogError(ex.Message);
            }
            catch (ArgumentException ex)
            {
                ShowNotification(ex.Message, "", ApplicationMessagesIcon.ErrorIcon);
                _logger.LogError(ex.Message);
            }

            PrepareViewBagForDropdowns();
            return View(model);
        }


        // ── ویرایش عامل سیستمی ────────────────────────────────────────

        [HttpGet("UpdateSystem/{systemCode}")]
        public async Task<IActionResult> UpdateSystemPayItem(string systemCode)
        {
            if (!_payItemService.IsSystemItem(systemCode))
            {
                ShowNotification("این عامل از نوع سیستمی نیست", "", ApplicationMessagesIcon.ErrorIcon);
                return RedirectToAction(nameof(AllPayItems));
            }

            var item = await _payItemService.GetPayItemBySystemCodeAsync(systemCode);

            PrepareViewBagForDropdowns();
            return View(BuildSystemEditDto(item));
        }

        [HttpPost("UpdateSystem")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSystemPayItem(UpdateSystemPayItemDto model)
        {
            // اگر UseDefaultFormula = false، فرمول باید پر باشد
            if (!model.UseDefaultFormula && model.HasDataTypeFormula)
            {
                if (string.IsNullOrWhiteSpace(model.Formula))
                    ModelState.AddModelError(nameof(model.Formula), "فرمول را وارد کنید یا از فرمول پیش‌فرض استفاده کنید");

                if (!model.FormulaValidFromDate.HasValue)
                    ModelState.AddModelError(nameof(model.FormulaValidFromDate), "تاریخ شروع اعتبار فرمول الزامی است");
            }

            if (!ModelState.IsValid)
            {
                ShowNotification(ApplicationMessages.MalformedInput, "", ApplicationMessagesIcon.ErrorIcon);
                PrepareViewBagForDropdowns();
                // بازسازی فیلدهای readonly از constants
                var current = await _payItemService.GetPayItemBySystemCodeAsync(model.SystemCode);
                if (current != null) RebuildSystemDtoReadonlyFields(model, current);
                return View(model);
            }

            try
            {
                var result = await _payItemService.UpdateSystemPayItemAsync(model);
                ShowNotification($"عامل «{model.Name}» با موفقیت ویرایش شد");
                return RedirectToAction(nameof(AllPayItems));
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
            {
                ShowNotification(ex.Message, "", ApplicationMessagesIcon.ErrorIcon);
                _logger.LogError(ex, ex.Message);
            }

            PrepareViewBagForDropdowns();
            var fallback = await _payItemService.GetPayItemBySystemCodeAsync(model.SystemCode);
            if (fallback != null) RebuildSystemDtoReadonlyFields(model, fallback);
            return View(model);
        }


        // ── بازگشت فرمول به پیش‌فرض ─────────────────────────────────

        [HttpPost("ResetFormula/{systemCode}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetSystemFormula(string systemCode)
        {
            try
            {
                await _payItemService.ResetSystemPayItemFormulaAsync(systemCode);
                ShowNotification("فرمول به حالت پیش‌فرض بازگشت");
            }
            catch (Exception ex)
            {
                ShowNotification(ex.Message, "", ApplicationMessagesIcon.ErrorIcon);
            }

            return RedirectToAction("UpdateSystemPayItem", new { systemCode });
        }

        // =====================================================
        // متد کمکی برای آماده‌سازی Dropdown ها
        // =====================================================
        private UpdateSystemPayItemDto BuildSystemEditDto(PayItemConstants.PayItemDto? item)
        {
            if (item is null) return new();

            var defaultFormula = _payItemService.GetDefaultFormula(item.SystemCode);
            var activeFormula = item.Formulas.FirstOrDefault(f => f.IsActive);

            // آیا فرمول فعلی همان default است؟
            bool usingDefault = activeFormula == null ||
                activeFormula.Formula.Replace("  ‹پیش‌فرض›", "").Trim()
                    .Equals(defaultFormula?.Trim(), StringComparison.OrdinalIgnoreCase);

            return new UpdateSystemPayItemDto
            {
                SystemCode = item.SystemCode,
                Name = item.Name,
                TypeDisplay = item.Type.PayItemTypeToString(),
                DataTypeDisplay = item.DataType.PayItemDataTypeToString(),
                HasDataTypeFormula = item.DataType == global::PayrollSystem.Domain.Entities.PayItem.PayItem.PayItemDataType.Formula,
                DefaultFormula = defaultFormula,
                IsInsured = item.IsInsured,
                IsTaxable = item.IsTaxable,
                IsActive = item.IsActive,
                SortOrder = item.SortOrder,
                UseDefaultFormula = usingDefault,
                Formula = usingDefault ? null : activeFormula?.Formula,
                FormulaValidFromDate = usingDefault ? null : activeFormula?.ValidFromDate
            };
        }

        private static void RebuildSystemDtoReadonlyFields(
            UpdateSystemPayItemDto dto, PayItemConstants.PayItemDto source)
        {
            dto.Name = source.Name;
            dto.TypeDisplay = source.Type.PayItemTypeToString();
            dto.DataTypeDisplay = source.DataType.PayItemDataTypeToString();
            dto.HasDataTypeFormula = source.DataType == global::PayrollSystem.Domain.Entities.PayItem.PayItem.PayItemDataType.Formula;
        }

        private void ValidateFormulaFields(
            global::PayrollSystem.Domain.Entities.PayItem.PayItem.PayItemDataType dataType, string? formula, DateTime? validFrom)
        {
            if (dataType != global::PayrollSystem.Domain.Entities.PayItem.PayItem.PayItemDataType.Formula) return;

            if (string.IsNullOrWhiteSpace(formula))
                ModelState.AddModelError(nameof(formula), "در صورت انتخاب نوع داده «فرمول»، وارد کردن فرمول الزامی است");

            if (!validFrom.HasValue)
                ModelState.AddModelError(nameof(validFrom), "تاریخ شروع اعتبار فرمول الزامی است");
        }

        private void PrepareViewBagForDropdowns()
        {
            ViewBag.PayItemTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = ((int)global :: PayrollSystem.Domain.Entities.PayItem.PayItem.PayItemType.Earning).ToString(), Text = "مزایا" },
                new SelectListItem { Value = ((int)global :: PayrollSystem.Domain.Entities.PayItem.PayItem.PayItemType.Deduction).ToString(), Text = "کسورات" },
                new SelectListItem { Value = ((int)global :: PayrollSystem.Domain.Entities.PayItem.PayItem.PayItemType.Info).ToString(), Text = "اطلاعاتی" }
            };

            ViewBag.PayItemDataTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = ((int)global :: PayrollSystem.Domain.Entities.PayItem.PayItem.PayItemDataType.Decimal).ToString(), Text = "عدد اعشاری" },
                new SelectListItem { Value = ((int)global :: PayrollSystem.Domain.Entities.PayItem.PayItem.PayItemDataType.Boolean).ToString(), Text = "بله/خیر" },
                new SelectListItem { Value = ((int)global :: PayrollSystem.Domain.Entities.PayItem.PayItem.PayItemDataType.Formula).ToString(), Text = "فرمول" },
                new SelectListItem { Value = ((int)global :: PayrollSystem.Domain.Entities.PayItem.PayItem.PayItemDataType.UserInput).ToString(), Text = "ورودی کاربر" }
            };
        }
    }
}

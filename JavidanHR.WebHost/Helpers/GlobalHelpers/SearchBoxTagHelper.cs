using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace WebHost.Helpers.GlobalHelpers
{
    [HtmlTargetElement("searchBox")]
    public class SearchBoxTagHelper : TagHelper
    {
        private readonly IUrlHelperFactory _urlHelperFactory;

        public SearchBoxTagHelper(IUrlHelperFactory urlHelperFactory)
        {
            _urlHelperFactory = urlHelperFactory;
        }

        [ViewContext]
        public required ViewContext ViewContext { get; set; }

        /// <summary>
        /// مقدار فعلی جستجو (برای پر کردن ورودی)
        /// </summary>
        public string SearchQuery { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // گرفتن نام اکشن و کنترلر جاری از RouteData
            var routeData = ViewContext.RouteData;
            var currentController = routeData.Values["controller"]?.ToString() ?? "";
            var currentAction = routeData.Values["action"]?.ToString() ?? "";

            // ساخت Url به اکشن و کنترلر فعلی
            var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);
            var actionUrl = urlHelper.Action(currentAction, currentController);

            // تبدیل خود <search-form> به یک <form>
            output.TagName = "form";
            output.Attributes.SetAttribute("method", "get");
            output.Attributes.SetAttribute("action", actionUrl);
            output.Attributes.SetAttribute("class", "d-flex align-items-center gap-2 gap-lg-3");
            output.Attributes.SetAttribute("id", "filterForm");

            // حالا HTML بدنه فرم را می‌سازیم
            var divContainer = new TagBuilder("div");
            divContainer.AddCssClass("m-0");

            // دکمه "فیلتر"
            var filterButton = new TagBuilder("a");
            filterButton.AddCssClass("btn btn-sm btn-flex btn-secondary fw-bold");
            filterButton.Attributes.Add("href", "#");
            filterButton.Attributes.Add("data-kt-menu-trigger", "click");
            filterButton.Attributes.Add("data-kt-menu-placement", "bottom-end");

            // آیکون و متن داخل دکمه
            var filterIcon = new TagBuilder("i");
            filterIcon.AddCssClass("ki-outline ki-filter fs-6 text-muted me-1");
            filterButton.InnerHtml.AppendHtml(filterIcon);
            filterButton.InnerHtml.Append("فیلتر"); // متن ثابت

            // ساخت منوی بازشو
            var menuDiv = new TagBuilder("div");
            menuDiv.AddCssClass("menu menu-sub menu-sub-dropdown w-250px w-md-300px");
            menuDiv.Attributes.Add("data-kt-menu", "true");
            menuDiv.Attributes.Add("id", "searchDropdownMenu");

            // هدر منوی بازشو
            var headerDiv = new TagBuilder("div");
            headerDiv.AddCssClass("px-7 py-5");

            var headerTitle = new TagBuilder("div");
            headerTitle.AddCssClass("fs-5 text-gray-900 fw-bold");
            headerTitle.InnerHtml.Append("فیلتر");

            headerDiv.InnerHtml.AppendHtml(headerTitle);

            // جداکننده
            var separator = new TagBuilder("div");
            separator.AddCssClass("separator border-gray-200");

            // محتوای منو
            var formContent = new TagBuilder("div");
            formContent.AddCssClass("px-7 py-5");

            // ورودی جستجو
            var mb10Div = new TagBuilder("div");
            mb10Div.AddCssClass("mb-10");

            var inputSearch = new TagBuilder("input");
            inputSearch.Attributes.Add("type", "text");
            inputSearch.Attributes.Add("id", "searchInput");
            inputSearch.Attributes.Add("name", "searchQuery");
            inputSearch.Attributes.Add("value", SearchQuery);
            inputSearch.AddCssClass("form-control text-center");
            inputSearch.Attributes.Add("placeholder", "جستجو ..."); // متن ثابت
            inputSearch.Attributes.Add("style", "width: 250px; margin-left: 10px;");

            mb10Div.InnerHtml.AppendHtml(inputSearch);

            formContent.InnerHtml.AppendHtml(mb10Div);

            // بخش دکمه‌ها
            var actionsDiv = new TagBuilder("div");
            actionsDiv.AddCssClass("d-flex justify-content-end");

            // دکمه ریست
            var resetLink = new TagBuilder("a");
            resetLink.AddCssClass("btn btn-sm btn-light btn-active-light-primary me-2");
            // لینکی به همین اکشن و کنترلر جاری بدون پارامتر جستجو
            resetLink.Attributes.Add("href", actionUrl ?? "#");
            resetLink.InnerHtml.Append("ریست");

            // دکمه اعمال فیلتر
            var applyButton = new TagBuilder("button");
            applyButton.Attributes.Add("type", "submit");
            applyButton.AddCssClass("btn btn-sm btn-primary");
            applyButton.InnerHtml.Append("اعمال فیلتر");

            actionsDiv.InnerHtml.AppendHtml(resetLink);
            actionsDiv.InnerHtml.AppendHtml(applyButton);

            formContent.InnerHtml.AppendHtml(actionsDiv);

            // منوی بازشو را مونتاژ می‌کنیم
            menuDiv.InnerHtml.AppendHtml(headerDiv);
            menuDiv.InnerHtml.AppendHtml(separator);
            menuDiv.InnerHtml.AppendHtml(formContent);

            // divContainer: دکمه فیلتر + منوی بازشو
            divContainer.InnerHtml.AppendHtml(filterButton);
            divContainer.InnerHtml.AppendHtml(menuDiv);

            // در نهایت، همه چیز را به محتوای خروجی اضافه می‌کنیم
            output.Content.AppendHtml(divContainer);
        }
    }
}

using _0_Framework.Utilities.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace WebHost.Helpers.GlobalHelpers
{
    [HtmlTargetElement("pagination")]
    public class PaginationTagHelper : TagHelper
    {
        private readonly IUrlHelperFactory _urlHelperFactory;

        public PaginationTagHelper(IUrlHelperFactory urlHelperFactory)
        {
            _urlHelperFactory = urlHelperFactory;
        }

        [ViewContext]
        public required ViewContext ViewContext { get; set; }


        public required PaginationResultBase PageModel { get; set; }


        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // get current controller and action name from route data 
            var routeData = ViewContext.RouteData;
            var currentController = routeData.Values["controller"]?.ToString() ?? "";
            var currentAction = routeData.Values["action"]?.ToString() ?? "";

            // create link to current controller and action
            var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);

            //  main div
            output.TagName = "div";
            output.Attributes.SetAttribute("class", "pagination-container mt-4");

            var ulTag = new TagBuilder("ul");
            ulTag.AddCssClass("pagination justify-content-center");

            // previous page
            if (PageModel.CurrentPage > 1)
            {
                var liPrev = new TagBuilder("li");
                liPrev.AddCssClass("page-item");

                var aPrev = new TagBuilder("a");
                aPrev.AddCssClass("page-link");
                aPrev.InnerHtml.Append("صفحه قبل");
                // previous page link
                aPrev.Attributes.Add("href", urlHelper.Action(currentAction, currentController, new { page = PageModel.CurrentPage - 1, searchQuery = PageModel.SearchQuery }));

                liPrev.InnerHtml.AppendHtml(aPrev);
                ulTag.InnerHtml.AppendHtml(liPrev);
            }

            // page numbers
            if (PageModel.PagesToShow.Any())
            {
                foreach (var pageNumber in PageModel.PagesToShow)
                {
                    var li = new TagBuilder("li");
                    var a = new TagBuilder("a");
                    a.AddCssClass("page-link");

                    li.AddCssClass(pageNumber == PageModel.CurrentPage
                        ? "page-item active"
                        : "page-item");

                    a.Attributes.Add("href", urlHelper.Action(currentAction, currentController, new { page = pageNumber, searchQuery = PageModel.SearchQuery }));
                    a.InnerHtml.Append(pageNumber.ToString());

                    li.InnerHtml.AppendHtml(a);
                    ulTag.InnerHtml.AppendHtml(li);
                }
            }

            // next page
            if (PageModel.CurrentPage < PageModel.TotalPages)
            {
                var liNext = new TagBuilder("li");
                liNext.AddCssClass("page-item");

                var aNext = new TagBuilder("a");
                aNext.AddCssClass("page-link");
                aNext.InnerHtml.Append("صفحه بعد");

                // next page url
                aNext.Attributes.Add("href", urlHelper.Action(currentAction, currentController, new { page = PageModel.CurrentPage + 1, searchQuery = PageModel.SearchQuery }));

                liNext.InnerHtml.AppendHtml(aNext);
                ulTag.InnerHtml.AppendHtml(liNext);
            }

            output.Content.AppendHtml(ulTag);
        }
    }
}

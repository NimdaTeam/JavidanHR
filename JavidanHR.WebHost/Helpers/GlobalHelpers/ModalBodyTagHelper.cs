using System.Text;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JavidanHR.WebHost.Helpers.GlobalHelpers
{
    [HtmlTargetElement("Modal-Buttons")]
    public class ModalButtons : TagHelper
    {
        private readonly IHtmlGenerator _htmlGenerator;

        public ModalButtons(IHtmlGenerator htmlGenerator)
        {
            _htmlGenerator = htmlGenerator;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"<div class=""text-center"">");
            sb.AppendLine(@"    <div class=""btn btn-sm btn-danger"" data-bs-dismiss=""modal"">");
            sb.AppendLine(@"        <i class=""fa fa-close fs-1""></i> انصراف");
            sb.AppendLine(@"    </div>");
            sb.AppendLine(@"    <button type=""submit"" id=""submit-button"" class=""btn btn-success btn-sm"">");
            sb.AppendLine(@"        <i class=""fa fa-check fs-1""></i> ثبت");
            sb.AppendLine(@"    </button>");
            sb.AppendLine(@"</div>");

            output.TagName = "";

            output.Content.SetHtmlContent(sb.ToString());
        }
    }
}

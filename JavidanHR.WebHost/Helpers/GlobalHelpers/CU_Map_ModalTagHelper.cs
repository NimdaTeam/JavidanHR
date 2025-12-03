using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace WebHost.Helpers.GlobalHelpers
{
    [HtmlTargetElement("CU-Map-Modal")]
    public class CU_Map_ModalTagHelper : TagHelper
    {
        private readonly IUrlHelperFactory _urlHelperFactory;

        public CU_Map_ModalTagHelper(IUrlHelperFactory urlHelperFactory)
        {
            _urlHelperFactory = urlHelperFactory;
        }

        [ViewContext]
        public required ViewContext ViewContext { get; set; }

        public required string AddRoute { get; set; }
        public required string UpdateRoute { get; set; }
        public required bool AllowPolygon { get; set; }
        public required bool AllowPoint { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);

            var sb = new StringBuilder();


            sb.AppendLine(@"<div class=""modal fade"" id=""AddModal"" tabindex=""-1"" aria-hidden=""true"" style=""display: none;""></div>");
            sb.AppendLine(@"<div class=""modal fade"" id=""UpdateModal"" tabindex=""-1"" aria-hidden=""true"" style=""display: none;""></div>");


            sb.AppendLine("<script>");
            sb.AppendLine("    $(document).ready(function () {");


            sb.AppendLine("        function generateMapId() {");
            sb.AppendLine("            return 'map_' + Math.random().toString(36).substr(2,9);");
            sb.AppendLine("        }");

            sb.AppendLine("        $('.AddModalTrigger').click(function (event) {");
            sb.AppendLine("            event.preventDefault();");
            sb.AppendLine("            var mapId = generateMapId();");
            sb.AppendLine();
            sb.AppendLine("            $.ajax({");

            sb.AppendLine($"                url: '{AddRoute}?mapId=' + mapId,");
            sb.AppendLine("                type: 'GET',");
            sb.AppendLine("                success: function (result) {");
            sb.AppendLine("                    // Inject the partial view content into the modal body");
            sb.AppendLine("                    $('#AddModal').html(result);");
            sb.AppendLine();
            sb.AppendLine("                    // Show the modal");
            sb.AppendLine("                    $('#AddModal').modal('show');");
            sb.AppendLine();
            sb.AppendLine("                    initializeMap('#AddModal', '#' + mapId, null, { allowPoint: " + AllowPoint.ToString().ToLower() + ", allowPolygon: " + AllowPolygon.ToString().ToLower() + " });");
            sb.AppendLine("                },");
            sb.AppendLine("                error: function (xhr, status, error) {");
            sb.AppendLine("                    console.error(error);");
            sb.AppendLine("                }");
            sb.AppendLine("            });");
            sb.AppendLine("        });");

            sb.AppendLine("        $('.EditModalTrigger').click(function (event) {");
            sb.AppendLine("            event.preventDefault();");
            sb.AppendLine("            var id = $(this).data('id');");
            sb.AppendLine();
            sb.AppendLine("            var mapId = generateMapId();");
            sb.AppendLine();
            sb.AppendLine("            $.ajax({");

            sb.AppendLine($"                url: '{UpdateRoute}/' + id + '?mapId=' + mapId,");
            sb.AppendLine("                type: 'GET',");
            sb.AppendLine("                success: function (result) {");
            sb.AppendLine("                    $('#UpdateModal').html(result);");
            sb.AppendLine("                    $('#UpdateModal').modal('show');");
            sb.AppendLine();
            sb.AppendLine("                    var existingArea = $('#UpdateModal #polygon').val();");
            sb.AppendLine();
            sb.AppendLine("                    initializeMap('#UpdateModal', '#' + mapId, existingArea, { allowPoint: " + AllowPoint.ToString().ToLower() + ",  allowPolygon: " + AllowPolygon.ToString().ToLower() + " });");
            sb.AppendLine("                },");
            sb.AppendLine("                error: function (xhr, status, error) {");
            sb.AppendLine("                    console.error(error);");
            sb.AppendLine("                }");
            sb.AppendLine("            });");
            sb.AppendLine("        });");


            sb.AppendLine();
            sb.AppendLine("    });");


            sb.AppendLine("    document.addEventListener('DOMContentLoaded', function () {");
            sb.AppendLine("        const deleteButtons = document.querySelectorAll('.delete-button');");
            sb.AppendLine("        deleteButtons.forEach(button => {");
            sb.AppendLine("            button.addEventListener('click', function (event) {");
            sb.AppendLine("                event.preventDefault();");
            sb.AppendLine("                const deleteUrl = this.getAttribute('data-url');");
            sb.AppendLine("                Swal.fire({");
            sb.AppendLine("                    title: 'آیا از حذف این رکورد مطمئن هستید ؟',");
            sb.AppendLine("                    text: '',");
            sb.AppendLine("                    icon: 'warning',");
            sb.AppendLine("                    showCancelButton: true,");
            sb.AppendLine("                    confirmButtonColor: '#d33',");
            sb.AppendLine("                    cancelButtonColor: '#3085d6',");
            sb.AppendLine("                    confirmButtonText: 'بله ؛ حذف کن!',");
            sb.AppendLine("                    cancelButtonText: 'منصرف شدم'");
            sb.AppendLine("                }).then((result) => {");
            sb.AppendLine("                    if (result.isConfirmed) {");
            sb.AppendLine("                        window.location.href = deleteUrl;");
            sb.AppendLine("                    }");
            sb.AppendLine("                });");
            sb.AppendLine("            });");
            sb.AppendLine("        });");
            sb.AppendLine("    });");

            sb.AppendLine("</script>");


            output.TagName = "";
            output.Content.SetHtmlContent(sb.ToString());
        }
    }
}

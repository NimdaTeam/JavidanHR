using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace WebHost.Helpers.GlobalHelpers
{
    [HtmlTargetElement("Simple-CU-Modal")]
    public class Simple_CU_ModalTagHelper : TagHelper
    {
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly LinkGenerator _linkGenerator;
        private readonly IHttpContextAccessor _httpContext;

        public Simple_CU_ModalTagHelper(IUrlHelperFactory urlHelperFactory, LinkGenerator linkGenerator, IHttpContextAccessor httpContext)
        {
            _urlHelperFactory = urlHelperFactory;
            _linkGenerator = linkGenerator;
            _httpContext = httpContext;
        }


        public required string AddRoute { get; set; }
        public required string UpdateRoute { get; set; }

        [ViewContext]
        public required ViewContext ViewContext { get; set; }


        public override void Process(TagHelperContext context, TagHelperOutput output)
        {

            var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);

            // get current controller and action from view context
            var routeData = ViewContext.RouteData;
            var currentController = routeData.Values["controller"]?.ToString() ?? "";


            var sb = new StringBuilder();

            //modals 
            sb.AppendLine(@"<div style=""width:100% !important"" class=""modal fade"" id=""AddModal"" tabindex=""-1"" aria-hidden=""true"" style=""display: none;""></div>");
            sb.AppendLine(@"<div style=""width:100% !important"" class=""modal fade"" id=""UpdateModal"" tabindex=""-1"" aria-hidden=""true"" style=""display: none;""></div>");


            sb.AppendLine("<script>");
            sb.AppendLine("    $(document).ready(function () {");
            sb.AppendLine("        // Event listener for the button click");
            sb.AppendLine("        $('.AddModalTrigger').click(function (event) {");
            sb.AppendLine("            event.preventDefault();");
            sb.AppendLine("            $.ajax({");
            sb.AppendLine($"                url: '{AddRoute}',");
            sb.AppendLine("                type: 'GET',");
            sb.AppendLine("                success: function (result) {");
            sb.AppendLine("                    $('#AddModal').html(result);");
            sb.AppendLine("                    $('#AddModal').modal('show');");
            sb.AppendLine("                },");
            sb.AppendLine("                error: function (xhr, status, error) {");
            sb.AppendLine("                    console.error(error);");
            sb.AppendLine("                }");
            sb.AppendLine("            });");
            sb.AppendLine("        });");
            sb.AppendLine();
            sb.AppendLine("        $('.EditModalTrigger').click(function (event) {");
            sb.AppendLine("            event.preventDefault();");
            sb.AppendLine("            var id = $(this).data('id');");
            sb.AppendLine("            $.ajax({");
            sb.AppendLine($"                url: '{UpdateRoute}/' + id,");
            sb.AppendLine("                type: 'GET',");
            sb.AppendLine("                success: function (result) {");
            sb.AppendLine("                    $('#UpdateModal').html(result);");
            sb.AppendLine("                    $('#UpdateModal').modal('show');");
            sb.AppendLine("                },");
            sb.AppendLine("                error: function (xhr, status, error) {");
            sb.AppendLine("                    console.error(error);");
            sb.AppendLine("                }");
            sb.AppendLine("            });");
            sb.AppendLine("        });");
            sb.AppendLine("    });");
            sb.AppendLine();
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

    [HtmlTargetElement("All-Requests-Modal")]
    public class AllRequestsTagHelper : TagHelper
    {
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly LinkGenerator _linkGenerator;
        private readonly IHttpContextAccessor _httpContext;

        public AllRequestsTagHelper(IUrlHelperFactory urlHelperFactory, LinkGenerator linkGenerator, IHttpContextAccessor httpContext)
        {
            _urlHelperFactory = urlHelperFactory;
            _linkGenerator = linkGenerator;
            _httpContext = httpContext;
        }

        public required string AddRoute { get; set; }
        public required string UpdateRoute { get; set; }

        [ViewContext]
        public required ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);
            var routeData = ViewContext.RouteData;
            var currentController = routeData.Values["controller"]?.ToString() ?? "";

            var sb = new StringBuilder();

            // مودال‌ها
            sb.AppendLine(@"<div style=""width:100% !important"" class=""modal fade"" id=""AddModal"" tabindex=""-1"" aria-hidden=""true"" style=""display: none;""></div>");
            sb.AppendLine(@"<div style=""width:100% !important"" class=""modal fade"" id=""UpdateModal"" tabindex=""-1"" aria-hidden=""true"" style=""display: none;""></div>");

            // جاوااسکریپت
            sb.AppendLine("<script>");
            sb.AppendLine("$(document).ready(function () {");
            sb.AppendLine("    console.log('AllRequestsTagHelper initialized');");
            sb.AppendLine("    // تابع برای لود مودال ثبت درخواست");
            sb.AppendLine("    function loadAddRequestModal() {");
            sb.AppendLine("        console.log('Loading AddRequestModal');");
            sb.AppendLine("        $.ajax({");
            sb.AppendLine($"            url: '{System.Web.HttpUtility.JavaScriptStringEncode(AddRoute)}',");
            sb.AppendLine("            type: 'GET',");
            sb.AppendLine("            success: function (result) {");
            sb.AppendLine("                console.log('AddModal loaded successfully');");
            sb.AppendLine("                $('#AddModal').html(result);");
            sb.AppendLine("                $('#AddModal').modal('show');");
            sb.AppendLine("                bindAddRequestForm();");
            sb.AppendLine("            },");
            sb.AppendLine("            error: function (xhr, status, error) {");
            sb.AppendLine("                console.error('Error loading AddModal: ' + error);");
            sb.AppendLine("                toastr.error('خطا در لود فرم درخواست جدید.');");
            sb.AppendLine("            }");
            sb.AppendLine("        });");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    // تابع برای مدیریت ارسال فرم");
            sb.AppendLine("    function bindAddRequestForm() {");
            sb.AppendLine("        console.log('Binding AddRequestForm');");
            sb.AppendLine("        var form = $('#kt_modal_new_target_form');");
            sb.AppendLine("        if (form.length === 0) {");
            sb.AppendLine("            console.error('Form #kt_modal_new_target_form not found');");
            sb.AppendLine("            return;");
            sb.AppendLine("        }");
            sb.AppendLine("        form.on('submit', function (e) {");
            sb.AppendLine("            e.preventDefault();");
            sb.AppendLine("            console.log('Form submitted');");
            sb.AppendLine("            var formData = new FormData(this);");
            sb.AppendLine("            $.ajax({");
            sb.AppendLine($"                url: this.action,");
            sb.AppendLine("                type: 'POST',");
            sb.AppendLine("                data: formData,");
            sb.AppendLine("                contentType: false,");
            sb.AppendLine("                processData: false,");
            sb.AppendLine("                headers: {");
            sb.AppendLine("                    'RequestVerificationToken': $('input[name=\"__RequestVerificationToken\"]').val()");
            sb.AppendLine("                },");
            sb.AppendLine("                success: function (response) {");
            sb.AppendLine("                    console.log('AJAX response:', response);");
            sb.AppendLine("                    if (response.success) {");
            sb.AppendLine("                        $('#AddModal').modal('hide');");
            sb.AppendLine("                        toastr.success(response.message);");
            sb.AppendLine("                        setTimeout(function () { window.location.reload(); }, 2000);");
            sb.AppendLine("                    } else {");
            sb.AppendLine("                        var errorMessage = response.errors ? response.errors.join('<br>') : response.message;");
            sb.AppendLine("                        toastr.error(errorMessage);");
            sb.AppendLine("                        if (typeof selectedFiles !== 'undefined' && selectedFiles.length > 0) {");
            sb.AppendLine("                            toastr.warning('لطفاً فایل‌ها را دوباره انتخاب کنید.');");
            sb.AppendLine("                        }");
            sb.AppendLine("                    }");
            sb.AppendLine("                },");
            sb.AppendLine("                error: function (xhr, status, error) {");
            sb.AppendLine("                    console.error('Error submitting form: ' + error);");
            sb.AppendLine("                    toastr.error('خطای غیرمنتظره رخ داد.');");
            sb.AppendLine("                }");
            sb.AppendLine("            });");
            sb.AppendLine("        });");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    // مدیریت کلیک دکمه درخواست جدید");
            sb.AppendLine("    $('.AddModalTrigger').click(function (event) {");
            sb.AppendLine("        event.preventDefault();");
            sb.AppendLine("        console.log('AddModalTrigger clicked');");
            sb.AppendLine("        $.ajax({");
            sb.AppendLine("            url: '/User/CheckUserInfo',");
            sb.AppendLine("            type: 'GET',");
            sb.AppendLine("            dataType: 'json',");
            sb.AppendLine("            success: function (response) {");
            sb.AppendLine("                if (response.isComplete) {");
            sb.AppendLine("                    loadAddRequestModal();");
            sb.AppendLine("                } else {");
            sb.AppendLine("                    window.location.href = response.redirectUrl;");
            sb.AppendLine("                }");
            sb.AppendLine("            },");
            sb.AppendLine("            error: function (xhr, status, error) {");
            sb.AppendLine("                console.error('Error checking user info: ' + error);");
            sb.AppendLine("                toastr.error('خطا در بررسی اطلاعات کاربر.');");
            sb.AppendLine("            }");
            sb.AppendLine("        });");
            sb.AppendLine("    });");
            sb.AppendLine();
            sb.AppendLine("    // مدیریت کلیک دکمه ویرایش درخواست");
            sb.AppendLine("    $('.EditModalTrigger').click(function (event) {");
            sb.AppendLine("        event.preventDefault();");
            sb.AppendLine("        var id = $(this).data('id');");
            sb.AppendLine("        console.log('EditModalTrigger clicked, id: ' + id);");
            sb.AppendLine("        $.ajax({");
            sb.AppendLine($"            url: '{System.Web.HttpUtility.JavaScriptStringEncode(UpdateRoute)}/' + id,");
            sb.AppendLine("            type: 'GET',");
            sb.AppendLine("            success: function (result) {");
            sb.AppendLine("                $('#UpdateModal').html(result);");
            sb.AppendLine("                $('#UpdateModal').modal('show');");
            sb.AppendLine("                bindAddRequestForm();");
            sb.AppendLine("            },");
            sb.AppendLine("            error: function (xhr, status, error) {");
            sb.AppendLine("                console.error('Error loading UpdateModal: ' + error);");
            sb.AppendLine("                toastr.error('خطا در لود فرم ویرایش.');");
            sb.AppendLine("            }");
            sb.AppendLine("        });");
            sb.AppendLine("    });");
            sb.AppendLine();
            sb.AppendLine("    // مدیریت دکمه‌های حذف");
            sb.AppendLine("    $(document).on('click', '.delete-button', function (event) {");
            sb.AppendLine("        event.preventDefault();");
            sb.AppendLine("        var deleteUrl = $(this).attr('data-url');");
            sb.AppendLine("        if (!deleteUrl) {");
            sb.AppendLine("            console.error('data-url not found for delete-button');");
            sb.AppendLine("            toastr.error('خطا: URL حذف یافت نشد.');");
            sb.AppendLine("            return;");
            sb.AppendLine("        }");
            sb.AppendLine("        Swal.fire({");
            sb.AppendLine("            title: 'آیا از حذف این درخواست مطمئن هستید؟',");
            sb.AppendLine("            text: 'این عملیات قابل بازگشت نیست!',");
            sb.AppendLine("            icon: 'warning',");
            sb.AppendLine("            showCancelButton: true,");
            sb.AppendLine("            confirmButtonColor: '#d33',");
            sb.AppendLine("            cancelButtonColor: '#3085d6',");
            sb.AppendLine("            confirmButtonText: 'بله، حذف کن!',");
            sb.AppendLine("            cancelButtonText: 'انصراف'");
            sb.AppendLine("        }).then(function (result) {");
            sb.AppendLine("            if (result.isConfirmed) {");
            sb.AppendLine("                window.location.href = deleteUrl;");
            sb.AppendLine("            }");
            sb.AppendLine("        });");
            sb.AppendLine("    });");
            sb.AppendLine();
            sb.AppendLine("    // مدیریت دکمه‌های ارجاع");
            sb.AppendLine("    $(document).on('click', '.refer-button', function (event) {");
            sb.AppendLine("        event.preventDefault();");
            sb.AppendLine("        var referUrl = $(this).attr('data-url');");
            sb.AppendLine("        var status = $(this).attr('data-status') || 'ارجاع';");
            sb.AppendLine("        if (!referUrl) {");
            sb.AppendLine("            console.error('data-url not found for refer-button');");
            sb.AppendLine("            toastr.error('خطا: URL ارجاع یافت نشد.');");
            sb.AppendLine("            return;");
            sb.AppendLine("        }");
            sb.AppendLine("        Swal.fire({");
            sb.AppendLine("            title: 'آیا از تغییر وضعیت این درخواست به «' + status + '» اطمینان دارید؟',");
            sb.AppendLine("            text: '',");
            sb.AppendLine("            icon: 'warning',");
            sb.AppendLine("            showCancelButton: true,");
            sb.AppendLine("            confirmButtonColor: '#d33',");
            sb.AppendLine("            cancelButtonColor: '#3085d6',");
            sb.AppendLine("            confirmButtonText: 'بله، تغییر وضعیت!',");
            sb.AppendLine("            cancelButtonText: 'انصراف'");
            sb.AppendLine("        }).then(function (result) {");
            sb.AppendLine("            if (result.isConfirmed) {");
            sb.AppendLine("                window.location.href = referUrl;");
            sb.AppendLine("            }");
            sb.AppendLine("        });");
            sb.AppendLine("    });");
            sb.AppendLine("});");
            sb.AppendLine("</script>");

            output.TagName = "";
            output.Content.SetHtmlContent(sb.ToString());
        }
    }
}

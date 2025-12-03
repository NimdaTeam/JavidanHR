using AuthenticationSystem.Services.Repositories;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace WebHost.Helpers.GlobalHelpers
{
    [HtmlTargetElement(Attributes = "permissions")]
    public class PermissionTagHelper : TagHelper
    {
        // Update permission to be a list of integers
        public List<long> Permissions { get; set; } = new List<long>();

        private readonly IRoleRepository _roleService;

        public PermissionTagHelper(IRoleRepository roleService)
        {
            _roleService = roleService;
        }

        public override void Init(TagHelperContext context)
        {
            // Parse the "permission" attribute value into a list of integers
            if (context.AllAttributes["permissions"].Value is string permissionValue)
            {
                Permissions = permissionValue.Split(',')
                    .Select(p => long.TryParse(p, out var permission) ? permission : (long?)null)
                    .Where(p => p.HasValue)
                    .Select(p => p.Value)
                    .ToList();
            }
            base.Init(context);
        }

        public override async void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!_roleService.IsUserAuthenticated().Result)
            {
                output.SuppressOutput();
                return;
            }

            var userPermissions = await _roleService.GetLoggedInUserPermissions();

            // Suppress output if none of the specified permissions match the user's permissions
            if (!userPermissions.Any(up => Permissions.Contains(up)))
            {
                output.SuppressOutput();
                return;
            }

            base.Process(context, output);
        }
    }
}
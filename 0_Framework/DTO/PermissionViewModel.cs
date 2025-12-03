namespace _0_Framework.DTO
{
    public class PermissionViewModel
    {
        public long Permission { get; set; }

        public string Name { get; set; }

        public bool IsSelected { get; set; }

        public PermissionViewModel(string name , long permission,bool isSelected = false)
        {
            Name = name;
            Permission = permission;
            IsSelected = isSelected;
        }
    }

    public class PermissionGroupViewModel
    {
        public string ProjectName { get; set; }


        public List<PermissionViewModel> Permissions { get; set; }

        public PermissionGroupViewModel(string projectName , List<PermissionViewModel> permissions)
        {
            ProjectName = projectName;
            Permissions = permissions;
        }
    }

}

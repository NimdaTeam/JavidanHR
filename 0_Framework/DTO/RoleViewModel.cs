namespace _0_Framework.DTO
{
    public class RoleViewModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public int UserCount { get; set; }


    }

    public class EditUserRolesViewModel
    {

        public long Id { get; set; }

        public string Name { get; set; }

        public bool IsSelected { get; set; }

        public EditUserRolesViewModel(long id,string name,bool isSelected)
        {
            Id = id;
            Name = name;
            IsSelected = isSelected;
        }
    }
}

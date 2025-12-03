using _0_Framework.DTO;

namespace AuthenticationSystem.SystemPermissions
{
    public static class SystemPermissions
    {
        #region Users Permissions
        public const int UsersList = 1;
        public const int AddNewUser = 2;
        public const int UpdateUser = 3;
        public const int DeleteUser = 4;
        public const int ChangeUserPasswordFromAdmin = 5;
        public const int ActivateAndDeactivateUsers = 6;
        #endregion

        #region Roles Prermission
        public const int RolesList = 101;
        public const int AddNewRole = 102;
        public const int UpdateRole = 103;
        public const int DeleteRole = 104;
        #endregion

        #region Employees Management Permissions
        public const int EmployeesList = 201;
        public const int CreateEmployee = 202;
        public const int EditEmployeePersonalInfo = 203;     // مرحله ۱
        public const int EditEmployeeContactInfo = 204;     // مرحله ۲
        public const int EditEmployeeOrganizationalInfo = 205; // مرحله ۳
        public const int EditEmployeeFinancialInfo = 206;   // مرحله ۴
        public const int EditEmployeeAdditionalInfo = 207;  // مرحله ۵
        public const int ViewEmployeeDetails = 208;
        public const int PrintEmployeeProfile = 209;
        public const int DeleteEmployee = 210;
        #endregion

        public static List<PermissionViewModel> UserManagementPermissions = new List<PermissionViewModel>()
        {
            new PermissionViewModel("مشاهده لیست کاربران" , UsersList),
            new PermissionViewModel("اضافه کردن کاربر جدید",AddNewUser),
            new PermissionViewModel("ویرایش کاربر", UpdateUser),
            new PermissionViewModel("حذف کاربر",DeleteUser),
            new PermissionViewModel("تغییر گذرواژه کاربر (کاربر ادمین)",ChangeUserPasswordFromAdmin),
            new PermissionViewModel("فعالسازی/غیرفعالسازی حساب کاربری کاربران",ActivateAndDeactivateUsers),
        };

        public static List<PermissionViewModel> RoleManagementPermissions = new List<PermissionViewModel>()
        {
            new PermissionViewModel("مشاهده لیست سطوح دسترسی" , RolesList),
            new PermissionViewModel("اضافه کردن سطح دسترسی جدید",AddNewRole),
            new PermissionViewModel("ویرایش سطح دسترسی",UpdateRole),
            new PermissionViewModel("حذف سطح دسترسی" , DeleteRole)
        };

        public static List<PermissionViewModel> EmployeeManagementPermissions = new List<PermissionViewModel>()
        {
            new PermissionViewModel("مشاهده لیست کارمندان", EmployeesList),
            new PermissionViewModel("ایجاد کارمند جدید", CreateEmployee),
            new PermissionViewModel("ویرایش اطلاعات هویتی کارمند", EditEmployeePersonalInfo),
            new PermissionViewModel("ویرایش اطلاعات تماس کارمند", EditEmployeeContactInfo),
            new PermissionViewModel("ویرایش اطلاعات سازمانی کارمند", EditEmployeeOrganizationalInfo),
            new PermissionViewModel("ویرایش اطلاعات مالی کارمند", EditEmployeeFinancialInfo),
            new PermissionViewModel("ویرایش اطلاعات تکمیلی کارمند", EditEmployeeAdditionalInfo),
            new PermissionViewModel("مشاهده جزئیات کامل کارمند", ViewEmployeeDetails),
            new PermissionViewModel("چاپ پروفایل پرسنلی", PrintEmployeeProfile),
            new PermissionViewModel("حذف کارمند", DeleteEmployee),
        };


        public static List<PermissionGroupViewModel> AllSystemPermissions = new List<PermissionGroupViewModel>()
        {
            new PermissionGroupViewModel("مدیریت کاربران", UserManagementPermissions),
            new PermissionGroupViewModel("مدیریت سطوح دسترسی" , RoleManagementPermissions),
            new PermissionGroupViewModel("مدیریت کارمندان", EmployeeManagementPermissions),
        };
    }
}

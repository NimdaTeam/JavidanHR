using _0_Framework.DTO;

namespace AuthenticationSystem.SystemPermissions
{
    public static class SystemPermissions
    {
        public enum PermissionList
        {
            #region UserPermissions
            UsersList,
            AddNewUser,
            UpdateUser,
            DeleteUser,
            ChangeUserPasswordFromAdmin,
            ActivateAndDeactivateUsers,
            #endregion

            #region Roles Prermissions
            RolesList,
            AddNewRole,
            UpdateRole,
            DeleteRole,
            #endregion

            #region Hr Permission
            EmployeesList,
            CreateNewEmployee,
            EditEmployeePersonalInfo,
            EditEmployeeEducationInfo,
            EditEmployeeWorkExperienceInfo,
            EditEmployeeOrganizationalInfo,
            EditEmployeeFinancialInfo,
            ViewEmployeeDetails,
            PrintEmployeeProfile,
            DeleteEmployee,
            EditEmployeeAfterUserConfirmation,
            UnlockEmployeeDataForReEditByEmployee,
            #endregion

            #region Attendance Permissions
            AttendanceAllRecordsList,
            AttendanceAllEmployeesMonthlyReport,
            AttendanceEmployeeMonthlyReport,
            AttendanceLiveStatus,
            AttendanceEditRecord,
            AttendanceEditDateRecords,
            AttendanceAllManualRequestList,
            AttendanceApproveManualRequest,
            AttendanceRejectManualRequest,
            AttendanceDeleteManualRequest
            #endregion
        }


        public static List<PermissionViewModel> UserManagementPermissions = new List<PermissionViewModel>()
        {
            new PermissionViewModel("مشاهده لیست کاربران" ,(long)PermissionList.UsersList),
            new PermissionViewModel("اضافه کردن کاربر جدید",(long) PermissionList.AddNewUser),
            new PermissionViewModel("ویرایش کاربر", (long) PermissionList.UpdateUser),
            new PermissionViewModel("حذف کاربر",(long) PermissionList.DeleteUser),
            new PermissionViewModel("تغییر گذرواژه کاربر (کاربر ادمین)",(long) PermissionList.ChangeUserPasswordFromAdmin),
            new PermissionViewModel("فعالسازی/غیرفعالسازی حساب کاربری کاربران",(long) PermissionList.ActivateAndDeactivateUsers),
        };

        public static List<PermissionViewModel> RoleManagementPermissions = new List<PermissionViewModel>()
        {
            new PermissionViewModel("مشاهده لیست سطوح دسترسی" , (long) PermissionList.RolesList),
            new PermissionViewModel("اضافه کردن سطح دسترسی جدید",(long) PermissionList.AddNewRole),
            new PermissionViewModel("ویرایش سطح دسترسی",(long) PermissionList.UpdateRole),
            new PermissionViewModel("حذف سطح دسترسی" , (long) PermissionList.DeleteRole)
        };

        public static List<PermissionViewModel> EmployeeManagementPermissions = new List<PermissionViewModel>()
        {
            new PermissionViewModel("مشاهده لیست کارمندان", (long) PermissionList.EmployeesList),
            new PermissionViewModel("ایجاد کارمند جدید", (long) PermissionList.CreateNewEmployee),
            new PermissionViewModel("ویرایش کارمند بعد از تأیید نهایی اطلاعات(دسترسی مهم !!!)", (long) PermissionList.EditEmployeeAfterUserConfirmation),
            new PermissionViewModel("تغییر وضعیت تأیید اطلاعات کارمند جهت ویرایش مجدد توسط او", (long) PermissionList.UnlockEmployeeDataForReEditByEmployee),
            new PermissionViewModel("ویرایش اطلاعات هویتی و خانوادگی کارمند", (long) PermissionList.EditEmployeePersonalInfo),
            new PermissionViewModel("ویرایش اطلاعات تحصیلی کارمند", (long) PermissionList.EditEmployeeEducationInfo),
            new PermissionViewModel("ویرایش سوابق کاری کارمند", (long) PermissionList.EditEmployeeWorkExperienceInfo),
            new PermissionViewModel("ویرایش اطلاعات سازمانی کارمند", (long) PermissionList.EditEmployeeOrganizationalInfo),
            new PermissionViewModel("ویرایش اطلاعات مالی کارمند", (long) PermissionList.EditEmployeeFinancialInfo),
            new PermissionViewModel("مشاهده جزئیات کارمند", (long) PermissionList.ViewEmployeeDetails),
            new PermissionViewModel("چاپ پروفایل پرسنلی", (long) PermissionList.PrintEmployeeProfile),
            new PermissionViewModel("حذف کارمند", (long) PermissionList.DeleteEmployee),
        };

        public static List<PermissionViewModel> AttendanceManagementPermissions =
        [
            new PermissionViewModel("مشاهده لیست همه تردد ها", (long) PermissionList.AttendanceAllRecordsList),
            new PermissionViewModel("مشاهده گزارش تردد ماهانه تمام کارمندان", (long) PermissionList.AttendanceAllEmployeesMonthlyReport),
            new PermissionViewModel("گزارش تردد ماهانه کارمند", (long) PermissionList.AttendanceEmployeeMonthlyReport),
            new PermissionViewModel("مشاهده وضعیت فعلی حضور و غیاب کارمندان", (long) PermissionList.AttendanceLiveStatus),
            new PermissionViewModel("ویرایش یک تردد حضور و غیاب", (long) PermissionList.AttendanceEditRecord),
            new PermissionViewModel("ویرایش تردد های روزانه یک کارمند", (long) PermissionList.AttendanceEditDateRecords),
            new PermissionViewModel("مشاهده لیست همه درخواست های تردد دستی", (long) PermissionList.AttendanceAllManualRequestList),
            new PermissionViewModel("تأیید درخواست تردد دستی", (long) PermissionList.AttendanceApproveManualRequest),
            new PermissionViewModel("رد درخواست تردد دستی", (long) PermissionList.AttendanceRejectManualRequest),
            new PermissionViewModel("حذف درخواست تردد دستی در صورت عدم وضعیت تأیید/رد", (long) PermissionList.AttendanceDeleteManualRequest),
        ];

        public static List<PermissionGroupViewModel> AllSystemPermissions = new List<PermissionGroupViewModel>()
        {
            new PermissionGroupViewModel("مدیریت کاربران", UserManagementPermissions),
            new PermissionGroupViewModel("مدیریت سطوح دسترسی" , RoleManagementPermissions),
            new PermissionGroupViewModel("مدیریت کارمندان", EmployeeManagementPermissions),
            new PermissionGroupViewModel("مدیریت حضور و غیاب", AttendanceManagementPermissions),
        };
    }
}

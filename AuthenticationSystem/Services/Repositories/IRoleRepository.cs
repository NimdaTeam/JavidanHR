using _0_Framework.DTO;
using _0_Framework.GenericRepositoy.Interface;
using AuthenticationSystem.Domain.Role;
using AuthenticationSystem.Domain.RolePermission;
using AuthenticationSystem.Domain.User;
using Microsoft.AspNetCore.Http;

namespace AuthenticationSystem.Services.Repositories
{
    public interface IRoleRepository : IRepository<long, Roles>
    {
        #region Roles

        Task<bool> IsUserInRole(string phoneNumber, long roleId);

        Task<bool> AddRoleToUser(List<long> roles, long userId);

        Task<bool> RemoveRolesFromUser(long userId);

        Task<List<Roles>> GetUserRoles(long userId);


       Task<List<UserRoles>> GetUserRolesFromUserRolesTable(long userId);

        Task<List<RoleViewModel>> GetRolesForRolesGrid();

        Task<bool> IsRoleNameDuplicate(string name);

        Task<bool> IsRoleNameDuplicateForEdit(long roleId, string name);

        Task<Roles> CreateNewRole(string name,long creatorId);

        Task<OperationResult> UpdateRoleInformation(long roleId, string name, List<long> permissions);

        Task<bool> DoesRoleHasAnyPermissions(long roleId);

        Task<Roles?> GetDefaultRole();

        Task<OperationResult> DeleteAllRolePermissions(long roleId);

        Task<OperationResult> MigrateAllUsersToDefaultRole(long roleId);

        Task<bool> DoesRoleHasAnyUsers(long roleId);

        Task<List<EditUserRolesViewModel>> GetUserRolesForEditUser(long userId);
        #endregion

        #region Permissions

        Task<bool> IsUserAuthenticated();

        Task<List<RolePermissions>> GetRolePermissions(long roleId);

        Task<List<PermissionGroupViewModel>> GetPermissionsForRoleEdit(long roleId);

        Task<bool> AddPermissionsToRole(List<long> permissions, long roleId);

        Task<bool> RemovePermissionsFromRole(long roleId);

        Task<bool> CheckPermission(long permission, long userId);

        Task<List<RolePermissions>> GetUserPermissions(long userId);

        Task<List<long>> GetLoggedInUserPermissions();

        void AddUserPermissionsToHttpContext(List<long> permissions);

        Task<HttpContext> GetHttpContext();


        Task<bool> VerifyOTPCode(string otpCode, string phoneNumber);

        Task<OtpCodes?> GetOTPCodeByPhoneNumber(string phoneNumber);

        Task<OtpCodes?> GetOTPCode(string otp);

        void DeleteOTPCode(OtpCodes code);

        void DeleteOTPCode(long id);

        Task<string> GenerateOTPCode(Users user);

        Task<OtpCodes?> GetOTPCodeById(long id);

        void UpdateOTP(OtpCodes code);

        void DeactivateOTP(long id);
        
        void DeactivateOTP(string otp);

        #endregion
    }
}

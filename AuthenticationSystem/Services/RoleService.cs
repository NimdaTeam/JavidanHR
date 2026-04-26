using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using _0_Framework.DTO;
using _0_Framework.GenericRepositoy.Service;
using _0_Framework.Utilities.Generators;
using AuthenticationSystem.Domain.Role;
using AuthenticationSystem.Domain.RolePermission;
using AuthenticationSystem.Domain.User;
using AuthenticationSystem.Infrastructure;
using AuthenticationSystem.Services.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationSystem.Services
{
    public class RoleService : RepositoryService<long, Roles>, IRoleRepository
    {
        private readonly AuthenticationSystemContext _context;
        private readonly IUserRepository _userService;
        private readonly IHttpContextAccessor _httpContext;

        public RoleService(AuthenticationSystemContext context, IUserRepository userService, IHttpContextAccessor httpContext) : base(context)
        {
            _context = context;
            _userService = userService;
            _httpContext = httpContext;
        }

        #region Roles
        public async Task<bool> IsUserInRole(string phoneNumber, long roleId)
        {
            var user = await _userService.GetUserByPhoneNumber(phoneNumber);
            if (user == null)
            {
                return false;
            }

            return await _context.UserRoles
                .AnyAsync(x => x.UserId == user.Id && x.RoleId == roleId);
        }


        public async Task<bool> AddRoleToUser(List<long> roles, long userId)
        {
            try
            {
                foreach (var role in roles)
                {
                    _context.UserRoles.Add(new UserRoles()
                    {
                        UserId = userId,
                        RoleId = role
                    });
                }
                await SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                //TODO : Add exception to log
                return false;
            }
        }

        public async Task<bool> RemoveRolesFromUser(long userId)
        {
            try
            {
                var userRoles = await GetUserRolesFromUserRolesTable(userId);

                foreach (var role in userRoles)
                {
                    role.SoftDelete();
                    _context.UserRoles.Update(role);
                }

                await SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public async Task<List<Roles>> GetUserRoles(long userId)
        {
            var roleList = await _context.UserRoles
                .Where(x => x.UserId == userId)
                .Select(x => x.RoleId)
                .ToListAsync();

            var roles = new List<Roles>();


            foreach (var r in roleList)
            {
                var role = await GetAsNoTrackingAsync(r);
                if (role is null)
                    continue;

                roles.Add(role);
            }

            return roles;
        }

        public async Task<List<UserRoles>> GetUserRolesFromUserRolesTable(long userId)
        {
            return await _context.UserRoles.Where(x => x.UserId == userId).ToListAsync();
        }

        public async Task<List<RoleViewModel>> GetRolesForRolesGrid()
        {
            return await _context.Roles.Select(r => new RoleViewModel() { Name = r.Name, Id = r.Id, UserCount = _context.UserRoles.Count(x => x.RoleId == r.Id) }).ToListAsync();
        }

        public async Task<bool> IsRoleNameDuplicate(string name)
        {
            return await _context.Roles.AnyAsync(x => x.Name == name);
        }

        public async Task<bool> IsRoleNameDuplicateForEdit(long roleId, string name)
        {
            return await _context.Roles.AnyAsync(x => x.Name == name && x.Id != roleId);
        }

        public async Task<Roles> CreateNewRole(string name, long creatorId)
        {
            var role = new Roles()
            {
                Name = name,
                CreatorId = creatorId,
                CreationDate = DateTime.Now
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return role;
        }

        public async Task<OperationResult> UpdateRoleInformation(long roleId, string name, List<long> permissions)
        {
            try
            {
                var role = await GetAsync(roleId);
                if (role is null)
                {
                    return new OperationResult()
                    {
                        IsSuccessful = false
                    };
                }

                role.Name = name;

                if (await DoesRoleHasAnyPermissions(roleId))
                {
                   await RemovePermissionsFromRole(roleId);
                }

                await UpdateAsync(role);

                await AddPermissionsToRole(permissions, roleId);

                return new OperationResult()
                {
                    IsSuccessful = true
                };
            }
            catch (Exception e)
            {
                return new OperationResult()
                {
                    IsSuccessful = false,
                    Exception = e
                };
            }
        }

        public async Task<bool> DoesRoleHasAnyPermissions(long roleId)
        {
            return await _context.RolePermissions.AnyAsync(x => x.RoleId == roleId);
        }

        public async Task<Roles?> GetDefaultRole()
        {
            return await _context.Roles.FirstOrDefaultAsync(x => x.Name == "پیش فرض");
        }

        public async Task<OperationResult> DeleteAllRolePermissions(long roleId)
        {
            try
            {
                foreach (var p in _context.RolePermissions.Where(x => x.RoleId == roleId))
                {
                    p.SoftDelete();
                    _context.RolePermissions.Update(p);
                }
                await SaveChangesAsync();

                return new OperationResult()
                {
                    IsSuccessful = true
                };
            }
            catch (Exception e)
            {
                return new OperationResult()
                {
                    IsSuccessful = false,
                    Exception = e
                };
            }
        }

        public async Task<OperationResult> MigrateAllUsersToDefaultRole(long roleId)
        {
            try
            {
                var role = await GetDefaultRole();

                if (role == null)
                {
                    return new OperationResult()
                    {
                        IsSuccessful = false
                    };
                }

                foreach (var ur in _context.UserRoles
                             .Where(x => x.RoleId == roleId))
                {
                    ur.RoleId = role.Id;
                    _context.Update(ur);
                }

                await SaveChangesAsync();

                return new OperationResult()
                {
                    IsSuccessful = true
                };
            }
            catch (Exception e)
            {
                return new OperationResult()
                {
                    IsSuccessful = false,
                    Exception = e
                };
            }
        }

        public async Task<bool> DoesRoleHasAnyUsers(long roleId)
        {
            return await _context.UserRoles.AnyAsync(x => x.RoleId == roleId);
        }

        public async Task<List<EditUserRolesViewModel>> GetUserRolesForEditUser(long userId)
        {
            var AllRoles = await GetAllAsync();

            var userRoles = await GetUserRoles(userId);

            var resultList = new List<EditUserRolesViewModel>();

            foreach (var r in AllRoles)
            {
                resultList
                    .Add(new EditUserRolesViewModel(r.Id, r.Name, userRoles.Any(x => x.Id == r.Id)));
            }

            return resultList;
        }

        public Task<bool> IsUserAuthenticated()
        {
            return Task.FromResult(_httpContext.HttpContext.User.Identity is { IsAuthenticated: true });
        }

        #endregion


        #region Permissions
        public async Task<List<RolePermissions>> GetRolePermissions(long roleId)
        {
            return await _context.RolePermissions
                .Where(x => x.RoleId == roleId).ToListAsync();
        }

        public async Task<List<PermissionGroupViewModel>> GetPermissionsForRoleEdit(long roleId)
        {
            var allPermissions = SystemPermissions.SystemPermissions.AllSystemPermissions;
            var rolePermissions = await GetRolePermissions(roleId);

            var rolePermissionSet = new HashSet<string>(
                rolePermissions.Select(rp => rp.Permission.ToString()),
                StringComparer.OrdinalIgnoreCase // برای تطبیق دقیق
            );

            foreach (var group in allPermissions)
            {
                foreach (var permission in group.Permissions)
                {
                    permission.IsSelected = rolePermissionSet.Contains(permission.Permission.ToString());
                }
            }

            return allPermissions;
        }

        //public async Task<List<PermissionGroupViewModel>> GetPermissionsForRoleEdit(long roleId)
        //{
        //    var allPermissions = SystemPermissions.SystemPermissions.AllSystemPermissions;

        //    var rolePermissions = await GetRolePermissions(roleId);



        //    foreach (var pg in allPermissions)
        //    {
        //        foreach (var p in pg.Permissions)
        //        {
        //            if (rolePermissions.Any(x => x.Permission == p.Permission))
        //            {
        //                p.IsSelected = true;
        //            }
        //        }
        //    }

        //    return allPermissions;
        //}

        public async Task<bool> AddPermissionsToRole(List<long> permissions, long roleId)
        {
            try
            {
                if (await _context.RolePermissions.AnyAsync(x => x.RoleId == roleId))
                    await RemovePermissionsFromRole(roleId);


                foreach (var p in permissions)
                {
                    _context.RolePermissions.Add(new RolePermissions()
                    {
                        Permission = p,
                        RoleId = roleId
                    });
                }

                await SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public async Task<bool> RemovePermissionsFromRole(long roleId)
        {
            try
            {
                var permissions = await GetRolePermissions(roleId);

                foreach (var p in permissions)
                {
                    p.SoftDelete();
                    _context.RolePermissions.Update(p);
                }

                await SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public async Task<bool> CheckPermission(long permission, long userId)
        {
            var userPermissions = await GetUserPermissions(userId);

            return userPermissions.Any(x => x.Permission == permission);
        }

        public async Task<List<RolePermissions>> GetUserPermissions(long userId)
        {
            var roles = await GetUserRoles(userId);

            List<RolePermissions> permissions = new List<RolePermissions>();

            foreach (var r in roles)
            {
                var rolePermissions = await GetRolePermissions(r.Id);

                foreach (var p in rolePermissions)
                {
                    permissions.Add(p);
                }
            }

            return permissions;
        }

        public Task<List<long>> GetLoggedInUserPermissions()
        {
            var context = GetHttpContext();

            var permissionClaim = context.Result.User.FindFirst("permissions")?.Value;

            if (permissionClaim != null)
            {
                return Task.FromResult(JsonSerializer.Deserialize<List<long>>(permissionClaim));
            }

            return Task.FromResult(new List<long>());
        }

        public void AddUserPermissionsToHttpContext(List<long> permissions)
        {
            var context = GetHttpContext();

            string permissionsJson = JsonSerializer.Serialize(permissions);

            var claim = new Claim("permissions", permissionsJson);

            var identity = context.Result.User.Identity as ClaimsIdentity;

            identity.AddClaim(claim);
        }

        public Task<HttpContext> GetHttpContext()
        {
            return Task.FromResult(_httpContext.HttpContext);
        }

        public async Task<bool> VerifyOTPCode(string otpCode, string phoneNumber)
        {
            return await _context.OtpCodes
                .AnyAsync(x => x.PhoneNumber == phoneNumber
                          && x.HashedCode == otpCode
                          && !x.IsUsed);
        }

        public async Task<OtpCodes?> GetOTPCodeByPhoneNumber(string phoneNumber)
        {
            return await _context.OtpCodes.FirstOrDefaultAsync(x =>
                x.PhoneNumber == phoneNumber && !x.IsUsed && x.ExpireDate > DateTime.Now);
        }

        public async Task<OtpCodes?> GetOTPCode(string otp)
        {
            return await _context.OtpCodes.FirstOrDefaultAsync(x => x.HashedCode == otp);
        }

        public async void DeleteOTPCode(OtpCodes code)
        {
            _context.OtpCodes.Remove(code);
            await SaveChangesAsync();
        }

        public async void DeleteOTPCode(long id)
        {
            var otp = await _context.OtpCodes.FindAsync(id);
            if (otp != null)
            {
                _context.OtpCodes.Remove(otp);
                await SaveChangesAsync();
            }
        }

        public async Task<string> GenerateOTPCode(Users user)
        {
            var otp = OTPCodeGenerator.GenerateOTPCode();

            _context.OtpCodes.Add(new OtpCodes()
            {
                UserId = user.Id,
                IsUsed = false,
                HashedCode = otp,
                PhoneNumber = user.PhoneNumber,
                ExpireDate = DateTime.Now.AddSeconds(120)
            });

            await SaveChangesAsync();

            return otp;
        }

        public async Task<OtpCodes?> GetOTPCodeById(long id)
        {
            return await _context.OtpCodes.FindAsync(id);
        }

        public async void UpdateOTP(OtpCodes code)
        {
            _context.OtpCodes.Update(code);
            await SaveChangesAsync();
        }

        public async void DeactivateOTP(long id)
        {
            var otp = await GetOTPCodeById(id);
            if (otp != null)
            {
                otp.IsUsed = true;
                otp.UseDate = DateTime.Now;
                UpdateOTP(otp);
            }
        }

        public async void DeactivateOTP(string otp)
        {
            var code = await GetOTPCode(otp);
            if (code != null)
            {
                DeactivateOTP(code.Id);
            }
        }
        #endregion
    }
}

using System.Security.Claims;
using _0_Framework.DTO;
using _0_Framework.GenericRepositoy.Service;
using AuthenticationSystem.Domain.User;
using AuthenticationSystem.Infrastructure;
using AuthenticationSystem.Services.Repositories;
using AuthenticationSystem.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationSystem.Services
{
    public class UserService : RepositoryService<long, Users>, IUserRepository
    {
        private readonly AuthenticationSystemContext _context;

        public UserService(AuthenticationSystemContext context) : base(context)
        {
            _context = context;
        }


        public async Task<List<AllUsersViewModel>> GetAllUsersForGrid()
        {
            var users = await GetAllAsync();

            var resultList = new List<AllUsersViewModel>();

            foreach (var u in users)
            {
                var roles = _context.UserRoles
                    .Where(x => x.UserId == u.Id)
                    .Select(x => x.Roles.Name).ToList();


                resultList.Add(new AllUsersViewModel()
                {
                    Id = u.Id,
                    PhoneNumber = u.PhoneNumber,
                    Fullname = u.FullName,
                    Roles = String.Join(" - ", roles),
                    IsActive = u.IsActive
                });
            }

            return resultList;
        }

        public async Task<Users?> GetUserByPhoneNumber(ClaimsPrincipal? user)
        {
            if (user?.Identity is null)
                return null;

            if (string.IsNullOrEmpty(user.Identity.Name))
                return null;

            return await _context.Users.FirstOrDefaultAsync(x => x.PhoneNumber == user.Identity.Name);
        }

        public async Task<Users?> GetUserByPhoneNumber(string? phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return null;

            return await _context.Users.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber);
        }

        public async Task<bool> IsPhoneNumberDuplicate(string phoneNumber)
        {
            return await _context.Users.AnyAsync(x => x.PhoneNumber == phoneNumber);
        }

        public async Task<bool> IsPhoneNumberDuplicateForEdit(long userId, string phoneNumber)
        {
            return await _context.Users.AnyAsync(x => x.PhoneNumber == phoneNumber && x.Id != userId);
        }

        public async Task<long> CreateNewUser(Users user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user.Id;
        }

        public async Task<bool> IsUserInfoComplete(long userId)
        {
            var user = await GetAsNoTrackingAsync(userId);
            if (user == null)
            {
                throw new Exception("user not found !");
            }

            return (!string.IsNullOrWhiteSpace(user.FullName) && !string.IsNullOrWhiteSpace(user.NationalCode) &&
                    !string.IsNullOrWhiteSpace(user.FatherName));
        }

        public async Task<Users?> GetUserWithoutQueryFilter(long userId)
        {
            return await _context.Users.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == userId);
        }

        public async Task<string?> GetUserPasswordHash(long userId)
        {
            var user = await GetAsNoTrackingAsync(userId);

            return user?.PasswordHash;
        }

        public async Task<bool> ToggleUserActiveStatus(long userId, bool activeStatus)
        {
            try
            {
                var user = await GetAsync(userId);

                if (user is null)
                    return false;

                user.IsActive = activeStatus;

                //todo: execute in action
                await UpdateAsync(user);
                await SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public bool VerifyPassword(string userPasswordHash, string password)
        {
            return PasswordSecurity.PasswordHasher.VerifyPassword(password, userPasswordHash);
        }

        public async Task<bool> AddUserLoginHistory(UserLoginHistory history)
        {
            try
            {
                await _context.UserLoginHistories.AddAsync(history);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public async Task<int> CountRecentOtpCodesForPhoneNumber(string phoneNumber)
        {
            return await _context.OtpCodes
                .Where(o => o.PhoneNumber == phoneNumber && o.CreationDate > DateTime.UtcNow.AddMinutes(-10))
                .CountAsync();
        }

        public async Task<int> CountRecentOtpCodesForResetPassword(string phoneNumber)
        {
            return await _context.UserLoginHistories
                .Where(l => l.PhoneNumber == phoneNumber &&
                            l.LoginAt > DateTime.UtcNow.AddMinutes(-10) &&
                            l.Method == LoginMethod.ForgotPassword)
                .CountAsync();
        }

        public async Task<UserLoginHistory?> GetLastRequestedOtpSent(string phoneNumber)
        {
            return await _context.UserLoginHistories
                .Where(h => h.PhoneNumber == phoneNumber
                            && h.Method == LoginMethod.Otp
                            && h.FailureReason == "OtpRequested_Success"
                            && h.LoginAt > DateTime.UtcNow.AddSeconds(-120))
                .OrderByDescending(h => h.LoginAt)
                .FirstOrDefaultAsync();
        }
    }
}

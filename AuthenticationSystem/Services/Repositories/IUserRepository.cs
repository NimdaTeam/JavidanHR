using _0_Framework.DTO;
using _0_Framework.GenericRepositoy.Interface;
using AuthenticationSystem.Domain.User;

namespace AuthenticationSystem.Services.Repositories
{
    public interface IUserRepository : IRepository<long, Users>
    {

        //Users LoginUser(LoginViewModel model);

        Task<List<AllUsersViewModel>> GetAllUsersForGrid();

        Task<Users?> GetUserByPhoneNumber(string phoneNumber);

        Task<bool> IsPhoneNumberDuplicate(string phoneNumber);

        Task<bool> IsPhoneNumberDuplicateForEdit(long userId, string phoneNumber);

        Task<long> CreateNewUser(Users user);

        Task<bool> IsUserInfoComplete(long userId);

        Task<Users?> GetUserWithoutQueryFilter(long userId);

        Task<string?> GetUserPasswordHash(long userId);

        Task<bool> ToggleUserActiveStatus(long userId, bool activeStatus);

        bool VerifyPassword(string userPasswordHash, string password);

        Task<bool> AddUserLoginHistory(UserLoginHistory history);

        Task<int> CountRecentOtpCodesForPhoneNumber(string phoneNumber);
        Task<int> CountRecentOtpCodesForResetPassword(string phoneNumber);

        Task<UserLoginHistory?> GetLastRequestedOtpSent(string phoneNumber);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interface
{
     public interface IUserRepository : IBaseRepository<User>
    {
        
        Task<bool> GetExsitsEmailAsync(string email);
        Task<User?> GetUserByEmailAsync(string email);
        //tim kiem nguoi (dang)
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<List<User>> SearchUsersAsync(string keyword);
        Task<string?> GetFullNameByIdAsync(Guid id);
        Task<List<User>> GetAllUsersAsync();
        Task<List<User>> GetUsersByIdsAsync(List<Guid> userIds);
        Task<IEnumerable<User>> GetAdminsAsync();
        Task<User?> GetAdminByIdAsync(Guid adminId);
        Task<bool> ExistUsersAsync(Guid userId);
        Task<IEnumerable<(DateTime Date, int Count)>> GetUserTrendAsync(string timeRange);
        Task<IEnumerable<(string TrustCategory, int Count)>> GetUserTrustDistributionAsync();
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetUserBySuggestFriendAsync(Guid userId);
        Task<List<User>> GetAllActiveUsersAsync();
        Task<List<User>> GetUsersWithDetailsAsync();
    }
}

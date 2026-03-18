
namespace Infrastructure.Data.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        public override Task<bool> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .Where(u => u.Role == RoleEnum.User)
                .ToListAsync();
        }
     


        public async Task<bool> GetExsitsEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(x => x.Email == email);
        }

        public async Task<string?> GetFullNameByIdAsync(Guid id)
        {
            return await _context.Users.Where(x => x.Id == id).Select(x => x.FullName).FirstOrDefaultAsync();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
           return await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<List<User>> SearchUsersAsync(string keyword)
        {
            return await _context.Users
            .Where(u => u.FullName.Contains(keyword) || u.Email.Contains(keyword))
            .ToListAsync();
        }
        //Đếm số lượng bài đăng đi chung xe (RidePosts) của tất cả user và sắp xếp theo thứ tự giảm dần
        public async Task<List<User>> GetTopUsersByRidePostsAsync(int top)
        {
            return await _context.Users
                .OrderByDescending(u => u.RidePosts.Count)
                .Take(top)
                .ToListAsync();
        }
        //Đếm số lượng tham gia nhiều chuyến đi (Rides) nhất với vai trò tài xế của tất cả user và sắp xếp theo thứ tự giảm dần
        public async Task<List<User>> GetTopDriversByRidesAsync(int top)
        {
            return await _context.Users
                .OrderByDescending(u => u.DrivenRides.Count)
                .Take(top)
                .ToListAsync();
        }
        //Đếm số lượng tham gia nhiều chuyến đi (Rides) nhất với vai hành khách của tất cả user và sắp xếp theo thứ tự giảm dần
        public async Task<List<User>> GetTopPassengersByRidesAsync(int top)
        {
            return await _context.Users
                .OrderByDescending(u => u.RidesAsPassenger.Count)
                .Take(top)
                .ToListAsync();
        }
        //đếm số lượng bạn bè của các user (bao gồm cả gửi và nhận lời mời). và sắp xếp theo thứ tự giảm dần
        public async Task<List<User>> GetTopUsersByFriendsAsync(int top)
        {
            return await _context.Users
                .OrderByDescending(u => u.SentFriendRequests.Count + u.ReceivedFriendRequests.Count)
                .Take(top)
                .ToListAsync();
        }
        //đếm số lượng chiase bài viết (Shares) của tất cả user và sắp xếp theo thứ tự giảm dần
        public async Task<List<User>> GetTopUsersBySharesAsync(int top)
        {
            return await _context.Users
                .OrderByDescending(u => u.Shares.Count)
                .Take(top)
                .ToListAsync();
        }
        //đếm số lượng bình luận (Comments) của tất cả user và sắp xếp theo thứ tự giảm dần

        public async Task<List<User>> GetUsersByIdsAsync(List<Guid> userIds)
        {
            return await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();
        }

        public async Task<bool> ExistUsersAsync(Guid userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId);
        }

        public async Task<IEnumerable<User>> GetAdminsAsync()
        {
            return await _context.Users
                .Where(u => u.Role ==RoleEnum.Admin)
                .ToListAsync();
        }

        public async Task<User?> GetAdminByIdAsync(Guid adminId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == adminId && u.Role == RoleEnum.Admin);
        }

        public async Task<IEnumerable<(DateTime Date, int Count)>> GetUserTrendAsync(string timeRange)
        {
            var users = await _context.Users
         .Select(u => new { u.CreatedAt })
         .ToListAsync();

            var groupedData = timeRange switch
            {
                "day" => users
                    .GroupBy(u => u.CreatedAt.Date)
                    .Select(g => new { Date = g.Key, Count = g.Count() }),
                "week" => users
                    .GroupBy(u => new { u.CreatedAt.Year, Week = GetIsoWeekOfYear(u.CreatedAt) })
                    .Select(g => new { Date = GetFirstDayOfWeek(g.Key.Year, g.Key.Week), Count = g.Count() }),
                _ => users
                    .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
                    .Select(g => new { Date = new DateTime(g.Key.Year, g.Key.Month, 1), Count = g.Count() }),
            };

            var result = groupedData
                .OrderBy(g => g.Date)
                .Select(g => (g.Date, g.Count))
                .ToList();

            return result;
        }

        // Repository Layer
        public async Task<IEnumerable<(string TrustCategory, int Count)>> GetUserTrustDistributionAsync()
        {
            // 1. Lấy danh sách điểm uy tín của User
            var users = await _context.Users
                 .Where(u => u.Role == RoleEnum.User)
                 .Select(u => new { u.TrustScore })
                 .ToListAsync();


            // 2. Thực hiện đếm theo 3 nhóm yêu cầu
            // Nhóm Thấp: 0 - 30
            var lowCount = users.Count(u => (u.TrustScore) <= 30);

            var trustedCount = users.Count(u => u.TrustScore >= 40);
            var untrustedCount = users.Count(u => u.TrustScore < 40);


            // Nhóm Trung bình: 31 - 50
            var mediumCount = users.Count(u => (u.TrustScore) > 30 && (u.TrustScore) <= 50);

            // Nhóm Cao: 51 - 100
            var highCount = users.Count(u => (u.TrustScore) > 50);

            // 3. Trả về danh sách kết quả với Label tiếng Việt
            var result = new List<(string TrustCategory, int Count)>
    {
        ("Thấp (0 - 30)", lowCount),
        ("Trung bình (31 - 50)", mediumCount),
        ("Cao (51 - 100)", highCount)
    };

            return result;
        }
        private int GetIsoWeekOfYear(DateTime date)
        {
            var dayOfWeek = (int)date.DayOfWeek;
            var firstDayOfYear = new DateTime(date.Year, 1, 1);
            var daysOffset = DayOfWeek.Thursday - firstDayOfYear.DayOfWeek;

            var firstThursday = firstDayOfYear.AddDays(daysOffset);
            var calendarWeek = (int)Math.Floor((date - firstThursday).TotalDays / 7) + 1;
            return calendarWeek;
        }

        private DateTime GetFirstDayOfWeek(int year, int weekOfYear)
        {
            var jan1 = new DateTime(year, 1, 1);
            var daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            var firstThursday = jan1.AddDays(daysOffset);
            var firstDayOfWeek = firstThursday.AddDays((weekOfYear - 1) * 7);
            return firstDayOfWeek;
        }

        public async Task<User?> GetUserBySuggestFriendAsync(Guid userId)
        {
            return await _context.Users
                .Include(u => u.Posts)
                .Include(u => u.Likes)
                .Include(u => u.Comments)
                .Include(u => u.RidePosts)
                .Include(u => u.LocationUpdates)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<List<User>> GetAllActiveUsersAsync()
        {
            return await _context.Users
                .Where(u => u.Status == "Active")
                .ToListAsync();
        }

        public async Task<List<User>> GetUsersWithDetailsAsync()
        {
            return await _context.Users
                .Include(u => u.Posts)
                .Include(u => u.Likes)
                .Include(u => u.Comments)
                .Include(u => u.RidePosts)
                .Include(u => u.LocationUpdates)
                .Include(u => u.SentFriendRequests)
                .Include(u => u.ReceivedFriendRequests)
                .Where(u => u.Status == "Active")
                .ToListAsync();
        }
    }
}

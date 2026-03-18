using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repositories
{
    public class LikeRepository : BaseRepository<Like>, ILikeRepository
    {
        public LikeRepository(AppDbContext context) : base(context)
        {
        }

        public async Task AddRangeAsync(IEnumerable<Like> entities)
        {
            //viết logic thêm nhiều like vào db
            await _context.AddRangeAsync(entities);
        }

        public async Task<int> CountLikesByPostIdAsync(Guid postId)
        {
            return await _context.Likes
                .Where(l => l.PostId == postId && !l.IsDeleted && l.IsLike)
                .CountAsync();
        }

        public override Task<bool> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<Like?> GetLikeByPostIdAsync(Guid postId,Guid userId)
        {
            return await _context.Likes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
        }

        public async Task<List<Like>> GetLikesByPostIdAsync(Guid postId, int page, int pageSize)
        {
            return await _context.Likes
            .Where(l => l.PostId == postId && !l.IsDeleted && l.IsLike)
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(l => l.User) // Load thông tin User
            .ToListAsync();
        }
        public async Task<List<Like>> GetLikesByPostIdDeleteAsync(Guid postId)
        {
            return await _context.Likes
                .Where(l => l.PostId == postId && !l.IsDeleted)
                .ToListAsync();
        }

        public Task<int> GetLikeCountAsync(Guid userId)
        {
            return _context.Likes.CountAsync(l => l.UserId == userId);
        }

        public async Task<bool> CheckLike(Guid postId, Guid userId)
        {
            return await _context.Likes.AnyAsync(l => l.PostId == postId && l.UserId == userId);
        }


        public async Task<(List<Like>, Guid?)> GetLikesByPostIdWithCursorAsync(Guid postId, Guid? lastUserId, int pageSize)
        {
            var query = _context.Likes
               .Include(l => l.User)
                .Where(l => l.PostId == postId && l.User != null && !l.IsDeleted && l.IsLike)
               .OrderBy(l => l.UserId) // 📌 Sắp xếp theo UserId để dùng cursor
               .AsQueryable();

            if (lastUserId.HasValue)
            {
                query = query.Where(l => l.UserId.CompareTo(lastUserId.Value) > 0);
            }

            var likes = await query.Take(pageSize + 1).ToListAsync(); // 📌 Lấy thêm 1 để kiểm tra còn dữ liệu không

            // 📌 Nếu lấy đủ 2 người (pageSize), nextCursor = người thứ 2, nếu ít hơn thì nextCursor = null
            Guid? nextCursor = likes.Count > pageSize ? likes[pageSize - 1].UserId : null;

            return (likes.Take(pageSize).ToList(), nextCursor);
        }

        public async Task<IEnumerable<(DateTime Date, int Count)>> GetLikesOverTimeAsync(string timeRange)
        {
            var likes = await _context.Likes
         .Where(l => l.IsLike && !l.IsDeleted)
         .Select(l => new { l.CreatedAt })
         .ToListAsync();

            var groupedData = timeRange switch
            {
                "day" => likes
                    .GroupBy(l => l.CreatedAt.Date)
                    .Select(g => new { Date = g.Key, Count = g.Count() }),
                "week" => likes
                    .GroupBy(l => new { l.CreatedAt.Year, Week = GetIsoWeekOfYear(l.CreatedAt) })
                    .Select(g => new { Date = GetFirstDayOfWeek(g.Key.Year, g.Key.Week), Count = g.Count() }),
                _ => likes
                    .GroupBy(l => new { l.CreatedAt.Year, l.CreatedAt.Month })
                    .Select(g => new { Date = new DateTime(g.Key.Year, g.Key.Month, 1), Count = g.Count() }),
            };

            var result = groupedData
                .OrderBy(g => g.Date)
                .Select(g => (g.Date, g.Count))
                .ToList();

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

        public async Task<bool> IsPostLikedByUserAsync(Guid postId, Guid userId)
        {
            return await _context.Likes.AnyAsync(l => l.PostId == postId && l.UserId == userId && l.IsLike && !l.IsDeleted);
        }
    }
}

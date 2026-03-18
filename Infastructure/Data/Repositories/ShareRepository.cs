using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repositories
{
    public class ShareRepository : BaseRepository<Share>, IShareRepository
    {
        public ShareRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<int> CountPostShareAsync(Expression<Func<Share, bool>> predicate)
        {
            return await _context.Shares.CountAsync(predicate);
        }

        public override Task<bool> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }
        public async Task<List<Share>> GetSharesByPostIdAsync(Guid postId)
        {
            return await _context.Shares
                .Where(s => s.PostId == postId && !s.IsDeleted)
                .ToListAsync();
        }
        public async Task<List<Share>> GetSharedUsersByPostIdWithCursorAsync(Guid postId, Guid? lastUserId, int pageSize, CancellationToken cancellationToken)
        {

            const int PAGE_SIZE = 10; // 🔥 Giới hạn cố định 10 người
            pageSize = Math.Min(pageSize, PAGE_SIZE); // Đảm bảo không vượt quá 10

            var query = _context.Shares
                .Where(s => s.PostId == postId)
                .OrderByDescending(s => s.CreatedAt); // ⚠️ OrderByDescending trả về IOrderedQueryable

            // Nếu có LastUserId, lấy những user có CreatedAt nhỏ hơn
            if (lastUserId.HasValue)
            {
                var lastUserShare = await _context.Shares.FirstOrDefaultAsync(s => s.User.Id == lastUserId.Value);
                if (lastUserShare != null)
                {
                    query = query.Where(s => s.CreatedAt < lastUserShare.CreatedAt)
                                 .OrderByDescending(s => s.CreatedAt); // 🔥 Sắp xếp lại để giữ kiểu IOrderedQueryable
                }
            }

            // Thêm Include sau khi đã xử lý các điều kiện lọc
            return await query
                .Include(s => s.User) // Đặt Include ở đây
                .Take(pageSize) // Giới hạn tối đa 10 người
                .ToListAsync(cancellationToken);
        }
        public async Task<List<Share>> GetSharesByPostIdAsync(Guid postId, int page, int pageSize)
        {
            return await _context.Shares
            .Where(s => s.PostId == postId)
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(s => s.User) // Load thông tin User
            .ToListAsync();
        }
        public async Task<List<Share>> SearchSharesAsync(string keyword)
        {
            return await _context.Shares
              .Include(s => s.User)
              .Include(s => s.Post)
              .ThenInclude(p => p.User)
              .Where(s => (s.Content != null && s.Content.Contains(keyword)) ||
                            (s.User != null && s.User.FullName.Contains(keyword)))
              .ToListAsync();
        }
        public async Task<List<Post>> GetSharedPostAllDeleteAsync(Guid originalPostId)
        {
            return await _context.Posts
                .Where(p => p.OriginalPostId == originalPostId && !p.IsDeleted)
                .ToListAsync();
        }
        public Task<int> GetShareCountAsync(Guid userId)
        {
            return _context.Shares.CountAsync(s => s.UserId == userId);
        }

        public async Task<IEnumerable<(DateTime Date, int Count)>> GetSharesOverTimeAsync(string timeRange)
        {
            var shares = await _context.Shares
                .Where(s => !s.IsDeleted)
                .Select(s => new { s.CreatedAt })
                .ToListAsync();

            var groupedData = timeRange switch
            {
                "day" => shares
                    .GroupBy(s => s.CreatedAt.Date)
                    .Select(g => new { Date = g.Key, Count = g.Count() }),
                "week" => shares
                    .GroupBy(s => new { s.CreatedAt.Year, Week = GetIsoWeekOfYear(s.CreatedAt) })
                    .Select(g => new { Date = GetFirstDayOfWeek(g.Key.Year, g.Key.Week), Count = g.Count() }),
                _ => shares
                    .GroupBy(s => new { s.CreatedAt.Year, s.CreatedAt.Month })
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
    }
}
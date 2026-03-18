using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repositories
{
    public class AccommodationReviewRepository : BaseRepository<AccommodationReview>, IAccommodationReviewRepository
    {
        public AccommodationReviewRepository(AppDbContext context) : base(context)
        {
        }

        public override Task<bool> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<AccommodationReview?> GetByPostAndUserAsync(Guid accommodationPostId, Guid userId)
        {
            return await _context.AccommodationReviews
                .FirstOrDefaultAsync(r => r.AccommodationPostId == accommodationPostId && r.UserId == userId && r.IsDelete == false);
        }

        public async Task<List<int>?> GetRatingsByAccommodationPostIdAsync(Guid accommodationPostId)
        {
            return await _context.AccommodationReviews
                .Where(r => r.AccommodationPostId == accommodationPostId && r.IsDelete == false)
                .Select(r => r.Rating)
                .ToListAsync();
        }

        public async Task<List<AccommodationReview>> GetReviewsByAccommodationPostIdAsync(Guid accommodationPostId, Guid? lastAccommodationReviewId, int pageSize)
        {
            const int MAX_PAGE_SIZE = 50;
            pageSize = Math.Min(pageSize, MAX_PAGE_SIZE);

            var query = _context.AccommodationReviews
                // =======================================================
                // 💡 THÊM .Include(r => r.User) VÀO ĐÂY
                .Include(r => r.User) // Đảm bảo Entity User được tải cùng
                                      // =======================================================
                .Where(r => r.AccommodationPostId == accommodationPostId && r.IsDelete == false)
                .OrderByDescending(r => r.CreatedAt)
                .AsQueryable();

            if (lastAccommodationReviewId.HasValue)
            {
                // Thay FindAsync bằng FirstOrDefaultAsync có .AsNoTracking()
                // để không cần phải tracking entity này.
                var lastReview = await _context.AccommodationReviews
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(r => r.Id == lastAccommodationReviewId);

                if (lastReview != null)
                {
                    // Lọc theo CreatedAt của bài đánh giá cuối cùng
                    query = query.Where(r => r.CreatedAt < lastReview.CreatedAt);
                }
            }

            return await query
                .Take(pageSize)
                .ToListAsync();

        }
    }
}

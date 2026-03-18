using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repositories
{
    public class StudyMaterialRatingRepository : BaseRepository<StudyMaterialRating>, IStudyMaterialRatingRepository
    {
        public StudyMaterialRatingRepository(AppDbContext context) : base(context)
        {
        }

        public override Task<bool> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<StudyMaterialRating>> GetAllStudyMaterialRatingAsync(Guid? lastStudyMaterialRatingId, int pageSize, Guid StudyMaterialId)
        {
            const int MAX_PAGE_SIZE = 50;
            pageSize = Math.Min(pageSize, MAX_PAGE_SIZE);
            var query = _context.StudyMaterialRatings
                .Include(r => r.User)
                .Include(r => r.Material)
                .ThenInclude(m => m.User)
                .Where(r => r.MaterialId == StudyMaterialId)
                .OrderByDescending(r => r.CreatedAt)
                .AsQueryable();
            if (lastStudyMaterialRatingId.HasValue)
            {
                var lastRating = await _context.StudyMaterialRatings.AsNoTracking().FirstOrDefaultAsync(r => r.Id == lastStudyMaterialRatingId);
                if (lastRating != null)
                {
                    query = query.Where(r => r.CreatedAt < lastRating.CreatedAt);
                }
            }
            return await query
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<StudyMaterialRating?> GetByMaterialAndUserAsync(Guid materialId, Guid userId)
        {
            return await _context.StudyMaterialRatings
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.MaterialId == materialId && r.UserId == userId);
        }
    }
}

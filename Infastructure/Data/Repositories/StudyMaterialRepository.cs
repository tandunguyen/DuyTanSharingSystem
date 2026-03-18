using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repositories
{
    public class StudyMaterialRepository : BaseRepository<StudyMaterial>, IStudyMaterialRepository
    {
        public StudyMaterialRepository(AppDbContext context) : base(context)
        {
        }

        public override Task<bool> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }
        public async Task<long> GetTotalFileSizeByUserAsync(Guid userId)
        {
            return await _context.StudyMaterials
                .Where(m => m.UserId == userId && !m.IsDeleted)
                .SumAsync(m => m.TotalFileSize);
        }
        public async Task<List<StudyMaterial>> GetAllStudyMaterialAsync(Guid? lastPostId, int pageSize)
        {
            const int MAX_PAGE_SIZE = 50;
            pageSize = Math.Min(pageSize, MAX_PAGE_SIZE);
            var query = _context.StudyMaterials
                .Include(x => x.User)
                .Where(x => x.IsDeleted == false) // Chỉ lấy tài liệu không bị xóa
                .OrderByDescending(x => x.CreatedAt) // Sắp xếp theo thời gian mới nhất
                .AsQueryable();
            if (lastPostId.HasValue)
            {
                // Lấy tài liệu cuối cùng để tìm kiếm theo cursor (CreatedAt của tài liệu cuối cùng)
                var lastPost = await _context.StudyMaterials.AsNoTracking().FirstOrDefaultAsync(p => p.Id == lastPostId);
                if (lastPost != null)
                {
                    // Chỉ lấy các tài liệu cũ hơn tài liệu cuối cùng đã xem
                    query = query.Where(p => p.CreatedAt < lastPost.CreatedAt);
                }
            }
            return await query
                .Take(pageSize) // Giới hạn số lượng tài liệu
                .ToListAsync();
        }
    }
}

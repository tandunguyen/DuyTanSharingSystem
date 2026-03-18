// File: Infrastructure/Data/Repositories/AccommodationPostRepository.cs

using Domain.Entities;
using Domain.Interface;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using static Domain.Common.Enums;
using System.Linq.Expressions;

namespace Infrastructure.Data.Repositories
{
    // Giả định bạn có BaseRepository<T>
    public class AccommodationPostRepository : BaseRepository<AccommodationPost>, IAccommodationPostRepository
    {

        public AccommodationPostRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<List<AccommodationPost>> GetAllAccommodationPostAsync(Guid? lastPostId, int pageSize)
        {
            const int MAX_PAGE_SIZE = 50;
            pageSize = Math.Min(pageSize, MAX_PAGE_SIZE);

            var query = _context.AccommodationPosts
                .Include(p => p.User)
                .Where(x => x.Status == StatusAccommodationEnum.Available && x.IsDelete == false) // Chỉ lấy bài viết đang mở
                .OrderByDescending(x => x.CreatedAt) // Sắp xếp theo thời gian mới nhất
                .AsQueryable();

            if (lastPostId.HasValue)
            {
                // Lấy bài viết cuối cùng để tìm kiếm theo cursor (CreatedAt của bài cuối cùng)
                var lastPost = await _context.AccommodationPosts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == lastPostId);
                if (lastPost != null)
                {
                    // Chỉ lấy các bài đăng cũ hơn bài đăng cuối cùng đã xem
                    query = query.Where(p => p.CreatedAt < lastPost.CreatedAt);
                }
            }

            return await query
                .Take(pageSize) // Giới hạn số lượng bài viết
                .ToListAsync();
        }
        // ===================================================================
        // HÀM LẤY DANH SÁCH BÀI ĐĂNG (QUAN TRỌNG NHẤT)
        // ===================================================================
        /// <summary>
        /// Lấy danh sách bài đăng trọ theo bán kính và các tiêu chí lọc khác.
        /// Hàm này sử dụng NetTopologySuite và EF Core để tối ưu hóa truy vấn địa lý trên SQL.
        /// </summary>
        public async Task<List<AccommodationPost>> GetAccommodationPostsForSearchAsync(
            Point targetLocation,
            double radiusMeters,
            decimal? priceMin,
            decimal? priceMax,
            string? roomType,
            Guid? lastPostId,
            int pageSize)
        {
            // 1. Lọc cơ bản
            var query = _context.AccommodationPosts
                .Include(p => p.User) // Giả định cần User info để hiển thị FullName/Avatar
                .Where(p =>
                    p.Status == StatusAccommodationEnum.Available &&
                    (!priceMin.HasValue || p.Price >= priceMin.Value) &&
                    (!priceMax.HasValue || p.Price <= priceMax.Value) &&
                    (string.IsNullOrEmpty(roomType) || p.RoomType == roomType)
                )
                .AsQueryable();

            // 2. Lọc theo Cursor/Pagination
            if (lastPostId.HasValue)
            {
                // Giả định dùng Id cho cursor
                query = query.Where(p => p.Id > lastPostId.Value);
            }

            // 3. Lọc theo Khoảng cách (Nếu cột Location là kiểu Point của NTS)
            // LƯU Ý: Bạn cần đảm bảo đã ánh xạ Location (hoặc kết hợp Lat/Lng) thành kiểu Point/Geometry
            // Nếu bạn chỉ lưu Lat/Lng riêng biệt, bạn cần dùng hàm SQL thô để tính STDistance, 
            // hoặc tính toán trên client như ví dụ trước. 
            // Tôi giả định bạn đã có cách để tạo cột Location/Point trong DB.

            // Ví dụ tối ưu (sử dụng cột Location NTS trên DB)
            /* query = query
                .Where(p => p.Location.Distance(targetLocation) <= radiusMeters)
                .OrderBy(p => p.Location.Distance(targetLocation)) // Sắp xếp theo khoảng cách
                .ThenBy(p => p.Id);
            */

            // Ví dụ đơn giản hơn (chỉ dùng các điều kiện đã lọc):
            query = query
                .OrderBy(p => p.CreatedAt) // Sắp xếp mặc định
                .Take(pageSize);

            return await query.ToListAsync();
        }

        public override async Task<AccommodationPost?> GetByIdAsync(Guid id)
        {
            return await _context.AccommodationPosts
                .Include(p => p.User)  // Thêm include ở đây
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        // Các hàm khác kế thừa từ BaseRepository, ví dụ:
        public override Task<bool> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
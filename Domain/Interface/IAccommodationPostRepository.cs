// File: Domain/Interface/IAccommodationPostRepository.cs (Sửa đổi)

using Domain.Entities;
using System.Linq.Expressions;
using NetTopologySuite.Geometries;

namespace Domain.Interface
{
    public interface IAccommodationPostRepository : IBaseRepository<AccommodationPost>
    {
        // Hàm mới: Lấy danh sách mặc định (không lọc theo vị trí)
        Task<List<AccommodationPost>> GetAllAccommodationPostAsync(Guid? lastPostId, int pageSize);

        // Hàm cũ: Tìm kiếm chính (dùng khi có tọa độ, bộ lọc vị trí)
        Task<List<AccommodationPost>> GetAccommodationPostsForSearchAsync(
            Point targetLocation,
            double radiusMeters,
            decimal? priceMin,
            decimal? priceMax,
            string? roomType,
            Guid? lastPostId,
            int pageSize);
        //Get by id
    }
}
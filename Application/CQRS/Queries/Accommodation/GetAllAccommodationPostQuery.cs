// File: Application/CQRS/Queries/AccommodationPosts/GetAllAccommodationPostQuery.cs (Sửa đổi)

using Application.DTOs.Accommodation;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.CQRS.Queries.AccommodationPosts
{
    // Query để lấy danh sách bài đăng trọ mặc định/trang chủ (phân trang theo cursor)
    public class GetAllAccommodationPostQuery : IRequest<ResponseModel<GetAllAccommodationPostDto>>
    {
        public Guid? LastPostId { get; set; } // Cursor
        public int PageSize { get; set; } = 10;
    }
}
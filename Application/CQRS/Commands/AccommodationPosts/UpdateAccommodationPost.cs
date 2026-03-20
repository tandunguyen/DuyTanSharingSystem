// File: Application/CQRS/Commands/AccommodationPosts/UpdateAccommodationPost.cs

using Application.DTOs.Accommodation;
using System.ComponentModel.DataAnnotations;

namespace Application.CQRS.Commands.AccommodationPosts
{
    public class UpdateAccommodationPost : IRequest<ResponseModel<AccommodationPostDto>>
    {
        // ID của bài đăng cần cập nhật
        [Required]
        public required Guid Id { get; set; }

        // Các trường có thể cập nhật
        public string? Title { get; set; }
        public string? Content { get; set; }

        // Tọa độ mới do người dùng chọn trên bản đồ (Nếu có cập nhật vị trí)
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? MaxPeople { get; set; }
        public int? CurrentPeople { get; set; }

        // Địa chỉ hiển thị (thường là người dùng nhập hoặc FE dịch ngược thô)
        public string? AddressString { get; set; }

        [Range(0, 999999999.99)]
        public decimal? Price { get; set; }

        [Range(1, 1000)]
        public decimal? Area { get; set; } // m2

        public string? RoomType { get; set; }
        public string? Amenities { get; set; }
    }
}
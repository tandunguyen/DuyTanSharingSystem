using Application.DTOs.Accommodation;
using MediatR; // Giả định sử dụng MediatR
using static Domain.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.CQRS.Commands.AccommodationPosts
{
    // Kế thừa IRequest<T> để MediatR nhận diện là Command
    public class CreateAccommodationPostCommand : IRequest<ResponseModel<AccommodationPostDto>>
    {
        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        public string? Content { get; set; }

        [Required]
        [Range(-90, 90)] // Kiểm tra Vĩ độ hợp lệ
        public double Latitude { get; set; } // Bắt buộc, gửi từ FE khi người dùng click map

        [Required]
        [Range(-180, 180)] // Kiểm tra Kinh độ hợp lệ
        public double Longitude { get; set; } // Bắt buộc, gửi từ FE khi người dùng click map
        [Required]
        public string? AddressString { get; set; } = string.Empty;

        [Required]
        [Range(0, 999999999.99)]
        public decimal Price { get; set; }
        [Range(1, 10)]
        public int? MaxPeople { get; set; }
        [Range(0, 10)]
        public int? CurrentPeople { get; set; }

        [Range(1, 1000)]
        public decimal? Area { get; set; }

        [MaxLength(50)]
        public string? RoomType { get; set; }

        public string? Amenities { get; set; }

        // UserId sẽ được lấy từ UserContextService trong Handler, không phải từ request

        // Constructor (Tùy chọn, nếu bạn muốn dùng như RidePost)
        
        public CreateAccommodationPostCommand() { }
    }
}
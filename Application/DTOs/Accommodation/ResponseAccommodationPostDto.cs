
using Application.DTOs.AccommodationReview;

namespace Application.DTOs.Accommodation
{
    // DTO dùng để trả về thông tin chi tiết của một bài đăng phòng trọ.
    public class ResponseAccommodationPostDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserAvatar { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }

        public required string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public decimal Price { get; set; }
        public decimal? Area { get; set; } // m2
        public string? RoomType { get; set; }
        public string? Amenities { get; set; } // Tiện ích (có thể là chuỗi JSON)

        public StatusAccommodationEnum Status { get; set; }
        public bool IsVerified { get; set; }
        public string? CreatedAt { get; set; }
        public string? UpdatedAt { get; set; }

        // Thông tin tóm tắt về đánh giá
        public double AverageSafetyScore { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }

    }
}
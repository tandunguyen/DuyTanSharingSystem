namespace Application.DTOs.Accommodation
{
    // DTO tổng hợp kết quả tìm kiếm phòng trọ
    public class GetAllAccommodationPostDto
    {
        //public List<AccommodationPostDto> Posts { get; set; } = new();
        public List<LatLogAccommodation> LatLogAccommodations { get; set; } = new();
        public Guid? NextCursor { get; set; } // Dùng cho phân trang dạng Cursor
        public int TotalCount { get; set; } // Tổng số lượng bài đăng (không phân trang)

        // DTO tóm tắt dùng trong danh sách
        public class LatLogAccommodation
        {
            public Guid Id { get; set; }
            public Guid UserId { get; set; }
            public string Title { get; set; } = string.Empty;
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public required string Address { get; set; }
            public decimal Price { get; set; }
            public string Status { get; set; } = string.Empty;
        }
        public class AccommodationPostDto
        {
            public Guid Id { get; set; }
            public Guid UserId { get; set; }
            public double? AverageRating { get; set; }
            public required string UserName { get; set; }
            public string PhoneNumber { get; set; } = string.Empty;
            public string? ProfilePicture { get;  set; }
            public decimal TrustScore { get;  set; } = 0;
            public int? MaxPeople { get; set; }
            public int? CurrentPeople { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
            public required string Address { get; set; }

            public double Latitude { get; set; }
            public double Longitude { get; set; }

            public decimal Price { get; set; }
            public decimal? Area { get; set; }
            public string? RoomType { get; set; }

            public string Status { get; set; } = string.Empty; // Trạng thái dưới dạng chuỗi
            public string CreatedAt { get; set; } = string.Empty;

            public double DistanceKm { get; set; } // Khoảng cách từ điểm tìm kiếm đến trọ (thêm vào Service)
        }
    }
}
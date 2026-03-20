namespace Application.DTOs.Accommodation
{
    // DTO phản hồi yêu cầu tìm trọ ghép
    public class RoommateDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? AccommodationPostId { get; set; }

        public string? UserName { get; set; }
        public string? UserAvatar { get; set; }

        public string? TargetArea { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Description { get; set; } // Nội dung Gemini dùng để so khớp
        public bool IsActive { get; set; }
        public string? GenderPreference { get; set; }
        public string? CreatedAt { get; set; }

        // Thêm trường này nếu dùng cho gợi ý
        public double? MatchingScore { get; set; } // Điểm so khớp do Gemini tính toán
    }
}
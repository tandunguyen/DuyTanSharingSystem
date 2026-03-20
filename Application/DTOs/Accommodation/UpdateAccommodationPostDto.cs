// DTO này tương tự Create, nhưng có thể bỏ qua một số ràng buộc Required nếu dùng cho PATCH/PUT
namespace Application.DTOs.Accommodation
{
    public class UpdateAccommodationPostDto
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Address { get; set; }
        public decimal? Price { get; set; }
        public decimal? Area { get; set; }
        public string? RoomType { get; set; }
        public string? Amenities { get; set; }
        public int? Status { get; set; } // Có thể cập nhật Status (Available, Rented...)
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Accommodation
{
    public class AccommodationPostDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public required string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int? MaxPeople { get; set; }
        public int? CurrentPeople { get; set; }
        public decimal Price { get; set; }
        public decimal? Area { get; set; }
        public string? RoomType { get; set; }
        public StatusAccommodationEnum Status { get; set; }// Trạng thái dưới dạng chuỗi
        public string CreatedAt { get; set; } = string.Empty;
    }
}

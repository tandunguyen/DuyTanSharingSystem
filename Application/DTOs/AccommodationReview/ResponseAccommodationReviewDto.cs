using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.AccommodationReview
{
    public class ResponseAccommodationReviewDto
    {
     public class AccommodationReviewDto
        {
            public Guid Id { get; set; }
            public Guid AccommodationPostId { get; set; }
            public Guid UserId { get; set; }
            public string? UserName { get; set; }
            public string? UserAvatar { get; set; }
            public decimal TrustScore { get;  set; } = 0;
            public int Rating { get; set; }
            public string? Comment { get; set; }
            public int? SafetyScore { get; set; }
            public int? PriceScore { get; set; }
            public string? CreatedAt { get; set; }
            public bool IsApproved { get; set; }
        }
    }
}

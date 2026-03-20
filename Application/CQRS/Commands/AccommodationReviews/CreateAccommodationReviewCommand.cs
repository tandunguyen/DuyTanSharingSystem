using Application.DTOs.AccommodationReview;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CQRS.Commands.AccommodationReviews
{
    public class CreateAccommodationReviewCommand : IRequest<ResponseModel<ResponseAccommodationReviewDto.AccommodationReviewDto>>
    {
        public Guid AccommodationPostId { get; set; }
        public Guid UserId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public int? SafetyScore { get; set; }
        public int? PriceScore { get; set; }
        public CreateAccommodationReviewCommand(Guid accommodationPostId, Guid userId, int rating, string? comment, int? safetyScore = null, int? priceScore = null)
        {
            AccommodationPostId = accommodationPostId;
            UserId = userId;
            Rating = rating;
            Comment = comment;
            SafetyScore = safetyScore;
            PriceScore = priceScore;
        }
    }
}

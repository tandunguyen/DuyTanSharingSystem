// File: Application/CQRS/Commands/AccommodationReviews/UpdateAccommodationReviewCommand.cs

using Application.DTOs.AccommodationReview;
using MediatR;
using Domain.Common; // Giả định ResponseModel nằm ở đây

namespace Application.CQRS.Commands.AccommodationReviews
{
    public class UpdateAccommodationReviewCommand : IRequest<ResponseModel<ResponseAccommodationReviewDto.AccommodationReviewDto>>
    {
        // ID của bài đánh giá cần cập nhật
        public required Guid ReviewId { get; set; }

        // Dữ liệu mới
        public int Rating { get; set; }
        public string? Comment { get; set; }

        public UpdateAccommodationReviewCommand(Guid reviewId, int rating, string? comment)
        {
            ReviewId = reviewId;
            Rating = rating;
            Comment = comment;
        }
    }
}
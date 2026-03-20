// File: Application/CQRS/Commands/StudyMaterialReviews/UpdateStudyMaterialReviewCommand.cs

using Application.DTOs.StudyMaterial;
using MediatR;


namespace Application.CQRS.Commands.StudyMaterialReviews
{
    public class UpdateStudyMaterialReviewCommand : IRequest<ResponseModel<GetMaterialReviewDto>>
    {
        // ID của bài đánh giá cần cập nhật
        public required Guid ReviewId { get; set; }

        // Dữ liệu mới
        public int RatingLevel { get; set; }
        public string? Comment { get; set; }
        public bool IsHelpful { get; set; } // Thêm thuộc tính này từ CreateStudyMaterialReviewCommand

        public UpdateStudyMaterialReviewCommand(Guid reviewId, int ratingLevel, string? comment, bool isHelpful)
        {
            ReviewId = reviewId;
            RatingLevel = ratingLevel;
            Comment = comment;
            IsHelpful = isHelpful;
        }
    }
}
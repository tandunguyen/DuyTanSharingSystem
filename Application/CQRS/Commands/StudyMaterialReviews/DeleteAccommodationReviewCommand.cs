// File: Application/CQRS/Commands/StudyMaterialReviews/DeleteStudyMaterialReviewCommand.cs

using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.CQRS.Commands.StudyMaterialReviews
{
    public class DeleteStudyMaterialReviewCommand : IRequest<ResponseModel<bool>>
    {
        [Required]
        public required Guid Id { get; set; } // ID của đánh giá cần xóa
    }
}
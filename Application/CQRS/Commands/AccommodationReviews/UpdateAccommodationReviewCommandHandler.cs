// File: Application/CQRS/Commands/AccommodationReviews/UpdateAccommodationReviewCommandHandler.cs

using Application.DTOs.AccommodationReview;
using Application.Interface.ContextSerivce; // Giả định IUserContextService nằm ở đây
using Domain.Interface; // Giả định IUnitOfWork nằm ở đây
using MediatR;
using static Domain.Common.Helper; // Giả định FormatUtcToLocal nằm ở đây
using Domain.Common; // Giả định ResponseFactory nằm ở đây

namespace Application.CQRS.Commands.AccommodationReviews
{
    public class UpdateAccommodationReviewCommandHandler : IRequestHandler<UpdateAccommodationReviewCommand, ResponseModel<ResponseAccommodationReviewDto.AccommodationReviewDto>>
    {
        private readonly IUserContextService _userContextService;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateAccommodationReviewCommandHandler(IUnitOfWork unitOfWork, IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _userContextService = userContextService;
        }

        public async Task<ResponseModel<ResponseAccommodationReviewDto.AccommodationReviewDto>> Handle(UpdateAccommodationReviewCommand request, CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var userId = _userContextService.UserId();
                if (userId == Guid.Empty)
                    return ResponseFactory.Fail<ResponseAccommodationReviewDto.AccommodationReviewDto>("User not authenticated", 401);

                // 1. Tìm kiếm bài đánh giá theo ID và UserId
                var review = await _unitOfWork.AccommodationReviewRepository.GetByIdAsync(request.ReviewId);

                if (review == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ResponseFactory.Fail<ResponseAccommodationReviewDto.AccommodationReviewDto>("Review not found", 404);
                }

                // 2. Kiểm tra quyền sở hữu
                if (review.UserId != userId)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ResponseFactory.Fail<ResponseAccommodationReviewDto.AccommodationReviewDto>("Bạn không có quyền sửa đánh giá này.", 403);
                }

                // 3. Cập nhật Entity
                review.UpdateReview(request.Rating, request.Comment);

                // 4. Cập nhật và lưu thay đổi
                await  _unitOfWork.AccommodationReviewRepository.UpdateAsync(review); // Giả định Repository có phương thức Update
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // 5. Chuẩn bị DTO trả về
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);

                var reviewDto = new ResponseAccommodationReviewDto.AccommodationReviewDto
                {
                    Id = review.Id,
                    AccommodationPostId = review.AccommodationPostId,
                    UserId = review.UserId,
                    UserName = user?.FullName,
                    UserAvatar = user?.ProfilePicture,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedAt = FormatUtcToLocal(review.CreatedAt) // Hoặc UpdatedAt nếu bạn thêm vào entity
                };

                return ResponseFactory.Success(reviewDto, "Accommodation review updated successfully", 200);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ResponseFactory.Fail<ResponseAccommodationReviewDto.AccommodationReviewDto>($"Error updating accommodation review: {ex.Message}", 500);
            }
        }
    }
}
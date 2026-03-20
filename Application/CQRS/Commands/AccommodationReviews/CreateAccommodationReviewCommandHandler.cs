using Application.DTOs.AccommodationReview;

namespace Application.CQRS.Commands.AccommodationReviews
{
    public class CreateAccommodationReviewCommandHandler : IRequestHandler<CreateAccommodationReviewCommand, ResponseModel<ResponseAccommodationReviewDto.AccommodationReviewDto>>
    {
        private readonly IUserContextService _userContextService;
        private readonly IUnitOfWork _unitOfWork;
        public CreateAccommodationReviewCommandHandler(IUnitOfWork unitOfWork, IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _userContextService = userContextService;
        }
        public async Task<ResponseModel<ResponseAccommodationReviewDto.AccommodationReviewDto>> Handle(CreateAccommodationReviewCommand request, CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var userId = _userContextService.UserId();
                if (userId == Guid.Empty)
                    return ResponseFactory.Fail<ResponseAccommodationReviewDto.AccommodationReviewDto>("User not authenticated", 401);
                var existingReview = await _unitOfWork.AccommodationReviewRepository
                    .GetByPostAndUserAsync(request.AccommodationPostId, userId);
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
                if (user == null)
                    return ResponseFactory.Fail<ResponseAccommodationReviewDto.AccommodationReviewDto>("User not found", 404);
                if (user.TrustScore < 30 && user.TrustScore >= 0)
                    return ResponseFactory.Fail<ResponseAccommodationReviewDto.AccommodationReviewDto>("Để thao tác được chức năng này, bàn cần đạt ít nhất 31 điểm uy tín", 403);
                if (existingReview != null)
                {
                    // Lăn lại (rollback) transaction nếu đã có BeginTransactionAsync trước đó
                    await _unitOfWork.RollbackTransactionAsync();

                    return ResponseFactory.Fail<ResponseAccommodationReviewDto.AccommodationReviewDto>(
                        "Bạn đã đánh giá bài đăng này. Vui lòng sử dụng chức năng Cập nhật (Update) để sửa đánh giá.",
                        400); // Mã 400 (Bad Request) hoặc 409 (Conflict) đều phù hợp.
                }
                var review = new AccommodationReview
                (
                    accommodationPostId: request.AccommodationPostId,
                    userId: userId,
                    rating: request.Rating,
                    comment: request.Comment
                    );
                await _unitOfWork.AccommodationReviewRepository.AddAsync(review);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                var reviewDto = new ResponseAccommodationReviewDto.AccommodationReviewDto
                {
                    Id = review.Id,
                    AccommodationPostId = review.AccommodationPostId,
                    UserId = review.UserId,
                    UserName = user?.FullName,
                    UserAvatar = user?.ProfilePicture,
                    
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedAt = FormatUtcToLocal(review.CreatedAt)
                };
                return ResponseFactory.Success(reviewDto, "Accommodation review created successfully",201);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ResponseFactory.Fail<ResponseAccommodationReviewDto.AccommodationReviewDto>($"Error creating accommodation review: {ex.Message}",500);
            }
        }
    }
}

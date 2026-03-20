// File: Application/CQRS/Commands/StudyMaterialReviews/UpdateStudyMaterialReviewCommandHandler.cs

using Application.DTOs.StudyMaterial;


namespace Application.CQRS.Commands.StudyMaterialReviews
{
    public class UpdateStudyMaterialReviewCommandHandler : IRequestHandler<UpdateStudyMaterialReviewCommand, ResponseModel<GetMaterialReviewDto>>
    {
        private readonly IUserContextService _userContextService;
        private readonly IUnitOfWork _unitOfWork;

        // Giả định IUnitOfWork có property StudyMaterialRatingRepository

        public UpdateStudyMaterialReviewCommandHandler(IUnitOfWork unitOfWork, IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _userContextService = userContextService;
        }

        public async Task<ResponseModel<GetMaterialReviewDto>> Handle(UpdateStudyMaterialReviewCommand request, CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var userId = _userContextService.UserId();
                if (userId == Guid.Empty)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ResponseFactory.Fail<GetMaterialReviewDto>("Người dùng chưa được xác thực", 401);
                }

                // 1. Tìm kiếm bài đánh giá theo ID
                var review = await _unitOfWork.StudyMaterialRatingRepository.GetByIdAsync(request.ReviewId);

                if (review == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ResponseFactory.Fail<GetMaterialReviewDto>("Không tìm thấy đánh giá tài liệu học tập", 404);
                }

                // 2. Kiểm tra quyền sở hữu
                if (review.UserId != userId)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ResponseFactory.Fail<GetMaterialReviewDto>("Bạn không có quyền sửa đánh giá này.", 403);
                }

                // 3. Cập nhật Entity bằng phương thức Update (được định nghĩa trong StudyMaterialRating.cs)
                review.Update(request.RatingLevel, request.Comment, request.IsHelpful); // Sử dụng phương thức Update từ StudyMaterialRating.cs

                // 4. Cập nhật và lưu thay đổi
                await _unitOfWork.StudyMaterialRatingRepository.UpdateAsync(review); // Giả định Repository có phương thức Update
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // 5. Chuẩn bị DTO trả về (Lấy thông tin User tương tự như UpdateAccommodationReviewCommandHandler)
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);

                var reviewDto = new GetMaterialReviewDto
                {
                    Id = review.Id,
                    MaterialId = review.MaterialId,
                    UserId = review.UserId,
                    UserName = user?.FullName,
                    UserAvatarUrl = user?.ProfilePicture,
                    RatingLevel = review.RatingLevel,
                    Comment = review.Comment,
                    IsHelpful = review.IsHelpful,
                    CreatedAt = review.CreatedAt // Hoặc UpdatedAt nếu bạn thêm vào entity
                };

                return ResponseFactory.Success(reviewDto, "Đánh giá tài liệu học tập được cập nhật thành công", 200);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ResponseFactory.Fail<GetMaterialReviewDto>($"Lỗi dữ liệu đầu vào: {ex.Message}", 400);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ResponseFactory.Fail<GetMaterialReviewDto>($"Lỗi khi cập nhật đánh giá tài liệu học tập: {ex.Message}", 500);
            }
        }
    }
}
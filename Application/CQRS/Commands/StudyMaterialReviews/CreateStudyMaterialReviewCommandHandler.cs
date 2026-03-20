using Application.DTOs.StudyMaterial;

namespace Application.CQRS.Commands.StudyMaterialReviews
{
    // Cần đảm bảo rằng các interfaces/classes như IRequestHandler, ResponseModel, ResponseFactory,
    // IUnitOfWork, IUserContextService, StudyMaterialRating, GetMaterialReviewDto, v.v. đã được định nghĩa
    // trong các assembly/project tham chiếu.

    public class CreateStudyMaterialReviewCommandHandler : IRequestHandler<CreateStudyMaterialReviewCommand, ResponseModel<GetMaterialReviewDto>>
    {
        private readonly IUserContextService _userContextService;
        private readonly IUnitOfWork _unitOfWork;

        // Giả sử có một Repository cho StudyMaterialRating, hoặc sử dụng chung IUnitOfWork
        // Giả sử IUnitOfWork có property StudyMaterialRatingRepository

        public CreateStudyMaterialReviewCommandHandler(IUnitOfWork unitOfWork, IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _userContextService = userContextService;
        }

        public async Task<ResponseModel<GetMaterialReviewDto>> Handle(CreateStudyMaterialReviewCommand request, CancellationToken cancellationToken)
        {
            // Bắt đầu Transaction
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Lấy User ID từ Context Service
                // Sử dụng IUserContextService.UserId() giống như trong CreateAccommodationReviewCommandHandler
                var userId = _userContextService.UserId();

                if (userId == Guid.Empty)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ResponseFactory.Fail<GetMaterialReviewDto>("Người dùng chưa được xác thực", 401);
                }

                // KIỂM TRA ĐÁNH GIÁ ĐÃ TỒN TẠI CHƯA
                // Giả sử StudyMaterialRatingRepository có phương thức GetByMaterialAndUserAsync(materialId, userId)
                var existingReview = await _unitOfWork.StudyMaterialRatingRepository
                    .GetByMaterialAndUserAsync(request.MaterialId, userId);

                if (existingReview != null)
                {
                    await _unitOfWork.RollbackTransactionAsync();

                    return ResponseFactory.Fail<GetMaterialReviewDto>(
                        "Bạn đã đánh giá tài liệu này. Vui lòng sử dụng chức năng Cập nhật (Update) để sửa đánh giá.",
                        400);
                }

                // TẠO THỰC THỂ ĐÁNH GIÁ (ENTITY)
                var review = new StudyMaterialRating
                (
                    materialId: request.MaterialId,
                    userId: userId, // Lấy userId từ context thay vì từ request nếu đã xác thực
                    ratingLevel: request.RatingLevel,
                    comment: request.Comment,
                    isHelpful: request.IsHelpful ?? false
                );

                // THÊM VÀO REPOSITORY
                await _unitOfWork.StudyMaterialRatingRepository.AddAsync(review);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Lấy thông tin User để đưa vào DTO (giống như trong handler mẫu)
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);

                // TẠO DTO ĐỂ TRẢ VỀ
                var reviewDto = new GetMaterialReviewDto
                {
                    Id = review.Id,
                    MaterialId = review.MaterialId,
                    UserId = review.UserId,
                    UserName = user?.FullName,
                    UserAvatarUrl = user?.ProfilePicture,
                    TrustScore = user?.TrustScore,
                    RatingLevel = review.RatingLevel,
                    Comment = review.Comment,
                    IsHelpful = review.IsHelpful,
                    CreatedAt = review.CreatedAt // Giả sử GetMaterialReviewDto sử dụng DateTime hoặc cần FormatUtcToLocal
                                                 // Trong ví dụ này, tôi giữ nguyên CreateAt, bạn có thể cần FormatUtcToLocal nếu cần.
                };

                return ResponseFactory.Success(reviewDto, "Đánh giá tài liệu học tập được tạo thành công", 201);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                // Bắt lỗi ArgumentOutOfRangeException từ constructor của StudyMaterialRating (ví dụ: RatingLevel ngoài 1-5)
                return ResponseFactory.Fail<GetMaterialReviewDto>($"Lỗi dữ liệu đầu vào: {ex.Message}", 400);
            }
            catch (Exception ex)
            {
                // Rollback transaction nếu có lỗi
                await _unitOfWork.RollbackTransactionAsync();
                return ResponseFactory.Fail<GetMaterialReviewDto>($"Lỗi khi tạo đánh giá tài liệu học tập: {ex.Message}", 500);
            }
        }
    }
}
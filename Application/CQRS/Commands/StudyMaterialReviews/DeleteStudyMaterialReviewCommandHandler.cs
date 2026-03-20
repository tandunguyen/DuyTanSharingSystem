

namespace Application.CQRS.Commands.StudyMaterialReviews
{
    public class DeleteStudyMaterialReviewCommandHandler : IRequestHandler<DeleteStudyMaterialReviewCommand, ResponseModel<bool>>
    {
        private readonly IUserContextService _userContextService;
        private readonly IUnitOfWork _unitOfWork;

        // Giả định IUnitOfWork có property StudyMaterialRatingRepository

        public DeleteStudyMaterialReviewCommandHandler(IUnitOfWork unitOfWork, IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _userContextService = userContextService;
        }

        public async Task<ResponseModel<bool>> Handle(DeleteStudyMaterialReviewCommand request, CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var userId = _userContextService.UserId();
                if (userId == Guid.Empty)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ResponseFactory.Fail<bool>("Người dùng chưa được xác thực", 401);
                }

                // 1. Tìm đánh giá cần xóa
                var review = await _unitOfWork.StudyMaterialRatingRepository.GetByIdAsync(request.Id);

                if (review == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ResponseFactory.Fail<bool>("Không tìm thấy đánh giá tài liệu học tập", 404);
                }

                // 2. Kiểm tra quyền sở hữu
                if (review.UserId != userId)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ResponseFactory.Fail<bool>("Bạn không có quyền xóa đánh giá này", 403);
                }

                // 3. Xóa mềm (SoftDelete) và lưu DB
                review.SoftDelete(); // Sử dụng phương thức SoftDelete từ StudyMaterialRating.cs
                // Giả định StudyMaterialRatingRepository hỗ trợ UpdateAsync cho SoftDelete
                await _unitOfWork.StudyMaterialRatingRepository.UpdateAsync(review);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // 4. Trả về thành công
                return ResponseFactory.Success(
                    true,
                    "Đánh giá tài liệu học tập đã được xóa thành công",
                    200);
            }
            catch (Exception e)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ResponseFactory.Fail<bool>($"Lỗi khi xóa đánh giá tài liệu học tập: {e.Message}", 500);
            }
        }
    }
}


namespace Application.CQRS.Commands.AccommodationReviews
{
    public class DeleteAccommodationReviewCommandHandler : IRequestHandler<DeleteAccommodationReviewCommand, ResponseModel<bool>>
    {
        private readonly IUserContextService _userContextService;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteAccommodationReviewCommandHandler(IUnitOfWork unitOfWork, IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _userContextService = userContextService;
        }
        public async Task<ResponseModel<bool>> Handle(DeleteAccommodationReviewCommand request, CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var userId = _userContextService.UserId();
                if (userId == Guid.Empty)
                    return ResponseFactory.Fail<bool>("User not authenticated", 401);
                // 1. Tìm đánh giá cần xóa
                var review = await _unitOfWork.AccommodationReviewRepository.GetByIdAsync(request.Id);
                if (review == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ResponseFactory.Fail<bool>("Accommodation Review not found", 404);
                }
                // 2. Kiểm tra quyền sở hữu
                if (review.UserId != userId)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ResponseFactory.Fail<bool>("You do not have permission to delete this review", 403);
                }
                // 3. Xóa và lưu DB
                review.Delete();
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                // 4. Trả về thành công
                return ResponseFactory.Success(
                    true,
                    "Accommodation Review deleted successfully",
                    200);
            }
            catch (Exception e)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ResponseFactory.Fail<bool>(e.Message, 500);
            }
        }
    }
}

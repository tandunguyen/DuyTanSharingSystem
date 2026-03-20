// File: Application/CQRS/Commands/AccommodationPosts/DeleteAccommodationPostCommandHandler.cs

using Application.Interface.ContextSerivce;using Domain.Interface;
using MediatR;

namespace Application.CQRS.Commands.AccommodationPosts
{
    public class DeleteAccommodationPostCommandHandler : IRequestHandler<DeleteAccommodationPostCommand, ResponseModel<bool>>
    {
        private readonly IUserContextService _userContextService;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteAccommodationPostCommandHandler(IUnitOfWork unitOfWork, IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _userContextService = userContextService;
        }

        public async Task<ResponseModel<bool>> Handle(DeleteAccommodationPostCommand request, CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var userId = _userContextService.UserId();
                if (userId == Guid.Empty)
                    return ResponseFactory.Fail<bool>("User not authenticated", 401);

                // 1. Tìm bài đăng cần xóa
                var post = await _unitOfWork.AccommodationPostRepository.GetByIdAsync(request.Id);

                if (post == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ResponseFactory.Fail<bool>("Accommodation Post not found", 404);
                }

                // 2. Kiểm tra quyền sở hữu
                if (post.UserId != userId)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ResponseFactory.Fail<bool>("You do not have permission to delete this post", 403);
                }

                // 3. Xóa và lưu DB
                post.Delete();
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // 4. Trả về thành công
                return ResponseFactory.Success(
                    true,
                    "Accommodation Post deleted successfully",
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
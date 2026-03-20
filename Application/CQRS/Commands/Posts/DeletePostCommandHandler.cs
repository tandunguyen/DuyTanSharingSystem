
using Domain.Entities;

namespace Application.CQRS.Commands.Posts
{
    public class DeletePostCommandHandler : IRequestHandler<DeletePostCommand, ResponseModel<bool>>
    {
        private readonly IPostRepository _postRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContextService _userContextService;
        private readonly IRedisService _redisService;

        public DeletePostCommandHandler(IPostRepository postRepository, IUnitOfWork unitOfWork, IUserContextService userContextService, IRedisService redisService)
        {
            _postRepository = postRepository;
            _unitOfWork = unitOfWork;
            _userContextService = userContextService;
            _redisService = redisService;
        }

        public async Task<ResponseModel<bool>> Handle(DeletePostCommand request, CancellationToken cancellationToken)
        {
            var userId = _userContextService.UserId();

            var post = await _postRepository.GetByIdAsync(request.PostId);
           
            if (post == null )
            {
                return ResponseFactory.Fail<bool>("Không tìm thấy bài viết này", 404);
            }
            if (request.PostId != post.Id)
            {
                return ResponseFactory.Fail<bool>("Bạn không có quyền làm việc này", 404);
            }
            if ( userId != post.UserId)
            {
                return ResponseFactory.Fail<bool>("Bạn không có quyền làm việc này", 401);
            }
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                post.AdDelete();
                await _unitOfWork.PostRepository.UpdateAsync(post);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                if (request.redis_key != null)
                {
                    var key = $"{request.redis_key}";
                    await _redisService.RemoveAsync(key);
                }
                return ResponseFactory.Success(true, "Xóa bài viết thành công", 200);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ResponseFactory.Fail<bool>("Xóa không thành công", 500);
            }
        }
    }
}

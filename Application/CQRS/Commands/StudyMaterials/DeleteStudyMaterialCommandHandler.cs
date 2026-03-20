// File: Application/CQRS/Commands/StudyMaterial/DeleteStudyMaterialCommandHandler.cs



namespace Application.CQRS.Commands.StudyMaterial
{
    public class DeleteStudyMaterialCommandHandler : IRequestHandler<DeleteStudyMaterialCommand, ResponseModel<bool>>
    {
        private readonly IUserContextService _userContextService;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteStudyMaterialCommandHandler(IUnitOfWork unitOfWork, IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _userContextService = userContextService;
        }

        public async Task<ResponseModel<bool>> Handle(DeleteStudyMaterialCommand request, CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var userId = _userContextService.UserId();
                if (userId == Guid.Empty)
                    return ResponseFactory.Fail<bool>("User not authenticated", 401);

                // Tìm tài liệu cần xóa
                var material = await _unitOfWork.StudyMaterialRepository.GetByIdAsync(request.Id);

                if (material == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ResponseFactory.Fail<bool>("Study Material not found", 404);
                }

                // Kiểm tra quyền sở hữu
                if (material.UserId != userId)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ResponseFactory.Fail<bool>("You do not have permission to delete this material", 403);
                }

                // Xóa và lưu DB
                material.SoftDelete();
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Trả về thành công
                return ResponseFactory.Success(
                    true,
                    "Study Material deleted successfully",
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
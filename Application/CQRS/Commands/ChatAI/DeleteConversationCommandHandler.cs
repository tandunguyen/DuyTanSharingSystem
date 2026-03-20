using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CQRS.Commands.ChatAI
{
    public class DeleteConversationCommandHandler : IRequestHandler<DeleteConversationCommand, ResponseModel<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public DeleteConversationCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseModel<bool>> Handle(DeleteConversationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var conver = await _unitOfWork.AIConversationRepository.GetByIdAsync(request.ConversatiionId);
                if (conver == null)
                {
                    return ResponseFactory.Fail<bool>("Conversation dosen't exists", 400);
                }
                await _unitOfWork.AIConversationRepository.DeleteAsync(request.ConversatiionId);
                await _unitOfWork.SaveChangesAsync();
                return ResponseFactory.Success(true, "Delete conversation successfully", 200);
            }
            catch (Exception ex)
            {
                return ResponseFactory.Fail<bool>(ex.Message, 500);
            }
        }
    }
}

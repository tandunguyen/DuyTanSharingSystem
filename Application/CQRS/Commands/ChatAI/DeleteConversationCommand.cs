
namespace Application.CQRS.Commands.ChatAI
{
    public class DeleteConversationCommand : IRequest<ResponseModel<bool>>
    {
        public Guid ConversatiionId { get; set; }
    }
}

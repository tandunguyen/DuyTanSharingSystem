using Application.DTOs.Shares;


namespace Application.CQRS.Commands.Shares
{
    public class SharePostCommand : IRequest<ResponseModel<ResultSharePostDto>>
    {
        public Guid PostId { get; set; }
        public string? Content { get; set; }
        public ScopeEnum Privacy { get; set; }
        public string? redis_key { get; set; } = string.Empty;
    }
}

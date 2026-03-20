// File: Application/CQRS/Commands/AccommodationPosts/DeleteAccommodationPostCommand.cs

using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.CQRS.Commands.AccommodationPosts
{
    // Command này không cần trả về DTO, chỉ cần thông báo thành công
    public class DeleteAccommodationPostCommand : IRequest<ResponseModel<bool>>
    {
        [Required]
        public required Guid Id { get; set; }
    }
}
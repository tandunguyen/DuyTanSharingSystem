// File: Application/CQRS/Commands/StudyMaterial/DeleteStudyMaterialCommand.cs

using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.CQRS.Commands.StudyMaterial
{
    public class DeleteStudyMaterialCommand : IRequest<ResponseModel<bool>>
    {
        [Required]
        public required Guid Id { get; set; }
    }
}
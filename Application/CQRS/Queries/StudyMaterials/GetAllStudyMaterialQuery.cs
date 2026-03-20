

using Application.DTOs.StudyMaterial;

namespace Application.CQRS.Queries.StudyMaterials
{
    public class GetAllStudyMaterialQuery : IRequest<ResponseModel<GetAllStudyMaterialDto>>
    {
        public Guid? LastStudyMaterialId { get; set; } // Cursor
        public int PageSize { get; set; } = 10;
    }
}

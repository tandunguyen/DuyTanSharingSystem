using Application.DTOs.StudyMaterial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CQRS.Queries.StudyMaterials
{
    public class GetDetailStudyMaterialQuery : IRequest<ResponseModel<GetAllStudyMaterialDto.StudyMaterialDto>>
    {
        public Guid StudyMaterialId { get; set; }
    }
}

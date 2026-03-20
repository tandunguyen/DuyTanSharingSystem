using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CQRS.Commands.StudyMaterials
{
    public class CountDowloadCommand : IRequest<ResponseModel<int>>
    {
        public Guid StudyMaterialId { get; set; }
    }
}

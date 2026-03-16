using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interface
{
    public interface IStudyMaterialRepository : IBaseRepository<StudyMaterial>
    {
        Task <List<StudyMaterial>> GetAllStudyMaterialAsync(Guid? lastLastStudyMaterialIdId, int pageSize);
        Task<long> GetTotalFileSizeByUserAsync(Guid userId);
    }
}

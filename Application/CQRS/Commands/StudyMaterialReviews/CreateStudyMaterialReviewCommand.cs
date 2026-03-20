using Application.DTOs.StudyMaterial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CQRS.Commands.StudyMaterialReviews
{
    public class CreateStudyMaterialReviewCommand : IRequest<ResponseModel<GetMaterialReviewDto>>
    {
        public Guid MaterialId { get;  set; } // ID của tài liệu được đánh giá
        public Guid UserId { get;  set; }     // ID của người đánh giá
        public int RatingLevel { get;  set; } // Mức đánh giá (1-5)
        public string? Comment { get;  set; }
        public bool? IsHelpful { get;  set; } // Đánh giá chất lượng (Hữu ích/Cập nhật/Dễ hiểu)
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.StudyMaterial
{
    public class GetAllStudyMaterialReviewDto
    {
        public List<StudyMaterialReviewDto> Reviews { get; set; } = new();
        public Guid? NextCursor { get; set; } // Sử dụng Guid? để biểu thị có thể null
    }
    public class StudyMaterialReviewDto
    {
        public Guid Id { get; set; }
        public Guid MaterialId { get; set; } // ID của tài liệu được đánh giá
        public Guid UserId { get; set; }     // ID của người đánh giá
        public string? UserName { get; set; } // Tên đầy đủ của người đánh giá
        public string? UserAvatarUrl { get; set; } // URL ảnh đại diện của người đánh giá]
        public decimal? TrustScore { get; set; } // Điểm tin cậy của người đánh giá

        public int RatingLevel { get; set; } // Mức đánh giá (1-5)
        public string? Comment { get; set; }
        public bool IsHelpful { get; set; } // Đánh giá chất lượng (Hữu ích/Cập nhật/Dễ hiểu)
        public DateTime CreatedAt { get; set; }

    }
}

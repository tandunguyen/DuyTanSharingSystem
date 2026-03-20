// File: Application/DTOs/StudyMaterial/GetAllStudyMaterialDto.cs (Cập nhật toàn bộ class)

using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.StudyMaterial
{
    // DTO tổng hợp kết quả tìm kiếm tài liệu học tập
    public class GetAllStudyMaterialDto
    {
        public List<StudyMaterialDto> Materials { get; set; } = new();
        public Guid? NextCursor { get; set; } // Dùng cho phân trang dạng Cursor
        public int TotalCount { get; set; } // Tổng số tài liệu khớp với tìm kiếm

        // DTO chi tiết đầy đủ (điều chỉnh trường để khớp entity)
        public class StudyMaterialDto
        {
            public Guid Id { get; set; }
            public Guid UserId { get; set; }
            public decimal? AverageRating { get; set; }
            public required string UserName { get; set; }
            public string PhoneNumber { get; set; } = string.Empty;
            public string? ProfilePicture { get; set; }
            public decimal TrustScore { get; set; } = 0;
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty; // Thay cho Content
            public string Subject { get; set; } = string.Empty; // Môn học
            public string? Semester { get; set; } // Học kỳ (thay cho GradeLevel)
            public string? Faculty { get; set; } // Khoa/Bộ môn
            public List<string> FileUrls { get; set; } = new();
            // Liên kết file tải về
            public int DownloadCount { get; set; } = 0;
            public int ViewCount { get; set; } = 0;
            public string ApprovalStatus { get; set; } = string.Empty; // Trạng thái duyệt (chuỗi)
            public string CreatedAt { get; set; } = string.Empty;
            public long TotalFileSize
            {
                get; set;
            }
        }
    }
}
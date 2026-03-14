using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.Common.Enums;

namespace Domain.Entities
{
    // Sử dụng enum để quản lý trạng thái duyệt bài rõ ràng hơn


    public class StudyMaterial
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string Title { get; private set; }
        public string FileUrl { get; private set; }
        public string? Subject { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // CÁC THUỘC TÍNH BỔ SUNG TỪ YÊU CẦU MỚI:
        public string? Description { get; private set; }
        public string? Semester { get; private set; } // Học kỳ/Niên khóa
        public string? Faculty { get; private set; } // Khoa/Bộ môn
        public int DownloadCount { get; private set; } = 0;
        public int ViewCount { get; private set; } = 0;
        public StudyMaterialApprovalStatus ApprovalStatus { get; private set; } = StudyMaterialApprovalStatus.Pending;
        public Guid? ApprovedBy { get; private set; } // ID của Admin/Giảng viên duyệt
        public decimal AverageRating { get; private set; } = 0.00m;
        public long TotalFileSize { get; private set; }
        public bool IsDeleted { get; private set; } = false; // Đánh dấu xóa mềm
        // THUỘC TÍNH ĐIỀU HƯỚNG (Navigation Property)
        public ICollection<StudyMaterialRating> Ratings { get; private set; } = new List<StudyMaterialRating>();
        public User? User { get; private set; } // Giả sử có một User class

        public StudyMaterial(Guid userId, string title, string fileUrl, string? subject = null, string? description = null, string? semester = null, string? faculty = null)
        {
            if (userId == Guid.Empty) throw new ArgumentException("UserId cannot be empty.");
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.");
            if (string.IsNullOrWhiteSpace(fileUrl)) throw new ArgumentException("File URL is required.");

            Id = Guid.NewGuid();
            UserId = userId;
            Title = title;
            FileUrl = fileUrl;
            Subject = subject;
            Description = description;
            Semester = semester;
            Faculty = faculty;
            CreatedAt = DateTime.UtcNow;

            // Mặc định trạng thái là chờ duyệt
            ApprovalStatus = StudyMaterialApprovalStatus.Pending;
        }
        public void SetTotalFileSize(long fileSize)
        {
            if (fileSize < 0) throw new ArgumentException("File size cannot be negative.");
            TotalFileSize = fileSize;
        }

        /// <summary>
        /// Cập nhật thông tin tài liệu học tập.
        /// </summary>
        public void Update(string title, string fileUrl, string? subject = null, string? description = null, string? semester = null, string? faculty = null)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.");
            if (string.IsNullOrWhiteSpace(fileUrl)) throw new ArgumentException("File URL is required.");

            Title = title;
            FileUrl = fileUrl;
            Subject = subject;
            Description = description;
            Semester = semester;
            Faculty = faculty;
        }

        /// <summary>
        /// Cập nhật trạng thái duyệt tài liệu.
        /// </summary>
        public void UpdateApprovalStatus(StudyMaterialApprovalStatus status, Guid? approvedBy)
        {
            ApprovalStatus = status;
            ApprovedBy = approvedBy;
        }

        /// <summary>
        /// Cập nhật điểm đánh giá trung bình.
        /// </summary>
        public void UpdateAverageRating(decimal averageRating)
        {
            // Đảm bảo AverageRating không vượt quá 5.00
            AverageRating = Math.Min(averageRating, 5.00m);
        }

        /// <summary>
        /// Tăng lượt xem.
        /// </summary>
        public void IncrementViewCount()
        {
            ViewCount++;
        }

        /// <summary>
        /// Tăng lượt tải.
        /// </summary>
        public void IncrementDownloadCount()
        {
            DownloadCount++;
        }
        public void SoftDelete()
        {
            IsDeleted = true;
        }
    }
}
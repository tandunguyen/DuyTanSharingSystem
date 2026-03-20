// File: Application/CQRS/Commands/StudyMaterial/CreateStudyMaterialCommand.cs (Cập nhật toàn bộ class)

using Application.DTOs.StudyMaterial;
using MediatR;
using System.ComponentModel.DataAnnotations;
using static Application.DTOs.StudyMaterial.GetAllStudyMaterialDto;

namespace Application.CQRS.Commands.StudyMaterial
{
    public class CreateStudyMaterialCommand : IRequest<ResponseModel<StudyMaterialDto>>
    {
        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "File is required.")]
        public List<IFormFile>? Files { get; set; } = null!; // File tải lên bắt buộc

        [Required]
        [MaxLength(100)]
        public string Subject { get; set; } = string.Empty; // Môn học bắt buộc

        public string? Description { get; set; } // Mô tả (thay cho Content)

        [MaxLength(50)]
        public string? Semester { get; set; } // Học kỳ (thay cho GradeLevel)

        [MaxLength(100)]
        public string? Faculty { get; set; } // Khoa/Bộ môn
        [Required]
        public decimal TotalFileSize { get; set; }

        // Loại bỏ Price và Tags vì entity không có
    }
}
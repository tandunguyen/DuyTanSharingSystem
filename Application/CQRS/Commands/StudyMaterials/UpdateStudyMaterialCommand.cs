using System.ComponentModel.DataAnnotations;
using static Application.DTOs.StudyMaterial.GetAllStudyMaterialDto;
namespace Application.CQRS.Commands.StudyMaterial
{
    public class UpdateStudyMaterialCommand : IRequest<ResponseModel<StudyMaterialDto>>
    {
        [Required]
        public required Guid Id { get; set; }

        // Các trường có thể cập nhật (khớp entity)
        public string? Title { get; set; }
        public List<IFormFile>? FileUrls { get; set; }  // File mới (upload)
        public List<string>? ExistingFileUrls { get; set; }  // URL file cũ (giữ nguyên nếu không upload mới)
        public string? Subject { get; set; }
        public string? Description { get; set; }
        public string? Semester { get; set; }
        public string? Faculty { get; set; }

        // Loại bỏ Price và Tags
    }
}
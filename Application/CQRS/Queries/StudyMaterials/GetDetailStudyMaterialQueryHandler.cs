using Application.DTOs.StudyMaterial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CQRS.Queries.StudyMaterials
{
    public class GetDetailStudyMaterialQueryHandler : IRequestHandler<GetDetailStudyMaterialQuery, ResponseModel<GetAllStudyMaterialDto.StudyMaterialDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetDetailStudyMaterialQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseModel<GetAllStudyMaterialDto.StudyMaterialDto>> Handle(GetDetailStudyMaterialQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var material = await _unitOfWork.StudyMaterialRepository.GetByIdAsync(request.StudyMaterialId);
                if (material == null)
                {
                    return ResponseFactory.Fail<GetAllStudyMaterialDto.StudyMaterialDto>("Không tìm thấy tài liệu học tập.", 404);
                }
                var fileUrls = material.FileUrl?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(url => $"{Constaint.baseUrl.TrimEnd('/')}/{url.TrimStart('/')}")
                    .ToList() ?? new List<string>();
                var user = await _unitOfWork.UserRepository.GetByIdAsync(material.UserId);
                var materialDto = new GetAllStudyMaterialDto.StudyMaterialDto
                {
                    Id = material.Id,
                    UserId = material.UserId,
                    AverageRating = material.AverageRating,
                    UserName = user?.FullName ?? "Unknown",
                    PhoneNumber = user?.Phone ?? string.Empty,
                    ProfilePicture = user?.ProfilePicture,
                    TrustScore = user?.TrustScore ?? 0,
                    Title = material.Title ?? string.Empty,
                    Description = material.Description ?? string.Empty,
                    Subject = material.Subject ?? string.Empty,
                    Semester = material.Semester,
                    Faculty = material.Faculty,
                    FileUrls = fileUrls,
                    DownloadCount = material.DownloadCount,
                    ViewCount = material.ViewCount,
                    ApprovalStatus = material.ApprovalStatus.ToString() ?? string.Empty,
                    CreatedAt = material.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                };
                return ResponseFactory.Success(materialDto, "Lấy chi tiết tài liệu học tập thành công.", 200);

            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                return ResponseFactory.Fail<GetAllStudyMaterialDto.StudyMaterialDto>($"Đã xảy ra lỗi: {ex.Message}", 500);
            }

        }
    }
}

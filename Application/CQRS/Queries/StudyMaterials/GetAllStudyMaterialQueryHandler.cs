using Application.DTOs.StudyMaterial;
using static Application.DTOs.StudyMaterial.GetAllStudyMaterialDto;

namespace Application.CQRS.Queries.StudyMaterials
{
    public class GetAllStudyMaterialQueryHandler :
        IRequestHandler<GetAllStudyMaterialQuery, ResponseModel<GetAllStudyMaterialDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContextService _userContextService;

        public GetAllStudyMaterialQueryHandler(IUnitOfWork unitOfWork, IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _userContextService = userContextService;
        }

        public async Task<ResponseModel<GetAllStudyMaterialDto>> Handle(GetAllStudyMaterialQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _userContextService.UserId();
                if (userId == Guid.Empty)
                    return ResponseFactory.Fail<GetAllStudyMaterialDto>("User not authenticated", 401);

                // 1️⃣ Gọi repository để lấy danh sách (có phân trang cursor)
                var materials = await _unitOfWork.StudyMaterialRepository.GetAllStudyMaterialAsync(
                    lastLastStudyMaterialIdId: request.LastStudyMaterialId,
                    pageSize: request.PageSize + 1 // +1 để xác định NextCursor
                );

                // 2️⃣ Mapping sang DTO
                var resultMaterials = materials.Take(request.PageSize).Select(material =>
                {
                    var fileUrls = material.FileUrl?
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(url => $"{Constaint.baseUrl}{url}")
                        .ToList() ?? new List<string>();

                    return new StudyMaterialDto
                    {
                        Id = material.Id,
                        UserId = material.UserId,
                        UserName = material.User?.FullName ?? "Unknown",
                        ProfilePicture = material.User?.ProfilePicture,
                        TrustScore = material.User?.TrustScore ?? 0,
                        Title = material.Title,
                        TotalFileSize = material.TotalFileSize,
                        Description = material.Description ?? string.Empty,
                        Subject = material.Subject ?? string.Empty,
                        Semester = material.Semester,
                        Faculty = material.Faculty,
                        FileUrls = fileUrls,
                        DownloadCount = material.DownloadCount,
                        ViewCount = material.ViewCount,
                        ApprovalStatus = material.ApprovalStatus.ToString(),
                        CreatedAt = material.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                }).ToList();

                // 3️⃣ Xử lý phân trang Cursor
                var nextCursor = materials.Count > request.PageSize
                    ? (Guid?)resultMaterials.Last().Id
                    : null;

                // 4️⃣ Tổng số tài liệu
                var totalCount = await _unitOfWork.StudyMaterialRepository.CountAsync();

                // 5️⃣ Trả về kết quả
                return ResponseFactory.Success(
                    new GetAllStudyMaterialDto
                    {
                        Materials = resultMaterials,
                        NextCursor = nextCursor,
                        TotalCount = totalCount
                    },
                    "Get all study materials successful",
                    200
                );
            }
            catch (Exception ex)
            {
                return ResponseFactory.Fail<GetAllStudyMaterialDto>($"An error occurred: {ex.Message}", 500);
            }
        }
    }
}

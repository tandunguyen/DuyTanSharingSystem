using Application.CQRS.Commands.StudyMaterial;
using MediatR;
using System.ComponentModel.DataAnnotations;
using static Application.DTOs.StudyMaterial.GetAllStudyMaterialDto;

namespace Application.CQRS.Commands.StudyMaterials
{
    public class UpdateStudyMaterialCommandHandler : IRequestHandler<UpdateStudyMaterialCommand, ResponseModel<StudyMaterialDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContextService _userContextService;
        private readonly IFileService _fileService;

        private const long MAX_STORAGE_PER_USER = 100L * 1024 * 1024; // 100MB

        public UpdateStudyMaterialCommandHandler(
            IUnitOfWork unitOfWork,
            IUserContextService userContextService,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _userContextService = userContextService;
            _fileService = fileService;
        }

        public async Task<ResponseModel<StudyMaterialDto>> Handle(UpdateStudyMaterialCommand request, CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var userId = _userContextService.UserId();
                if (userId == Guid.Empty)
                    return ResponseFactory.Fail<StudyMaterialDto>("User not authenticated", 401);

                var material = await _unitOfWork.StudyMaterialRepository.GetByIdAsync(request.Id);
                if (material == null || material.IsDeleted)
                    return ResponseFactory.Fail<StudyMaterialDto>("Tài liệu không tồn tại", 404);

                if (material.UserId != userId)
                    return ResponseFactory.Fail<StudyMaterialDto>("Bạn không có quyền chỉnh sửa tài liệu này", 403);

                // === 1. TÍNH DUNG LƯỢNG HIỆN TẠI CỦA USER (TRỪ BỎ FILE CŨ CỦA BÀI NÀY) ===
                var totalUsedByUser = await _unitOfWork.StudyMaterialRepository.GetTotalFileSizeByUserAsync(userId);
                var currentMaterialSize = material.TotalFileSize;
                var remainingAfterRemoveThis = totalUsedByUser - currentMaterialSize;

                // === 2. TÍNH DUNG LƯỢNG FILE MỚI (NẾU CÓ) ===
                long newFilesSize = request.FileUrls?.Sum(f => f.Length) ?? 0;

                // === 3. KIỂM TRA GIỚI HẠN 100MB ===
                if (remainingAfterRemoveThis + newFilesSize > MAX_STORAGE_PER_USER)
                {
                    var usedMB = remainingAfterRemoveThis / (1024.0 * 1024);
                    var limitMB = MAX_STORAGE_PER_USER / (1024.0 * 1024);
                    return ResponseFactory.Fail<StudyMaterialDto>(
                        $"Vượt quá giới hạn lưu trữ: {usedMB:F1}MB / {limitMB}MB. Vui lòng xóa bớt tài liệu cũ trước khi cập nhật.",
                        400);
                }

                // === 4. XỬ LÝ FILE ===
                List<string> finalFileUrls = new();

                // Nếu có file mới → upload và dùng nó
                if (request.FileUrls != null && request.FileUrls.Any())
                {
                    foreach (var file in request.FileUrls)
                    {
                        if (file.Length == 0) continue;
                        if (file.Length > 50 * 1024 * 1024)
                            return ResponseFactory.Fail<StudyMaterialDto>($"File {file.FileName} vượt quá 50MB", 400);

                        var url = await _fileService.SaveFileAsync(file, "study-materials", isImage: false);
                        if (string.IsNullOrEmpty(url))
                            return ResponseFactory.Fail<StudyMaterialDto>("Upload file thất bại", 500);

                        finalFileUrls.Add(url);
                    }
                }
                // Nếu không có file mới → dùng file cũ được giữ lại từ frontend
                else if (request.ExistingFileUrls != null && request.ExistingFileUrls.Any())
                {
                    finalFileUrls = request.ExistingFileUrls.ToList();
                }
                // Nếu cả 2 đều rỗng → giữ nguyên file cũ (trường hợp chỉ sửa tiêu đề)
                else
                {
                    finalFileUrls = material.FileUrl?
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .ToList() ?? new List<string>();
                }

                // === 5. CẬP NHẬT ENTITY ===
                material.Update(
                    title: request.Title ?? material.Title,
                    fileUrl: string.Join(",", finalFileUrls),
                    subject: request.Subject ?? material.Subject,
                    description: request.Description ?? material.Description,
                    semester: request.Semester ?? material.Semester,
                    faculty: request.Faculty ?? material.Faculty
                );

                // Cập nhật dung lượng mới
                material.SetTotalFileSize(finalFileUrls.Any() ? newFilesSize : currentMaterialSize);

                await _unitOfWork.StudyMaterialRepository.UpdateAsync(material);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var user = await _unitOfWork.UserRepository.GetUserByIdAsync(userId);

                return ResponseFactory.Success(
                    new StudyMaterialDto
                    {
                        Id = material.Id,
                        UserId = material.UserId,
                        UserName = user?.FullName ?? "Unknown",
                        ProfilePicture = user?.ProfilePicture,
                        Title = material.Title,
                        Description = material.Description ?? string.Empty,
                        Subject = material.Subject ?? string.Empty,
                        Semester = material.Semester,
                        Faculty = material.Faculty,
                        FileUrls = finalFileUrls.Select(url => $"{Constaint.baseUrl}{url}").ToList(),
                        DownloadCount = material.DownloadCount,
                        ViewCount = material.ViewCount,
                        ApprovalStatus = material.ApprovalStatus.ToString(),
                        CreatedAt = FormatUtcToLocal(material.CreatedAt)
                    },
                    "Cập nhật tài liệu thành công!",
                    200);
            }
            catch (Exception e)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ResponseFactory.Fail<StudyMaterialDto>(e.Message, 500);
            }
        }
    }
}
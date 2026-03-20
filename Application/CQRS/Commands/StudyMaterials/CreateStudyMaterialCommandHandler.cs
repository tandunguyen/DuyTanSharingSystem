using Application.CQRS.Commands.StudyMaterial;

using static Application.DTOs.StudyMaterial.GetAllStudyMaterialDto;

namespace Application.CQRS.Commands.StudyMaterials
{
    public class CreateStudyMaterialCommandHandler : IRequestHandler<CreateStudyMaterialCommand, ResponseModel<StudyMaterialDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContextService _userContextService;
        private readonly IFileService _fileService; // Thêm service upload

        public CreateStudyMaterialCommandHandler(
            IUnitOfWork unitOfWork,
            IUserContextService userContextService,
            IFileService fileService) // Thêm dependency
        {
            _unitOfWork = unitOfWork;
            _userContextService = userContextService;
            _fileService = fileService;
        }

        public async Task<ResponseModel<StudyMaterialDto>> Handle(CreateStudyMaterialCommand request, CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var userId = _userContextService.UserId();
                if (userId == Guid.Empty)
                    return ResponseFactory.Fail<StudyMaterialDto>("User not authenticated", 401);

                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
                if (user == null)
                    return ResponseFactory.Fail<StudyMaterialDto>("User not found", 404);

                if (user.TrustScore < 30)
                    return ResponseFactory.Fail<StudyMaterialDto>("Cần ít nhất 31 điểm uy tín để đăng tài liệu", 403);

                // === KIỂM TRA GIỚI HẠN 100MB/USER ===
                const long MAX_STORAGE_PER_USER = 100L * 1024 * 1024; // 100MB

                var currentTotalSize = await _unitOfWork.StudyMaterialRepository
                    .GetTotalFileSizeByUserAsync(userId); // ← Bạn cần thêm method này ở repo

                long newFilesSize = request.Files?.Sum(f => f.Length) ?? 0;

                if (currentTotalSize + newFilesSize > MAX_STORAGE_PER_USER)
                {
                    var usedMB = currentTotalSize / (1024.0 * 1024);
                    var limitMB = MAX_STORAGE_PER_USER / (1024.0 * 1024);
                    return ResponseFactory.Fail<StudyMaterialDto>(
                        $"Bạn đã vượt quá giới hạn lưu trữ: {usedMB:F1}MB / {limitMB}MB. Vui lòng xóa bớt tài liệu cũ.",
                        400);
                }

                // Upload file
                List<string> fileUrls = new();
                if (request.Files == null || !request.Files.Any())
                    return ResponseFactory.Fail<StudyMaterialDto>("Vui lòng đính kèm ít nhất 1 file", 400);

                foreach (var file in request.Files)
                {
                    if (file.Length == 0) continue;
                    if (file.Length > 50 * 1024 * 1024)
                        return ResponseFactory.Fail<StudyMaterialDto>($"File {file.FileName} vượt quá 50MB", 400);

                    var fileUrl = await _fileService.SaveFileAsync(file, "study-materials", isImage: false);
                    if (string.IsNullOrEmpty(fileUrl))
                        return ResponseFactory.Fail<StudyMaterialDto>("Upload file thất bại", 500);

                    fileUrls.Add(fileUrl);
                }

                // Tạo entity
                var material = new Domain.Entities.StudyMaterial(
                    userId: userId,
                    title: request.Title,
                    fileUrl: string.Join(",", fileUrls),
                    subject: request.Subject,
                    description: request.Description,
                    semester: request.Semester,
                    faculty: request.Faculty
                );

                // Cập nhật dung lượng
                material.SetTotalFileSize(newFilesSize);

                await _unitOfWork.StudyMaterialRepository.AddAsync(material);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return ResponseFactory.Success(
                    new StudyMaterialDto
                    {
                        Id = material.Id,
                        UserId = material.UserId,
                        UserName = _userContextService.FullName() ?? "Unknown",
                        ProfilePicture = user?.ProfilePicture,
                        Title = material.Title,
                        Description = material.Description ?? string.Empty,
                        Subject = material.Subject ?? string.Empty,
                        Semester = material.Semester,
                        Faculty = material.Faculty,
                        FileUrls = fileUrls.Select(url => $"{Constaint.baseUrl}{url}").ToList(),
                        DownloadCount = material.DownloadCount,
                        ViewCount = material.ViewCount,
                        ApprovalStatus = material.ApprovalStatus.ToString(),
                        CreatedAt = FormatUtcToLocal(material.CreatedAt)
                    },
                    "Đăng tài liệu thành công!",
                    201);
            }
            catch (Exception e)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ResponseFactory.Fail<StudyMaterialDto>(e.Message, 500);
            }
        }
    }
    }

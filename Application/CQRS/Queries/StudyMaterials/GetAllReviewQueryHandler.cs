using Application.DTOs.StudyMaterial;
using Domain.Interface; // IUnitOfWork
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CQRS.Queries.StudyMaterials
{
    public class GetAllReviewQueryHandler : IRequestHandler<GetAllReviewQuery, ResponseModel<GetAllStudyMaterialReviewDto>> // Sửa thành GetAllStudyMaterialReviewDto
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetAllReviewQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseModel<GetAllStudyMaterialReviewDto>> Handle(GetAllReviewQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Validation cơ bản
                if (request.StudyMaterialId == Guid.Empty)
                {
                    return ResponseFactory.Fail<GetAllStudyMaterialReviewDto>("StudyMaterialId không được để trống.", 400);
                }

                var reviews = await _unitOfWork.StudyMaterialRatingRepository.GetAllStudyMaterialRatingAsync(
                    lastStudyMaterialRatingId: request.LastStudyMaterialRatingId, // Sửa tên tham số
                    pageSize: request.PageSize + 1, // Sửa thành request.PageSize + 1
                    StudyMaterialId: request.StudyMaterialId); // +1 để kiểm tra NextCursor

                var resultReviews = reviews.Take(request.PageSize).Select(r =>
                {
                    // Xử lý FileUrls nếu entity có FileUrl là string (comma-separated)
                    var fileUrls = r.Material?.FileUrl?
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(url => $"{Constaint.baseUrl.TrimEnd('/')}/{url.TrimStart('/')}")
                        .ToList() ?? new List<string>();
                    
                    return new StudyMaterialReviewDto
                    {
                        Id = r.Id,
                        MaterialId = r.MaterialId,
                        UserId = r.UserId,
                        UserName = r?.User?.FullName ?? "Người dùng ẩn danh",
                        UserAvatarUrl = r?.User?.ProfilePicture ?? "",
                        TrustScore = r?.User?.TrustScore ?? 0,
                        RatingLevel = r.RatingLevel,
                        Comment = r?.Comment ?? "",
                        IsHelpful = r.IsHelpful,
                        CreatedAt = r.CreatedAt
                    };
                }).ToList();

                var nextCursor = reviews.Count > request.PageSize ? (Guid?)resultReviews.Last().Id : null; // Sửa điều kiện

                return ResponseFactory.Success(
                    new GetAllStudyMaterialReviewDto // Tạo instance DTO mới
                    {
                        Reviews = resultReviews,
                        NextCursor = nextCursor
                    },
                    "Get all study material reviews successful",
                    200);
            }
            catch (Exception ex)
            {
                return ResponseFactory.Fail<GetAllStudyMaterialReviewDto>( // Sửa generic type
                    $"Error retrieving study material reviews: {ex.Message}",
                    500);
            }
        }
    }
}
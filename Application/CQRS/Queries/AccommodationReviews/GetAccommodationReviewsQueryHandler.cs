// File: Application/CQRS/Queries/AccommodationReviews/GetAccommodationReviewsQueryHandler.cs

using Application.DTOs.AccommodationReview;
using Application.Interface.ContextSerivce;
using Domain.Interface; // IUnitOfWork
using MediatR;
using static Application.DTOs.AccommodationReview.ResponseAccommodationReviewDto;

namespace Application.CQRS.Queries.AccommodationReviews
{
    public class GetAccommodationReviewsQueryHandler : IRequestHandler<GetAccommodationReviewsQuery, ResponseModel<GetAllAccommodationReviewDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContextService _userContextService; // Giả định dùng để lấy thông tin người dùng hiện tại

        public GetAccommodationReviewsQueryHandler(IUnitOfWork unitOfWork, IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _userContextService = userContextService;
        }

        public async Task<ResponseModel<GetAllAccommodationReviewDto>> Handle(GetAccommodationReviewsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Validation cơ bản
                if (request.AccommodationPostId == Guid.Empty)
                {
                    return ResponseFactory.Fail<GetAllAccommodationReviewDto>("AccommodationPostId không được để trống.", 400);
                }

                // 2. Gọi Repository để lấy danh sách đánh giá theo phân trang (cursor-based)
                var reviews = await _unitOfWork.AccommodationReviewRepository.GetReviewsByAccommodationPostIdAsync(
                    accommodationPostId: request.AccommodationPostId,
                    lastAccommodationReviewId: request.lastAccommodationReviewId,
                    pageSize: request.PageSize + 1); // +1 để kiểm tra NextCursor

                // 3. Ánh xạ (Mapping) từ Entity sang DTO
                var resultReviews = reviews.Take(request.PageSize).Select(r =>
                {
                    // Lấy thông tin người đánh giá (giả định User được Include trong Repository hoặc lấy riêng)
                    

                    return new AccommodationReviewDto
                    {
                        Id = r.Id,
                        AccommodationPostId = r.AccommodationPostId,
                        UserId = r.UserId,
                        UserName = r?.User?.FullName ?? "Người dùng ẩn danh",
                        UserAvatar = r?.User?.ProfilePicture??"",
                        TrustScore = r?.User?.TrustScore ?? 0, // Điểm Uy tín
                        Comment = r?.Comment ?? "",
                        Rating = r?.Rating ?? 0,
                        SafetyScore = r.SafetyScore, // Điểm An toàn (có thể là điểm do AI hoặc người dùng)
                        PriceScore = r.PriceScore,   // Điểm Giá
                        IsApproved = r.IsApproved,   // Trạng thái duyệt
                        CreatedAt = FormatUtcToLocal(r.CreatedAt),
                        // ... thêm các trường khác nếu cần
                    };
                }).ToList();

                // 4. Xử lý phân trang Cursor
                var nextCursor = reviews.Count > request.PageSize ? (Guid?)resultReviews.Last().Id : null;

                // 5. Trả về kết quả
                // Giả định GetAllAccommodationReviewDto là DTO chứa danh sách và NextCursor
                return ResponseFactory.Success(
                    new GetAllAccommodationReviewDto
                    {
                        Reviews = resultReviews,
                        NextCursor = nextCursor,
                        TotalCount = await _unitOfWork.AccommodationReviewRepository.CountAsync(r => r.AccommodationPostId == request.AccommodationPostId)
                    },
                    "Get accommodation reviews successful",
                    200);
            }
            catch (Exception e)
            {
                return ResponseFactory.Fail<GetAllAccommodationReviewDto>(e.Message, 500);
            }
        }
    }
}
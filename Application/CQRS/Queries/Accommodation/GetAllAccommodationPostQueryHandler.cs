// File: Application/CQRS/Queries/AccommodationPosts/GetAllAccommodationPostQueryHandler.cs (Sửa đổi)

using Application.DTOs.Accommodation;
using static Application.DTOs.Accommodation.GetAllAccommodationPostDto;


namespace Application.CQRS.Queries.AccommodationPosts
{
    public class GetAllAccommodationPostQueryHandler : IRequestHandler<GetAllAccommodationPostQuery, ResponseModel<GetAllAccommodationPostDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        // Không cần IMapService ở đây nữa, vì không có tính toán địa lý

        public GetAllAccommodationPostQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseModel<GetAllAccommodationPostDto>> Handle(GetAllAccommodationPostQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Gọi Repository để lấy danh sách mặc định
                var posts = await _unitOfWork.AccommodationPostRepository.GetAllAccommodationPostAsync(
                    lastPostId: request.LastPostId,
                    pageSize: request.PageSize + 1); // +1 để kiểm tra NextCursor

                // 2. Mapping và tính toán (Không cần tính khoảng cách)
                var resultPosts = posts.Take(request.PageSize).Select(p =>
                {
                    return new GetAllAccommodationPostDto.LatLogAccommodation
                    {
                        Id = p.Id,
                        UserId = p.UserId,
                        Title = p.Title,
                        Address = p.Address,
                        Latitude = p.Latitude,
                        Longitude = p.Longitude,
                        Price = p.Price,                      
                        Status = p.Status.ToString(),
                    };
                }).ToList();

                // 3. Xử lý phân trang Cursor
                var nextCursor = posts.Count > request.PageSize ? (Guid?)resultPosts.Last().Id : null;

                // 4. Trả về
                var totalCount = await _unitOfWork.AccommodationPostRepository.CountAsync(p =>  p.Status == Domain.Common.Enums.StatusAccommodationEnum.Available);

                return ResponseFactory.Success(
                    new GetAllAccommodationPostDto
                    {
                        LatLogAccommodations = resultPosts,
                        NextCursor = nextCursor,
                        TotalCount = totalCount
                    },
                    "Get all accommodation posts successful",
                    200);
            }
            catch (Exception e)
            {
                return ResponseFactory.Fail<GetAllAccommodationPostDto>(e.Message, 500);
            }
        }
    }
}
using Application.CQRS.Queries.Accommodation;
using Application.DTOs.Accommodation;
using MediatR;

namespace Application.CQRS.Queries.AccommodationPosts
{
    public class GetAccommodationPostQueryHandler
        : IRequestHandler<GetAccommodationPostQuery, ResponseModel<GetAllAccommodationPostDto.AccommodationPostDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAccommodationReviewRepository _accommodationReviewRepository;

        public GetAccommodationPostQueryHandler(IUnitOfWork unitOfWork, IAccommodationReviewRepository accommodationReviewRepository)
        {
            _unitOfWork = unitOfWork;
            _accommodationReviewRepository = accommodationReviewRepository;
        }

        public async Task<ResponseModel<GetAllAccommodationPostDto.AccommodationPostDto>> Handle(GetAccommodationPostQuery request, CancellationToken cancellationToken)
        {
            var post = await _unitOfWork.AccommodationPostRepository.GetByIdAsync(request.Id);

            if (post == null)
                return ResponseFactory.Fail<GetAllAccommodationPostDto.AccommodationPostDto>("Không tìm thấy bài đăng", 404);
            var ratings = await _accommodationReviewRepository.GetRatingsByAccommodationPostIdAsync(post.Id);
            var averageRating = 0.0;
            if (ratings != null && ratings.Count > 0)
            {
                averageRating = ratings.Average();
            }
            var result = new GetAllAccommodationPostDto.AccommodationPostDto
            {
                Id = post.Id,
                UserId = post.UserId,
                AverageRating = (float)Math.Round(averageRating, 1),
                UserName = post.User?.FullName ?? "N/A",
                PhoneNumber = post.User?.Phone ?? "N/A",
                ProfilePicture = post.User?.ProfilePicture ?? "",
                TrustScore = post.User?.TrustScore ?? 0,
                Title = post.Title,
                Content = post.Content ?? "",
                Address = post.Address,
                MaxPeople = post.MaxPeople,
                CurrentPeople = post.CurrentPeople,
                Latitude = post.Latitude,
                Longitude = post.Longitude,
                Price = post.Price,
                Area = post.Area,
                RoomType = post.RoomType,
                Status = post.Status.ToString(),
                CreatedAt = post.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            };

            return ResponseFactory.Success(result, "Get accommodation post detail successful", 200);
        }
    }
}

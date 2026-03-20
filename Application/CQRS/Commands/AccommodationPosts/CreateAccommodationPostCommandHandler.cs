using Application.DTOs.Accommodation;
namespace Application.CQRS.Commands.AccommodationPosts
{
    public class CreateAccommodationPostCommandHandler : IRequestHandler<CreateAccommodationPostCommand, ResponseModel<AccommodationPostDto>>
    {
        private readonly IUserContextService _userContextService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapService _mapService; // Vẫn cần để Dịch Ngược Tọa Độ

        public CreateAccommodationPostCommandHandler(IUnitOfWork unitOfWork, IMapService mapService, IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _mapService = mapService;
            _userContextService = userContextService;
        }

        public async Task<ResponseModel<AccommodationPostDto>> Handle(CreateAccommodationPostCommand request, CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var userId = _userContextService.UserId();
                if (userId == Guid.Empty)
                    return ResponseFactory.Fail<AccommodationPostDto>("User not authenticated", 401);
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
                if(user == null)
                    return ResponseFactory.Fail<AccommodationPostDto>("User not found", 404);
                if(user.TrustScore < 30 && user.TrustScore >= 0)
                    return ResponseFactory.Fail<AccommodationPostDto>("Để thao tác được chức năng này, bàn cần đạt ít nhất 31 điểm uy tín", 403);
                // 1. Tọa độ được lấy trực tiếp từ request của FE (Người dùng click trên bản đồ)
                double lat = request.Latitude;
                double lng = request.Longitude;

                // 2. Dịch Ngược Tọa Độ thành Địa Chỉ (Reverse Geocoding)
                // Điều này giúp lưu một địa chỉ văn bản chính thức vào cột 'Address' trong DB.
                // Nếu dịch ngược thất bại, chúng ta sử dụng chuỗi người dùng nhập (AddressString) hoặc tọa độ.
                string finalAddress = request.AddressString ?? $"{lat}, {lng}"; // Giá trị mặc định

                try
                {
                    var addressFromMap = await _mapService.GetAddressFromCoordinatesAsync(lat, lng);
                    if (!string.IsNullOrEmpty(addressFromMap))
                    {
                        finalAddress = addressFromMap;
                    }
                }
                catch (Exception)
                {
                    // Lỗi dịch ngược: sử dụng AddressString hoặc tọa độ thô (đảm bảo bài đăng vẫn được tạo)
                    // Không cần ném lỗi vì mục tiêu chính (tọa độ) đã có.
                }

                // 3. Tạo Entity
                var post = new AccommodationPost(
                    userId: userId,
                    title: request.Title,
                    // Dùng finalAddress (đã được dịch ngược hoặc là string thô)
                    address: finalAddress,
                    latitude: lat,
                    longitude: lng,
                    price: request.Price,
                    area: request.Area,
                    roomType: request.RoomType,
                    content: request.Content,
                    amenities: request.Amenities,
                    maxPeople: request.MaxPeople,
                    currentPeople: request.CurrentPeople
                );

                // 4. Lưu vào DB
                await _unitOfWork.AccommodationPostRepository.AddAsync(post);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // 5. Trả về DTO
                return ResponseFactory.Success(
                    new AccommodationPostDto
                    {
                        Id = post.Id,
                        UserId = post.UserId,
                        Title = post.Title,
                        Area = post.Area,
                        RoomType = post.RoomType,
                        // Trả về địa chỉ đã được BE xử lý (Dịch ngược)
                        Address = finalAddress,
                        Latitude = post.Latitude,
                        Longitude = post.Longitude,
                        Price = post.Price,
                        Status = post.Status,
                        MaxPeople = post.MaxPeople,
                        CurrentPeople = post.CurrentPeople,
                        CreatedAt = FormatUtcToLocal(post.CreatedAt)
                    },
                    "Accommodation Post created successfully",
                    201);
            }
            catch (Exception e)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ResponseFactory.Fail<AccommodationPostDto>(e.Message, 500);
            }
        }
    }
}
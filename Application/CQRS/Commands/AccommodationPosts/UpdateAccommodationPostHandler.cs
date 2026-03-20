// File: Application/CQRS/Commands/AccommodationPosts/UpdateAccommodationPostHandler.cs

using Application.DTOs.Accommodation;

namespace Application.CQRS.Commands.AccommodationPosts
{
    public class UpdateAccommodationPostHandler : IRequestHandler<UpdateAccommodationPost, ResponseModel<AccommodationPostDto>>
    {
        private readonly IUserContextService _userContextService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapService _mapService;

        public UpdateAccommodationPostHandler(IUnitOfWork unitOfWork, IMapService mapService, IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _mapService = mapService;
            _userContextService = userContextService;
        }

        public async Task<ResponseModel<AccommodationPostDto>> Handle(UpdateAccommodationPost request, CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var userId = _userContextService.UserId();
                if (userId == Guid.Empty)
                    return ResponseFactory.Fail<AccommodationPostDto>("User not authenticated", 401);

                // 1. Tìm bài đăng cần cập nhật
                var post = await _unitOfWork.AccommodationPostRepository.GetByIdAsync(request.Id);

                if (post == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ResponseFactory.Fail<AccommodationPostDto>("Accommodation Post not found", 404);
                }

                // 2. Kiểm tra quyền sở hữu
                if (post.UserId != userId)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ResponseFactory.Fail<AccommodationPostDto>("You do not have permission to update this post", 403);
                }

                // 3. Xử lý cập nhật Tọa độ và Địa chỉ
                string? newAddress = null;
                double? newLat = post.Latitude;
                double? newLng = post.Longitude;

                if (request.Latitude.HasValue && request.Longitude.HasValue)
                {
                    newLat = request.Latitude.Value;
                    newLng = request.Longitude.Value;

                    try
                    {
                        // Reverse Geocoding: Dịch ngược tọa độ mới thành địa chỉ văn bản chính thức
                        newAddress = await _mapService.GetAddressFromCoordinatesAsync(newLat.Value, newLng.Value);
                    }
                    catch (Exception)
                    {
                        // Lỗi dịch ngược: sử dụng AddressString hoặc tọa độ thô làm địa chỉ
                        newAddress = request.AddressString ?? $"{newLat}, {newLng}";
                    }
                }

                // 4. Cập nhật các trường
                post.UpdatePost(
                    title: request.Title ?? post.Title,
                    content: request.Content ?? post.Content,
                    // Dùng newAddress nếu có cập nhật vị trí, ngược lại giữ nguyên địa chỉ cũ
                    address: newAddress ?? post.Address,
                    latitude: newLat.Value,
                    longitude: newLng.Value,
                    price: request.Price ?? post.Price,
                    area: request.Area ?? post.Area,
                    roomType: request.RoomType ?? post.RoomType,
                    amenities: request.Amenities ?? post.Amenities,
                    maxPeople: request.MaxPeople ?? post.MaxPeople,
                    currentPeople: request.CurrentPeople ?? post.CurrentPeople
                );

                // 5. Lưu vào DB
                await _unitOfWork.AccommodationPostRepository.UpdateAsync(post);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // 6. Trả về DTO
                return ResponseFactory.Success(
                    new AccommodationPostDto
                    {
                        Id = post.Id,
                        UserId = post.UserId,
                        Title = post.Title,
                        Address = post.Address,
                        Latitude = post.Latitude,
                        Longitude = post.Longitude,
                        Price = post.Price,
                        Status = post.Status,
                        CreatedAt = FormatUtcToLocal(post.CreatedAt),
                        MaxPeople = post.MaxPeople,
                        CurrentPeople = post.CurrentPeople,

                    },
                    "Accommodation Post updated successfully",
                    200);
            }
            catch (Exception e)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ResponseFactory.Fail<AccommodationPostDto>(e.Message, 500);
            }
        }
    }
}
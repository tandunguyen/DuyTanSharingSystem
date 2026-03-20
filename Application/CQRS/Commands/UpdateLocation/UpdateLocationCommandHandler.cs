using Application.DTOs.UpdateLocation;

namespace Application.CQRS.Commands.UpdateLocation
{
    public class UpdateLocationCommandHandler : IRequestHandler<UpdateLocationCommand, ResponseModel<UpdateLocationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisService _redisService;
        private readonly IUserContextService _userContextService;
        private readonly INotificationService _notificationService;

        public UpdateLocationCommandHandler(
            IUnitOfWork unitOfWork,
            IRedisService redisService,
            IUserContextService userContextService,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _redisService = redisService;
            _userContextService = userContextService;
            _notificationService = notificationService;
        }

        public async Task<ResponseModel<UpdateLocationDto>> Handle(UpdateLocationCommand request, CancellationToken cancellationToken)
        {
            // Kiểm tra tọa độ hợp lệ
            if (!IsValidCoordinates(request.Latitude, request.Longitude))
            {
                return ResponseFactory.Fail<UpdateLocationDto>("Tọa độ không hợp lệ", 400);
            }

            // Lấy thông tin chuyến đi
            var ride = await _unitOfWork.RideRepository.GetByIdAsync(request.RideId);
            if (ride == null)
            {
                return ResponseFactory.Fail<UpdateLocationDto>("Không tìm thấy chuyến đi", 404);
            }
            if (ride.Status == StatusRideEnum.Completed)
            {
                // Cho phép cập nhật nếu gần đến đích để hoàn thành chuyến đi, nhưng không lưu vị trí mới nữa.
                if (!request.IsNearDestination)
                {
                    return ResponseFactory.Fail<UpdateLocationDto>("Chuyến đi đã hoàn thành", 400);
                }
            }
            var ridePost = await _unitOfWork.RidePostRepository.GetByIdAsync(ride.RidePostId);
            if (ridePost == null)
            {
                return ResponseFactory.Fail<UpdateLocationDto>("Không tìm thấy bài đăng chuyến đi", 404);
            }

            var userId = _userContextService.UserId();
            bool isDriver = ride.DriverId == userId;
            bool isSafetyTrackingEnabled = ride.IsSafetyTrackingEnabled;
            LocationUpdate? locationUpdate = null; // Khởi tạo là null

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Làm sạch request.Location
                string cleanLocation = request.Location ?? $"{request.Latitude}, {request.Longitude}"; // Cung cấp giá trị mặc định nếu null
                if (cleanLocation.Contains("Tài xế đã cập nhật vị trí tại:") || cleanLocation.Contains("Hành khách đã cập nhật vị trí tại:"))
                {
                    cleanLocation = cleanLocation
                        .Replace("Tài xế đã cập nhật vị trí tại:", "")
                        .Replace("Hành khách đã cập nhật vị trí tại:", "")
                        .Trim();
                }

                // **THAY ĐỔI CHÍNH: Luôn thêm vị trí mới nếu được phép và chuyến đi chưa hoàn thành**
                if ((isDriver || isSafetyTrackingEnabled) && ride.Status != StatusRideEnum.Completed)
                {
                    // Tạo bản ghi vị trí mới
                    locationUpdate = new LocationUpdate(request.RideId, userId, request.Latitude, request.Longitude, isDriver);
                    //await _unitOfWork.LocationUpdateRepository.AddAsync(locationUpdate);

                    // Lưu sự kiện vị trí vào Redis
                    await _redisService.AddAsync("update_location_events", locationUpdate);

                    // Gửi thông báo (nếu cần, bỏ comment)
                    // string notificationMessage = isDriver
                    //             ? $"Tài xế đã cập nhật vị trí tại: {cleanLocation}"
                    //             : $"Hành khách đã cập nhật vị trí tại: {cleanLocation}";
                    // await _notificationService.SendNotificationUpdateLocationAsync(...);

                    // Cập nhật thời gian bắt đầu nếu chưa có VÀ đã di chuyển được 50m
                    if (ride.StartTime == null)
                    {
                        var startCoords = ParseLatLon(ridePost.LatLonStart);
                        if (startCoords != null && startCoords.Length == 2)
                        {
                            double distanceMovedKm = CalculateDistance(startCoords[0], startCoords[1], request.Latitude, request.Longitude);
                            if (distanceMovedKm >= 0.05)
                            {
                                ride.UpdateStartTime();
                                await _unitOfWork.RideRepository.UpdateAsync(ride);
                            }
                        }
                    }
                }

                // Xử lý hoàn thành chuyến đi nếu gần đến đích
                if (request.IsNearDestination && ride.Status != StatusRideEnum.Completed)
                {
                    Console.WriteLine($"[Backend] Completing ride {request.RideId} at {DateTime.UtcNow}");
                    ride.UpdateStatus(StatusRideEnum.Completed);
                    await _unitOfWork.RideRepository.UpdateAsync(ride);

                    // Gửi thông báo hoàn thành (nếu cần, bỏ comment)
                    // await _notificationService.SendNotificationUpdateLocationAsync(...);
                }

                // Lưu tất cả thay đổi vào DB
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Tạo DTO trả về
                return ResponseFactory.Success(new UpdateLocationDto
                {
                    // Sử dụng Id từ locationUpdate nếu nó được tạo, nếu không dùng Guid mới (hoặc null tùy logic)
                    Id = locationUpdate?.Id ?? Guid.Empty, // Sử dụng Guid.Empty thay vì NewGuid nếu không có gì được thêm
                    RideId = request.RideId,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    // Sử dụng timestamp từ locationUpdate nếu có, nếu không dùng UtcNow
                    // Đảm bảo bạn có hàm FormatUtcToLocal hoặc thay thế bằng .ToString("o")
                    Timestamp = locationUpdate != null ? FormatUtcToLocal(locationUpdate.Timestamp) : DateTime.UtcNow.ToString("o"),
                    RideStatus = ride.Status.ToString()
                }, "Cập nhật vị trí thành công", 200);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                // Ghi log lỗi chi tiết hơn
                Console.WriteLine($"[Backend Error] UpdateLocation failed: {ex.ToString()}");
                return ResponseFactory.Fail<UpdateLocationDto>("Cập nhật vị trí thất bại: " + ex.Message, 500);
            }
        }
        private bool IsValidCoordinates(double latitude, double longitude)
        {
            return latitude is >= -90 and <= 90 && longitude is >= -180 and <= 180;
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371e3; // Bán kính trái đất (mét)
            double φ1 = lat1 * Math.PI / 180;
            double φ2 = lat2 * Math.PI / 180;
            double Δφ = (lat2 - lat1) * Math.PI / 180;
            double Δλ = (lon2 - lon1) * Math.PI / 180;

            double a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                             Math.Cos(φ1) * Math.Cos(φ2) * Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c / 1000; // Khoảng cách tính bằng km
        }

        private double[]? ParseLatLon(string latLonString)
        {
            if (string.IsNullOrEmpty(latLonString) || latLonString == "0")
                return null;
            var parts = latLonString.Split(',');
            if (parts.Length != 2 || !double.TryParse(parts[0], out double lat) || !double.TryParse(parts[1], out double lon))
                return null;
            return new[] { lat, lon };
        }
    }
}
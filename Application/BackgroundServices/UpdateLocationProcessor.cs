using Application.Model.Events;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging; // Thêm
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading; // Thêm
using System.Threading.Tasks;

namespace Application.BackgroundServices
{
    public class UpdateLocationProcessor : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UpdateLocationProcessor> _logger; // Thêm Logger
        private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(10); // Đặt thời gian trễ thành biến cấu hình

        public UpdateLocationProcessor(IServiceProvider serviceProvider, ILogger<UpdateLocationProcessor> logger) // Inject ILogger
        {
            _serviceProvider = serviceProvider;
            _logger = logger; // Khởi tạo logger
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("UpdateLocationProcessor background service started."); // Ghi log khi dịch vụ bắt đầu

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var redisService = scope.ServiceProvider.GetRequiredService<ICacheService>();
                    var updateLocationRepository = scope.ServiceProvider.GetRequiredService<ILocationUpdateRepository>();
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var rideRepository = scope.ServiceProvider.GetRequiredService<IRideRepository>(); // Thêm RideRepository

                    List<LocationUpdate>? updateLocationEvents = null; // Thay đổi kiểu dữ liệu để khớp với sự kiện
                    string redisKey = "update_location_events";

                    try
                    {
                        updateLocationEvents = await redisService.GetAsync<List<LocationUpdate>>(redisKey);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error retrieving location update events from Redis.");
                        // Có thể xử lý lỗi Redis cụ thể ở đây, ví dụ: chờ lâu hơn trước khi thử lại
                    }

                    if (updateLocationEvents?.Any() == true)
                    {
                        _logger.LogInformation($"Processing {updateLocationEvents.Count} location update events.");

                        await unitOfWork.BeginTransactionAsync(); // 🛠 Bắt đầu transaction

                        try
                        {
                            // Chuyển đổi từ Event sang Entity Domain
                            var updateEntities = updateLocationEvents
                                .Select(e => new LocationUpdate(e.RideId, e.UserId, e.Latitude, e.Longitude, e.IsDriver))
                                .ToList();

                            // Kiểm tra chuyến đi (ride) chỉ một lần cho lô này nếu tất cả các cập nhật thuộc cùng một chuyến đi.
                            // Cần điều chỉnh nếu một lô có thể chứa cập nhật cho nhiều chuyến đi khác nhau.
                            // Giả định rằng tất cả các sự kiện trong một lô thuộc về cùng một RideId.
                            var firstRideId = updateEntities.First().RideId;
                            var ride = await rideRepository.GetByIdAsync(firstRideId);

                            if (ride == null)
                            {
                                // Nếu không tìm thấy chuyến đi, chúng ta không thể xử lý các cập nhật này.
                                // Cần quyết định: có nên rollback và bỏ qua lô này, hay chỉ bỏ qua các cập nhật không có chuyến đi hợp lệ?
                                // Hiện tại, tôi sẽ rollback toàn bộ lô nếu không tìm thấy chuyến đi cho sự kiện đầu tiên.
                                throw new InvalidOperationException($"Ride with ID {firstRideId} not found for location updates. Rolling back transaction.");
                            }

                            await updateLocationRepository.AddRangeAsync(updateEntities);
                            await unitOfWork.SaveChangesAsync();
                            await unitOfWork.CommitTransactionAsync(); // ✅ Commit transaction

                            // Sau khi commit thành công, xóa dữ liệu đã xử lý khỏi Redis
                            await redisService.RemoveAsync(redisKey);
                            _logger.LogInformation($"Successfully processed and committed {updateLocationEvents.Count} location updates for Ride ID: {firstRideId}. Removed from Redis.");
                        }
                        catch (InvalidOperationException ex) // Bắt các ngoại lệ nghiệp vụ cụ thể
                        {
                            await unitOfWork.RollbackTransactionAsync(); // ❌ Rollback nếu có lỗi nghiệp vụ
                            _logger.LogWarning(ex, $"Business rule violation during location update processing: {ex.Message}");
                            // Có thể thêm logic để chuyển các sự kiện bị lỗi sang hàng đợi khác hoặc bỏ qua
                        }
                        catch (Exception ex) // Bắt các ngoại lệ chung
                        {
                            await unitOfWork.RollbackTransactionAsync(); // ❌ Rollback nếu có lỗi
                            _logger.LogError(ex, "An error occurred during location update processing. Transaction rolled back.");
                            // Ném lại ngoại lệ hoặc xử lý thêm tùy thuộc vào yêu cầu của bạn.
                            // Ví dụ: có thể gửi thông báo lỗi đến hệ thống giám sát.
                        }
                    }
                }

                await Task.Delay(_processingInterval, stoppingToken); // Chờ trước khi lặp lại
            }

            _logger.LogInformation("UpdateLocationProcessor background service stopped."); // Ghi log khi dịch vụ dừng
        }
    }
}
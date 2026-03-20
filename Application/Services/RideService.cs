using Application.DTOs.Ride;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class RideService : IRideService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RideService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<RideDetailManagementDto?> GetRideDetailsAsync(Guid id)
        {
            var ride = await _unitOfWork.RideRepository.GetRideDetailsAsync(id);

            if (ride == null)
            {
                return null;
            }

            return new RideDetailManagementDto
            {
                RidePost = new RidePostInfo
                {
                    Content = ride.RidePost?.Content ?? "N/A",
                    StartLocation = ride.RidePost?.StartLocation ?? "N/A",
                    EndLocation = ride.RidePost?.EndLocation ?? "N/A",
                    StartTime = ride.RidePost?.StartTime ?? DateTime.MinValue,
                    CreatedAt = ride.RidePost?.CreatedAt ?? DateTime.MinValue
                },
                Driver = new UserInfo
                {
                    FullName = ride.Driver?.FullName ?? "N/A",
                    Email = ride.Driver?.Email ?? "N/A",
                    TrustScore = ride.Driver?.TrustScore ?? 0,
                    Phone = ride.Driver?.Phone ?? "N/A",
                    RelativePhone = ride.Driver?.RelativePhone ?? "N/A"
                },
                Passenger = new UserInfo
                {
                    FullName = ride.Passenger?.FullName ?? "N/A",
                    Email = ride.Passenger?.Email ?? "N/A",
                    TrustScore = ride.Passenger?.TrustScore ?? 0,
                    Phone = ride.Passenger?.Phone ?? "N/A",
                    RelativePhone = ride.Passenger?.RelativePhone ?? "N/A"
                },
                DriverLocations = ride.LocationUpdates?.Where(lu => lu.IsDriver)
                    .Select(lu => new LocationUpdateDto
                    {
                        Latitude = lu.Latitude,
                        Longitude = lu.Longitude,
                        Timestamp = lu.Timestamp
                    })
                    .ToList() ?? new List<LocationUpdateDto>(),
                PassengerLocations = ride.LocationUpdates?.Where(lu => !lu.IsDriver)
                    .Select(lu => new LocationUpdateDto
                    {
                        Latitude = lu.Latitude,
                        Longitude = lu.Longitude,
                        Timestamp = lu.Timestamp
                    })
                    .ToList() ?? new List<LocationUpdateDto>()
            };
        }
        public async Task<PagedRideManagementDto> GetRidesByStatusAsync(StatusRideEnum status, int page, int pageSize)
        {
            // Đảm bảo page và pageSize hợp lệ
            page = Math.Max(1, page); // page tối thiểu là 1
            pageSize = Math.Max(1, pageSize); // pageSize tối thiểu là 1

            // Lấy dữ liệu từ repository
            var (rides, totalRecords) = await _unitOfWork.RideRepository.GetRidesByStatusAsync(status, page, pageSize);

            // Ánh xạ sang DTO
            var rideDtos = rides.Select(r => new RideManagementDto
            {
                Id = r.Id,
                DriverId = r.DriverId,
                PassengerId = r.PassengerId,
                RidePostId = r.RidePostId,
                StartLocation = r.RidePost?.StartLocation ?? "N/A",
                EndLocation = r.RidePost?.EndLocation ?? "N/A",
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                EstimatedDuration = r.EstimatedDuration,
                CreatedAt = r.CreatedAt,
                IsSafetyTrackingEnabled = r.IsSafetyTrackingEnabled
            }).ToList();

            // Tính tổng số trang
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            return new PagedRideManagementDto
            {
                Rides = rideDtos,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            };
        }
    }
}

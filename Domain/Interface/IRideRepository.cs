using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.Common.Enums;

namespace Domain.Interface
{
    public interface IRideRepository : IBaseRepository<Ride>
    {
        Task<IEnumerable<Ride>> GetActiveRidesAsync();
        Task<Ride?> GetRideByUserIdAsync(Guid userId);
        Task<IEnumerable<Ride>> GetActiveRidesByPassengerIdAsync(Guid passengerId);
        Task<IEnumerable<Ride>> GetActiveRidesByDriverIdIdAsync(Guid driverId);
        Task<int> GetDriveRideCountAsync(Guid userId);
        Task<int> GetPassengerRideCountAsync(Guid userId);
        Task<List<Ride>> GetRidePostsByPassengerIdAsync(Guid passengerId, Guid? lastPostId, int pageSize);
        Task<List<Ride>> GetRidePostsByDriverIdAsync(Guid driverId, Guid? lastPostId, int pageSize);
        Task<List<Ride>> GetActiveRidesByDriverIdAsync(Guid driverId);
        Task<List<Ride>> GetCompletedRidesWithRatingAsync(Guid driverId);
        Task<(List<Ride> Rides, int TotalRecords)> GetRidesByStatusAsync(StatusRideEnum status, int page, int pageSize);
        Task<Ride?> GetRideDetailsAsync(Guid id);
        Task<List<Ride>> GetRidesByTimeRangeAsync(DateTime? startDate, DateTime? endDate);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Ride
{
    public class RideDetailManagementDto
    {
        public RidePostInfo RidePost { get; set; } = new RidePostInfo();
        public UserInfo Driver { get; set; } = new UserInfo();
        public UserInfo Passenger { get; set; } = new UserInfo();
        public List<LocationUpdateDto> DriverLocations { get; set; } = new List<LocationUpdateDto>();
        public List<LocationUpdateDto> PassengerLocations { get; set; } = new List<LocationUpdateDto>();
    }

    public class RidePostInfo
    {
        public string? Content { get; set; }
        public string? StartLocation { get; set; }
        public string? EndLocation { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UserInfo
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public decimal TrustScore { get; set; }
        public string? Phone { get; set; } 
        public string? RelativePhone { get; set; }
    }

    public class LocationUpdateDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
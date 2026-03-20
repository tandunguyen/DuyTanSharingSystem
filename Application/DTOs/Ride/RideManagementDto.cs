using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Ride
{
    public class RideManagementDto
    {
        public Guid Id { get; set; }
        public Guid DriverId { get; set; }
        public Guid PassengerId { get; set; }
        public Guid RidePostId { get; set; }
        public required string StartLocation { get; set; }
        public required string EndLocation { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int EstimatedDuration { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsSafetyTrackingEnabled { get; set; }
    }
}

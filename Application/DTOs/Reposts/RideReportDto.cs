
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reposts
{
    public class RideReportDto
    {
        public Guid Id { get; set; }
        public Guid RideId { get; set; }
        public Guid PassengerId { get; set; }
        public Guid DriverId { get; set; }
        public string? Message { get; set; }
        public string? PhonePassenger { get; set; }
        public string? NamePassenger { get; set; }
        public string? NameDriver { get; set; }
        public string? RelativePhonePassenger { get; set; }
        public string? PhoneDriver { get; set; }
        public string? RelativePhoneDriver { get; set; }
        public AlertTypeEnums AlertType { get; set; } 
        public DateTime CreatedAt { get; set; }
    }
}

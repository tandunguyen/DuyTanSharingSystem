using Application.DTOs.Ride;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface
{
    public interface IRideService
    {
        Task<PagedRideManagementDto> GetRidesByStatusAsync(StatusRideEnum status, int page, int pageSize);
        Task<RideDetailManagementDto?> GetRideDetailsAsync(Guid id);
    }
}

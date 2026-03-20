using Application.DTOs.DasbroadAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface
{
    public interface IDashboardAdminService
    {
        Task<DashboardOverviewDto> GetOverviewAsync();
        Task<DashboardReportStatsDto> GetReportStatsAsync();
        Task<DashboardUserStatsDto> GetUserStatsAsync();
        Task<DashboardTrustScoreStatsDto> GetTrustScoreDistributionAsync();
        Task<UserTrendDto> GetUserTrendAsync(string timeRange);
        Task<InteractionActivityDto> GetInteractionActivityAsync(string timeRange);
        Task<UserTrustDto> GetUserTrustDistributionAsync();
        Task<RatingStatisticsDto> GetRatingStatisticsAsync();
        Task<List<RideStatusStatisticsDto>> GetRideStatusStatisticsAsync(string groupBy);

    }
}

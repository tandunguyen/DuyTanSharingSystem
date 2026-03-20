using Application.DTOs.DasbroadAdmin;
using Domain.Interface;

namespace Application.Services
{
    public class DashboardAdminService : IDashboardAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        public DashboardAdminService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<InteractionActivityDto> GetInteractionActivityAsync(string timeRange)
        {
            var likes = (await _unitOfWork.LikeRepository.GetLikesOverTimeAsync(timeRange)).ToList();
            var comments = (await _unitOfWork.CommentRepository.GetCommentsOverTimeAsync(timeRange)).ToList();
            var shares = (await _unitOfWork.ShareRepository.GetSharesOverTimeAsync(timeRange)).ToList();

            // Combine all dates to create a unified list of labels
            var allDates = likes.Select(l => l.Date)
                .Union(comments.Select(c => c.Date))
                .Union(shares.Select(s => s.Date))
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            var labels = allDates.Select(d => FormatDate(d, timeRange)).ToList();
            var likesData = new List<int>();
            var commentsData = new List<int>();
            var sharesData = new List<int>();

            foreach (var date in allDates)
            {
                var likeEntry = likes.FirstOrDefault(l => l.Date.Date == date.Date);
                var commentEntry = comments.FirstOrDefault(c => c.Date.Date == date.Date);
                var shareEntry = shares.FirstOrDefault(s => s.Date.Date == date.Date);

                // Kiểm tra nếu likeEntry, commentEntry, shareEntry có giá trị (khác mặc định)
                likesData.Add(likeEntry.Equals(default((DateTime, int))) ? 0 : likeEntry.Count);
                commentsData.Add(commentEntry.Equals(default((DateTime, int))) ? 0 : commentEntry.Count);
                sharesData.Add(shareEntry.Equals(default((DateTime, int))) ? 0 : shareEntry.Count);
            }

            return new InteractionActivityDto
            {
                Labels = labels,
                Datasets = new InteractionDatasets
                {
                    Likes = likesData,
                    Comments = commentsData,
                    Shares = sharesData
                }
            };
        }

        public async Task<DashboardOverviewDto> GetOverviewAsync()
        {
            var totalUsers = await _unitOfWork.UserRepository.CountAsync();
            var totalLockedUsers = await _unitOfWork.UserRepository.CountAsync(x => x.Status == "Blocked");
            var totalUserReports = await _unitOfWork.UserReportRepository.CountAsync();
            var totalPostReports = await _unitOfWork.ReportRepository.CountAsync(); // Giả sử Report = bài viết bị báo cáo

            return new DashboardOverviewDto
            {
                TotalUsers = totalUsers,
                TotalLockedUsers = totalLockedUsers,
                TotalUserReports = totalUserReports,
                TotalPostReports = totalPostReports
            };
        }

        public async Task<DashboardReportStatsDto> GetReportStatsAsync()
        {
            var pendingReports = await _unitOfWork.UserReportRepository.CountAsync(x => x.Status == "Pending");
            var acceptedReports = await _unitOfWork.UserReportRepository.CountAsync(x => x.Status == "Accepted");
            var rejectedReports = await _unitOfWork.UserReportRepository.CountAsync(x => x.Status == "Rejected");

            return new DashboardReportStatsDto
            {
                PendingReports = pendingReports,
                AcceptedReports = acceptedReports,
                RejectedReports = rejectedReports
            };
        }

        public async Task<DashboardTrustScoreStatsDto> GetTrustScoreDistributionAsync()
        {
            var users = await _unitOfWork.UserRepository.GetAllAsync();
            var reliable  = users.Count(x => x.TrustScore >= 50);
            var unreliable = users.Count(x => x.TrustScore < 50);
            return new DashboardTrustScoreStatsDto
            {
                reliableTrustScore = reliable,
                unreliableTrustScore = unreliable
            };
        }

        public async Task<DashboardUserStatsDto> GetUserStatsAsync()
        {
            var activeUsers = await _unitOfWork.UserRepository.CountAsync(x => x.Status == "Active");
            var suspendedUsers = await _unitOfWork.UserRepository.CountAsync(x => x.Status == "Suspended");
            var lockedUsers = await _unitOfWork.UserRepository.CountAsync(x => x.Status == "Blocked");

            return new DashboardUserStatsDto
            {
                ActiveUsers = activeUsers,
                SuspendedUsers = suspendedUsers,
                LockedUsers = lockedUsers
            };
        }

        public async Task<UserTrendDto> GetUserTrendAsync(string timeRange)
        {
            var userTrends = await _unitOfWork.UserRepository.GetUserTrendAsync(timeRange);
            var labels = userTrends.Select(t => FormatDate(t.Date, timeRange)).ToList();
            var data = userTrends.Select(t => t.Count).ToList();

            return new UserTrendDto
            {
                Labels = labels,
                Data = data
            };
        }

        public async Task<UserTrustDto> GetUserTrustDistributionAsync()
        {
            var trustDistribution = await _unitOfWork.UserRepository.GetUserTrustDistributionAsync();
            var labels = trustDistribution.Select(t => t.TrustCategory).ToList();
            var data = trustDistribution.Select(t => t.Count).ToList();

            return new UserTrustDto
            {
                Labels = labels,
                Data = data
            };
        }
        public async Task<RatingStatisticsDto> GetRatingStatisticsAsync()
        {
            var totalRatings = await _unitOfWork.RatingRepository.GetAllAsync();
            var totalCount = totalRatings.Count;

            if (totalCount == 0)
            {
                return new RatingStatisticsDto
                {
                    PoorPercentage = 0,
                    AveragePercentage = 0,
                    GoodPercentage = 0,
                    ExcellentPercentage = 0
                };
            }

            // Lấy số lượng đánh giá theo Level từ repository
            var ratingCounts = await _unitOfWork.RatingRepository.GetRatingCountsByLevelAsync();

            var poorCount = ratingCounts[RatingLevelEnum.Poor];
            var averageCount = ratingCounts[RatingLevelEnum.Average];
            var goodCount = ratingCounts[RatingLevelEnum.Good];
            var excellentCount = ratingCounts[RatingLevelEnum.Excellent];

            return new RatingStatisticsDto
            {
                PoorPercentage = Math.Round((double)poorCount / totalCount * 100, 2),
                AveragePercentage = Math.Round((double)averageCount / totalCount * 100, 2),
                GoodPercentage = Math.Round((double)goodCount / totalCount * 100, 2),
                ExcellentPercentage = Math.Round((double)excellentCount / totalCount * 100, 2)
            };
        }

        // Thêm phương thức mới: Thống kê số lượng chuyến đi theo trạng thái
        public async Task<List<RideStatusStatisticsDto>> GetRideStatusStatisticsAsync(string groupBy)
        {
            var rides = await _unitOfWork.RideRepository.GetAllAsync();

            var groupedRides = groupBy.ToLower() switch
            {
                "week" => rides
                    .GroupBy(r => new { Year = r.CreatedAt.Year, Week = GetWeekOfYear(r.CreatedAt) })
                    .Select(g => new RideStatusStatisticsDto
                    {
                        TimeLabel = $"{g.Key.Year}-W{g.Key.Week}",
                        RejectedCount = g.Count(r => r.Status == StatusRideEnum.Rejected),
                        AcceptedCount = g.Count(r => r.Status == StatusRideEnum.Accepted),
                        CompletedCount = g.Count(r => r.Status == StatusRideEnum.Completed)
                    }),
                "month" => rides
                    .GroupBy(r => new { Year = r.CreatedAt.Year, Month = r.CreatedAt.Month })
                    .Select(g => new RideStatusStatisticsDto
                    {
                        TimeLabel = $"{g.Key.Year}-{g.Key.Month:D2}",
                        RejectedCount = g.Count(r => r.Status == StatusRideEnum.Rejected),
                        AcceptedCount = g.Count(r => r.Status == StatusRideEnum.Accepted),
                        CompletedCount = g.Count(r => r.Status == StatusRideEnum.Completed)
                    }),
                _ => rides // Mặc định nhóm theo ngày
                    .GroupBy(r => r.CreatedAt.Date)
                    .Select(g => new RideStatusStatisticsDto
                    {
                        TimeLabel = g.Key.ToString("yyyy-MM-dd"),
                        RejectedCount = g.Count(r => r.Status == StatusRideEnum.Rejected),
                        AcceptedCount = g.Count(r => r.Status == StatusRideEnum.Accepted),
                        CompletedCount = g.Count(r => r.Status == StatusRideEnum.Completed)
                    })
            };

            return groupedRides
                .OrderBy(r => r.TimeLabel)
                .ToList();
        }
        private string FormatDate(DateTime date, string timeRange)
        {
            if (timeRange == "day")
                return date.ToString("yyyy-MM-dd");
            else if (timeRange == "week")
                return $"{date.Year}-{GetWeekOfYear(date)}";
            else
                return date.ToString("yyyy-MM");
        }

        private int GetWeekOfYear(DateTime date)
        {
            var day = (int)date.DayOfWeek;
            var firstDayOfYear = new DateTime(date.Year, 1, 1);
            var dayOfYear = date.DayOfYear;
            return (dayOfYear - 1) / 7 + 1;
        }
    }    
}

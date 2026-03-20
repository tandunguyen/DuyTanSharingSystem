

using Domain.Entities;

namespace Application.Services
{
    public class FriendshipService : IFriendshipService
    {
        private readonly IUserContextService _userContextService;
        private readonly IUserRepository _userRepository;
        private readonly IFriendshipRepository _friendshipRepository;

        // Các hằng số để tính điểm
        private const double MaxInterestScore = 20;
        private const double MutualFriendWeight = 10;
        private const double ActivityWeightRecent = 10;
        private const double ActivityWeightSemiRecent = 5;
        private const double TrustScoreMaxBoost = 10;

        public FriendshipService(
            IUserRepository userRepository,
            IFriendshipRepository friendshipRepository,
            IUserContextService userContextService)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _friendshipRepository = friendshipRepository ?? throw new ArgumentNullException(nameof(friendshipRepository));
            _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
        }

        public async Task<List<FriendSuggestionDto>> GetFriendSuggestionsAsync(int limit = 10, int offset = 0)
        {
            var userId = _userContextService.UserId();
            if (userId == Guid.Empty)
                throw new ArgumentException("ID người dùng không hợp lệ.", nameof(userId));

            // Lấy thông tin người dùng hiện tại (await để đảm bảo bất đồng bộ)
            var currentUser = await _userRepository.GetUserBySuggestFriendAsync(userId);
            if (currentUser == null)
                throw new ArgumentException("Người dùng không tồn tại.", nameof(userId));

            // Lấy danh sách tất cả người dùng đang hoạt động (await để đảm bảo bất đồng bộ)
            var allUsers = await _userRepository.GetUsersWithDetailsAsync();
            var currentUserFriendships = await _friendshipRepository.GetFriendshipsByUserIdAsync(userId);

            // Tính điểm gợi ý
            var suggestions = new List<(User User, double Score)>();
            foreach (var user in allUsers)
            {
                if (user.Id == userId || user.Status != "Active")
                    continue;

                if (IsFriendOrPending(userId, currentUserFriendships, user))
                    continue;

                // Tính điểm tương thích (await cho CalculateLocationScoreAsync)
                double score = CalculateCompatibilityScore(currentUser, user, currentUserFriendships);
                score += CalculateLocationScore(currentUser, user);

                if (score > 0)
                    suggestions.Add((user, score));
            }

            // Sắp xếp và lấy top N
            return suggestions
                  .OrderByDescending(s => s.Score)
                  .Skip(offset)
                  .Take(limit)
                  .Select(s => new FriendSuggestionDto
                  {
                      Id = s.User.Id,
                      FullName = s.User.FullName,
                      ProfilePicture = s.User.ProfilePicture != null ? $"{Constaint.baseUrl}{s.User.ProfilePicture}" : null,
                      TrustScore = s.User.TrustScore,
                      LastActive = s.User.LastActive,
                      CommonInterests = CalculateCommonInterests(currentUser, s.User)
                  })
                  .ToList();
        }

        private double CalculateCompatibilityScore(User currentUser, User otherUser, List<Friendship> currentUserFriendships)
        {
            double score = 0;

            // 1. Sở thích chung
            score += CalculateInterestScore(currentUser, otherUser);

            // 2. Bạn chung
            score += CalculateMutualFriendsScore(currentUser, otherUser, currentUserFriendships);

            // 3. Mức độ hoạt động
            score += CalculateActivityScore(otherUser);

            // 4. Điểm tin cậy
            score += CalculateTrustScoreBoost(otherUser);

            return score;
        }

        private double CalculateInterestScore(User currentUser, User otherUser)
        {
            double score = 0;
            var commonPosts = currentUser.Posts.Intersect(otherUser.Posts).Count();
            var commonLikes = currentUser.Likes.Intersect(otherUser.Likes, new LikeComparer()).Count();
            var commonComments = currentUser.Comments.Intersect(otherUser.Comments).Count();
            var commonRidePosts = currentUser.RidePosts.Intersect(otherUser.RidePosts).Count();

            score += commonPosts * 5; // Trọng số cao cho bài đăng
            score += commonLikes * 3;
            score += commonComments * 2;
            score += commonRidePosts * 4;

            return Math.Min(score, MaxInterestScore);
        }

        private double CalculateMutualFriendsScore(User currentUser, User otherUser, List<Friendship> currentUserFriendships)
        {
            var currentUserFriends = currentUserFriendships
                .Where(f => f.Status == FriendshipStatusEnum.Accepted)
                .Select(f => f.FriendId == currentUser.Id ? f.UserId : f.FriendId)
                .ToList();

            var otherUserFriends = otherUser.SentFriendRequests
                .Where(f => f.Status == FriendshipStatusEnum.Accepted)
                .Select(f => f.FriendId)
                .Union(otherUser.ReceivedFriendRequests
                    .Where(f => f.Status == FriendshipStatusEnum.Accepted)
                    .Select(f => f.UserId))
                .ToList();

            var mutualFriends = currentUserFriends.Intersect(otherUserFriends).Count();
            return mutualFriends * MutualFriendWeight;
        }

        private double CalculateLocationScore(User currentUser, User otherUser)
        {
            var currentLocation = currentUser.LocationUpdates.OrderByDescending(l => l.Timestamp).FirstOrDefault();
            var otherLocation = otherUser.LocationUpdates.OrderByDescending(l => l.Timestamp).FirstOrDefault();

            if (currentLocation == null || otherLocation == null)
                return 0;

            // Công thức Haversine để tính khoảng cách (đơn vị km)
            const double R = 6371; // Bán kính Trái Đất (km)
            double lat1 = currentLocation.Latitude * Math.PI / 180;
            double lat2 = otherLocation.Latitude * Math.PI / 180;
            double deltaLat = (otherLocation.Latitude - currentLocation.Latitude) * Math.PI / 180;
            double deltaLon = (otherLocation.Longitude - currentLocation.Longitude) * Math.PI / 180;

            double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                       Math.Cos(lat1) * Math.Cos(lat2) *
                       Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distance = R * c; // Khoảng cách tính bằng km

            return distance < 10 ? 15 : (distance < 50 ? 10 : 0); // Điểm cao nếu gần (dưới 10km: 15 điểm, dưới 50km: 10 điểm)
        }

        private double CalculateActivityScore(User user)
        {
            if (user.LastActive == null && user.LastLoginDate == null)
                return 0;

            var lastActive = user.LastActive ?? user.LastLoginDate ?? DateTime.UtcNow;
            var timeDiff = (DateTime.UtcNow - lastActive).TotalHours;

            return timeDiff < 24 ? ActivityWeightRecent : (timeDiff < 72 ? ActivityWeightSemiRecent : 0);
        }

        private double CalculateTrustScoreBoost(User user)
        {
            // Ép kiểu user.TrustScore (decimal) về double để đồng nhất kiểu dữ liệu
            double trustScore = (double)user.TrustScore;
            return Math.Min(trustScore / 10, TrustScoreMaxBoost);
        }

        private bool IsFriendOrPending(Guid userId, List<Friendship> friendships, User otherUser)
        {
            return friendships.Any(f =>
                (f.UserId == userId && f.FriendId == otherUser.Id || f.UserId == otherUser.Id && f.FriendId == userId) &&
                (f.Status == FriendshipStatusEnum.Accepted || f.Status == FriendshipStatusEnum.Pending));
        }

        private int CalculateCommonInterests(User currentUser, User otherUser)
        {
            return currentUser.Posts.Intersect(otherUser.Posts).Count() +
                   currentUser.Likes.Intersect(otherUser.Likes, new LikeComparer()).Count() +
                   currentUser.Comments.Intersect(otherUser.Comments).Count() +
                   currentUser.RidePosts.Intersect(otherUser.RidePosts).Count();
        }
    }

    public class LikeComparer : IEqualityComparer<Like?>
    {
        public bool Equals(Like? x, Like? y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return x.PostId == y.PostId;
        }

        public int GetHashCode(Like? obj)
        {
            return obj?.PostId.GetHashCode() ?? 0;
        }
    }
}
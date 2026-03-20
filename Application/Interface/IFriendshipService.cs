
namespace Application.Interface
{
    public interface IFriendshipService
    {
        /// <summary>
        /// Lấy danh sách gợi ý kết bạn cho một người dùng.
        /// </summary>
        /// <param name="limit">Số lượng gợi ý tối đa (mặc định là 10).</param>
        /// <returns>Danh sách gợi ý kết bạn.</returns>
        Task<List<FriendSuggestionDto>> GetFriendSuggestionsAsync(int limit = 10, int offset = 0);
    }
}

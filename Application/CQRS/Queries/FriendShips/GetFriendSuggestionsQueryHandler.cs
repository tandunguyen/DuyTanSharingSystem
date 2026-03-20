

namespace Application.CQRS.Queries.FriendShips
{
    public class GetFriendSuggestionsQueryHandler : IRequestHandler<GetFriendSuggestionsQuery, ResponseModel<List<FriendSuggestionDto>>>
    {
        private readonly IFriendshipService _friendshipService;

        public GetFriendSuggestionsQueryHandler(IFriendshipService friendshipService)
        {
            _friendshipService = friendshipService ?? throw new ArgumentNullException(nameof(friendshipService));
        }

        public async Task<ResponseModel<List<FriendSuggestionDto>>> Handle(GetFriendSuggestionsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Gọi service để lấy danh sách gợi ý kết bạn, truyền cả limit và offset
                var suggestions = await _friendshipService.GetFriendSuggestionsAsync(request.Limit, request.Offset);

                // Điều kiện: Nếu danh sách gợi ý rỗng
                if (suggestions == null || !suggestions.Any())
                {
                    return ResponseFactory.Fail<List<FriendSuggestionDto>>("Không tìm thấy gợi ý kết bạn nào.", 404);
                }

                // Trả về thành công nếu có dữ liệu
                return ResponseFactory.Success(suggestions, "Lấy danh sách gợi ý kết bạn thành công", 200);
            }
            catch (ArgumentException ex)
            {
                return HandleArgumentException(ex);
            }
            catch (UnauthorizedAccessException)
            {
                return ResponseFactory.Fail<List<FriendSuggestionDto>>("Bạn không có quyền truy cập.", 403);
            }
            catch (TimeoutException)
            {
                return ResponseFactory.Fail<List<FriendSuggestionDto>>("Yêu cầu đã hết thời gian chờ.", 504);
            }
            catch (Exception ex)
            {
                return HandleGeneralException(ex);
            }
        }

        private ResponseModel<List<FriendSuggestionDto>> HandleArgumentException(ArgumentException ex)
        {
            if (ex.Message.Contains("ID người dùng không hợp lệ"))
            {
                return ResponseFactory.Fail<List<FriendSuggestionDto>>("ID người dùng không hợp lệ.", 400);
            }
            else if (ex.Message.Contains("Người dùng không tồn tại"))
            {
                return ResponseFactory.Fail<List<FriendSuggestionDto>>("Người dùng không tồn tại.", 404);
            }
            return ResponseFactory.Fail<List<FriendSuggestionDto>>(ex.Message, 400);
        }

        private ResponseModel<List<FriendSuggestionDto>> HandleGeneralException(Exception ex)
        {
            // Ghi log lỗi (giả định có ILogger)
            // _logger.LogError(ex, "Lỗi hệ thống khi lấy gợi ý kết bạn.");
            return ResponseFactory.Fail<List<FriendSuggestionDto>>("Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.", 500);
        }
    }
}
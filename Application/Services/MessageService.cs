using static Domain.Common.Helper;
using Application.CQRS.Commands.Messages;
using Application.DTOs.Message;
using Application.Interface.ContextSerivce;
using Application.Interface.Hubs;
using MediatR;
using Application.DTOs.User;


namespace Application.Services
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IChatService _chatService;
        private readonly IUserRepository _userRepository; // Assuming this exists
        private readonly IUserContextService _userContextService;
        public MessageService(
            IUnitOfWork unitOfWork,
            IChatService chatService,
            IUserRepository userRepository,
            IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _chatService = chatService;
            _userRepository = userRepository;
            _userContextService = userContextService;
        }



        public async Task<List<ConversationDto>> GetConversationsAsync()
        {
            var userId = _userContextService.UserId();
            var conversations = await _unitOfWork.ConversationRepository
                .GetAllConversationsAsync(userId);

            if (conversations == null || !conversations.Any())
            {
                return new List<ConversationDto>(); // Trả về danh sách rỗng nếu không có cuộc trò chuyện
            }

            // Lấy tất cả conversation IDs
            var conversationIds = conversations.Select(c => c.Id).ToList();

            // Tối ưu: Lấy tin nhắn cuối cùng và số tin nhắn chưa đọc trong một truy vấn
            var lastMessagesTask = _unitOfWork.MessageRepository.GetLastMessagesByConversationIdsAsync(conversationIds);
            var unreadCountsTask = _unitOfWork.MessageRepository.GetUnreadCountsByConversationIdsAsync(conversationIds, userId);
            var otherUserIds = conversations.Select(c => c.User1Id == userId ? c.User2Id : c.User1Id).Distinct().ToList();
            var otherUsersTask = _userRepository.GetUsersByIdsAsync(otherUserIds);

            // Chờ tất cả các tác vụ hoàn thành
            await Task.WhenAll(lastMessagesTask, unreadCountsTask, otherUsersTask);

            var lastMessages = lastMessagesTask.Result;
            var unreadCounts = unreadCountsTask.Result;
            var otherUsers = otherUsersTask.Result.ToDictionary(u => u.Id);

            var conversationDtos = conversations.Select(conv =>
            {
                var otherUserId = conv.User1Id == userId ? conv.User2Id : conv.User1Id;
                var lastMessage = lastMessages.FirstOrDefault(m => m.ConversationId == conv.Id);

                return new ConversationDto
                {
                    Id = conv.Id,
                    OtherUserId = otherUserId,
                    OtherUserName = otherUsers.ContainsKey(otherUserId) ? otherUsers[otherUserId].FullName : "Unknown User",
                    CreatedAt = conv.CreatedAt,
                    LastMessage = lastMessage != null ? new MessageDto
                    {
                        Id = lastMessage.Id,
                        ConversationId = lastMessage.ConversationId,
                        SenderId = lastMessage.SenderId,
                        Content = lastMessage.Content,
                        SentAt = FormatUtcToLocal(lastMessage.SentAt),
                        IsSeen = lastMessage.IsSeen,
                        SeenAt = lastMessage.SeenAt.HasValue ? FormatUtcToLocal(lastMessage.SeenAt.Value) : null
                    } : null,
                    UnreadCount = unreadCounts.ContainsKey(conv.Id) ? unreadCounts[conv.Id] : 0
                };
            }).OrderByDescending(c =>
                c.LastMessage != null
                    ? DateTime.Parse(c.LastMessage.SentAt ?? FormatUtcToLocal(DateTime.UtcNow))
                    : c.CreatedAt
                ).ToList();


            return conversationDtos;
        }

        public async Task<MessageListDto> GetMessagesAsync(Guid conversationId,Guid userId,Guid? lastMessageId = null,int pageSize = 10)
        {
            var messages = await _unitOfWork.MessageRepository
                .GetMessagesByConversationAsync(conversationId, 1, pageSize, lastMessageId);

            var messageDtos = messages.Select(m => new MessageDto
            {
                Id = m.Id,
                ConversationId = m.ConversationId,
                SenderId = m.SenderId,
                ReceiverId = m.Conversation.User1Id,
                Content = m.Content,
                SentAt =FormatUtcToLocal( m.SentAt),
                IsSeen = m.IsSeen,
                SeenAt =FormatUtcToLocal( m.SeenAt ?? DateTime.UtcNow),
                Status = m.Status.ToString(),
                DeliveredAt =FormatUtcToLocal(m.DeliveredAt??DateTime.UtcNow)
            }).ToList();

            var nextCursor = messageDtos.Count() == pageSize
                ? messageDtos.Last().Id
                : (Guid?)null;

            return new MessageListDto { Messages = messageDtos, NextCursor = nextCursor };

        }

        public async Task<ListInBoxDto> GetListInBoxAsync(Guid? cursor, int pageSize)
        {
            var currentUserId = _userContextService.UserId();
            if (currentUserId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User not authenticated or invalid user ID.");
            }

            if (pageSize <= 0) pageSize = Constaint.DefaultPageSize;
            if (pageSize > Constaint.MaxPageSize) pageSize = Constaint.MaxPageSize;

            // === Lấy pageSize + 1 items cho tin nhắn ===
            int itemsToFetch = pageSize + 1;

            // === Gọi phương thức Repository để lấy tin nhắn ===
            var latestMessages = await _unitOfWork.MessageRepository.GetLatestMessagesForInboxAsync(currentUserId, cursor, itemsToFetch);

            // === Lấy tất cả bạn bè chưa có hội thoại ===
            var friendsWithoutConversation = await _unitOfWork.MessageRepository.GetFriendsWithoutConversationAsync(currentUserId);

            var resultDto = new ListInBoxDto();
            var inboxDtos = new List<InBoxDto>();

            // === Xử lý tin nhắn ===
            bool hasNextPage = latestMessages.Count == itemsToFetch;
            var messagesForPage = latestMessages.Take(pageSize).ToList();

            foreach (var message in messagesForPage)
            {
                var otherUser = (message.Conversation.User1Id == currentUserId)
                        ? message.Conversation.User2
                        : message.Conversation.User1;
                if (otherUser == null)
                {
                    continue;
                }

                bool isLastMessageSeenByCurrentUser = message.SenderId != currentUserId ? message.IsSeen : true;

                int unreadCount = await _unitOfWork.MessageRepository.GetUnreadMessageCountAsync(message.ConversationId, currentUserId);

                var userDto = new UserDto
                {
                    Id = otherUser.Id,
                    FullName = otherUser.FullName,
                    ProfilePicture = otherUser.ProfilePicture ?? string.Empty
                };
                inboxDtos.Add(new InBoxDto
                {
                    User = userDto,
                    ConversationId = message.ConversationId,
                    LastMessage = message.Content,
                    LastMessageDate = message.SentAt,
                    UnreadCount = unreadCount,
                    IsSeen = isLastMessageSeenByCurrentUser
                });
            }

            // === Thêm tất cả bạn bè chưa có hội thoại ===
            var friendsToAdd = friendsWithoutConversation
                .Select(f => new InBoxDto
                {
                    User = new UserDto
                    {
                        Id = f.Friend.Id,
                        FullName = f.Friend.FullName,
                        ProfilePicture = f.Friend.ProfilePicture ?? string.Empty
                    },
                    ConversationId = Guid.Empty,
                    LastMessage = $"Bạn và {f.Friend.FullName} hiện đã là bạn",
                    LastMessageDate = f.CreatedAt, // Sử dụng thời gian kết bạn
                    UnreadCount = 0,
                    IsSeen = true
                });

            inboxDtos.AddRange(friendsToAdd);

            // === Sắp xếp danh sách ===
            inboxDtos = inboxDtos
                .OrderBy(dto => dto.UnreadCount > 0 ? 0 : 1) // Ưu tiên tin nhắn chưa đọc
                .ThenByDescending(dto => dto.LastMessageDate) // Sắp xếp theo thời gian (tin nhắn hoặc kết bạn)
                .ToList();

            resultDto.InBox = inboxDtos;

            // === Xác định NextCursor ===
            if (hasNextPage && messagesForPage.Any())
            {
                resultDto.NextCursor = messagesForPage.Last().Id;
            }
            else
            {
                resultDto.NextCursor = Guid.Empty;
            }

            return resultDto;
        }
    }
}

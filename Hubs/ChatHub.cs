using Microsoft.AspNetCore.SignalR;
using Bloomy.Services.Interfaces;
using System.Security.Claims;

namespace Bloomy.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatHub> _logger;

        // Mapping: userId -> connectionId
        private static Dictionary<string, string> _userConnections = new();

        public ChatHub(IChatService chatService, ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections[userId] = Context.ConnectionId;
                _logger.LogInformation($"User {userId} connected. ConnectionId: {Context.ConnectionId}");
                
                // Notify other users that this user is online
                await Clients.Others.SendAsync("UserOnline", userId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections.Remove(userId);
                _logger.LogInformation($"User {userId} disconnected.");
                
                // Notify other users that this user is offline
                await Clients.Others.SendAsync("UserOffline", userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Gửi tin nhắn text đến một cuộc trò chuyện
        /// </summary>
        public async Task SendMessage(string conversationId, string message)
        {
            var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(senderId))
            {
                _logger.LogWarning($"SendMessage called without authentication. ConnectionId: {Context.ConnectionId}");
                await Clients.Caller.SendAsync("Error", "Bạn chưa đăng nhập");
                return;
            }
            
            if (string.IsNullOrEmpty(message))
            {
                await Clients.Caller.SendAsync("Error", "Tin nhắn không được để trống");
                return;
            }

            try
            {
                var chatMessage = await _chatService.SendMessageAsync(
                    conversationId: Guid.Parse(conversationId),
                    senderId: Guid.Parse(senderId),
                    message: message
                );

                // Broadcast message to all users in this conversation
                await Clients.Group(conversationId).SendAsync("ReceiveMessage", new
                {
                    id = chatMessage.Id,
                    conversationId = chatMessage.ConversationId,
                    senderId = chatMessage.SenderId,
                    senderName = chatMessage.Sender?.FullName ?? "Unknown",
                    message = chatMessage.Message,
                    imageUrl = chatMessage.ImageUrl,
                    sentAt = chatMessage.SentAt,
                    isRead = chatMessage.IsRead
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                await Clients.Caller.SendAsync("Error", "Failed to send message");
            }
        }

        /// <summary>
        /// Gửi tin nhắn có ảnh
        /// </summary>
        public async Task SendMessageWithImage(string conversationId, string message, string imageUrl)
        {
            var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(senderId))
            {
                _logger.LogWarning($"SendMessageWithImage called without authentication. ConnectionId: {Context.ConnectionId}");
                await Clients.Caller.SendAsync("Error", "Bạn chưa đăng nhập");
                return;
            }

            try
            {
                var chatMessage = await _chatService.SendMessageAsync(
                    conversationId: Guid.Parse(conversationId),
                    senderId: Guid.Parse(senderId),
                    message: message,
                    imageUrl: imageUrl
                );

                await Clients.Group(conversationId).SendAsync("ReceiveMessage", new
                {
                    id = chatMessage.Id,
                    conversationId = chatMessage.ConversationId,
                    senderId = chatMessage.SenderId,
                    senderName = chatMessage.Sender?.FullName ?? "Unknown",
                    message = chatMessage.Message,
                    imageUrl = chatMessage.ImageUrl,
                    sentAt = chatMessage.SentAt,
                    isRead = chatMessage.IsRead
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message with image");
                await Clients.Caller.SendAsync("Error", "Failed to send message");
            }
        }

        /// <summary>
        /// Tham gia một cuộc trò chuyện (group)
        /// </summary>
        public async Task JoinConversation(string conversationId)
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
                _logger.LogInformation($"User joined conversation: {conversationId}");
                
                // Thông báo cho các user khác trong conversation
                await Clients.Group(conversationId).SendAsync("UserJoinedConversation", 
                    Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining conversation");
            }
        }

        /// <summary>
        /// Rời khỏi một cuộc trò chuyện (group)
        /// </summary>
        public async Task LeaveConversation(string conversationId)
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
                _logger.LogInformation($"User left conversation: {conversationId}");
                
                // Thông báo cho các user khác trong conversation
                await Clients.Group(conversationId).SendAsync("UserLeftConversation", 
                    Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving conversation");
            }
        }

        /// <summary>
        /// Đánh dấu tin nhắn là đã đọc
        /// </summary>
        public async Task MarkMessageAsRead(string messageId)
        {
            try
            {
                await _chatService.MarkMessageAsReadAsync(Guid.Parse(messageId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking message as read");
            }
        }

        /// <summary>
        /// Gửi thông báo gõ phím (typing indicator)
        /// </summary>
        public async Task SendTypingIndicator(string conversationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await Clients.Group(conversationId).SendAsync("UserTyping", userId);
        }

        /// <summary>
        /// Gửi thông báo dừng gõ phím
        /// </summary>
        public async Task SendStopTypingIndicator(string conversationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await Clients.Group(conversationId).SendAsync("UserStoppedTyping", userId);
        }
    }
}

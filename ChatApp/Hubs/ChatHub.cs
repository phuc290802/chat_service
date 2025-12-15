using ChatApp.Application.DTOs;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace ChatApp.API.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<Guid, UserConnection> _connections = new();
        private readonly IMessageService _messageService;
        private readonly IConversationService _conversationService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IMessageService messageService, IConversationService conversationService, ILogger<ChatHub> logger)
        {
            _messageService = messageService;
            _conversationService = conversationService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserIdFromContext();
            var connectionId = Context.ConnectionId;

            var userConnection = new UserConnection
            {
                UserId = userId,
                ConnectionId = connectionId,
                ConnectedAt = DateTime.UtcNow,
            };

            _connections[userId] = userConnection;

            var userConversations = await _conversationService.GetUserConversationsAsync(userId);
            foreach (var conv in userConversations)
            {
                await Groups.AddToGroupAsync(connectionId, $"conversation_{conv.Id}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = GetUserIdFromContext();
            _connections.TryRemove(userId, out _);

            var userConversations = await _conversationService.GetUserConversationsAsync(userId);
            foreach (var conv in userConversations)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conv.Id}");
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessageToConversation(SendMessageRequest request)
        {
            var senderId = GetUserIdFromContext();

            var isMember = await _conversationService.IsUserInConversationAsync(request.ConversationId, senderId);

            if (!isMember)
                throw new HubException("You are not a member of this conversation");
            var message = new Message
            {
                ConversationId = request.ConversationId,
                SenderId = senderId,
                Content = request.Content,
                MessageType = "text",
                CreatedAt = DateTime.UtcNow,
            };

            var savedMessage = await _messageService.SaveMessageAsync(message);

            await Clients.Group($"conversation_{request.ConversationId}")
                .SendAsync("ReceiveMessage", new MessageDto
                {
                    Id = savedMessage.Id,
                    ConversationId = savedMessage.ConversationId,
                    SenderId = savedMessage.SenderId,
                    Content = savedMessage.Content,
                    MessageType = savedMessage.MessageType,
                    CreatedAt = DateTime.UtcNow
                });
            await Clients.Caller.SendAsync("MessageSent", new { MessageId = savedMessage.Id });
        }

        public async Task JoinConverstation(Guid conversationId)
        {
            var userId = GetUserIdFromContext();

            var isMember = await _conversationService.IsUserInConversationAsync(conversationId, userId);

            if (!isMember)
                throw new HubException("You are not a member of this conversation");

            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
            await Clients.Caller.SendAsync("JoinedConversation", conversationId);
        }

        public async Task<List<string>> GetOnlineUsersInConversation(Guid conversationId)
        {
            var members = await _conversationService.GetConversationMembersAsync(conversationId);

            var onlineUsers = members
                .Where(m => _connections.ContainsKey(m.UserId))
                .Select(m => m.UserId.ToString())
                .ToList();

            return onlineUsers;
        }


        private Guid GetUserIdFromContext()
        {
            var userIdClaim = Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            _logger.LogInformation($"User ID Claim: {userIdClaim}");

            if (string.IsNullOrEmpty(userIdClaim))
            {
                _logger.LogError("Missing claim");
                throw new HubException("User not authenticated - No user ID claim");
            }
            return Guid.Parse(userIdClaim);
        }
    }
}

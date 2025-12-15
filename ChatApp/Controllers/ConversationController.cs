using ChatApp.API.Hubs;
using ChatApp.Application.DTOs;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ChatApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        private readonly IConversationService _conversationService;
        private readonly IUserConnectionService _userConnectionService;
        private readonly IMessageService _messageService;
        private readonly IHubContext<ChatHub> _hubContext;

        public ConversationController(
            IConversationService conversationService,
            IUserConnectionService userConnectionService,
            IMessageService messageService,
            IHubContext<ChatHub> hubContext
            )
        {
            _conversationService = conversationService;
            _userConnectionService = userConnectionService;
            _messageService = messageService;
            _hubContext = hubContext;
        }

        [Authorize]
        [HttpPost("new")]
        public async Task<IActionResult> AddFriend(Guid user2)
        {
            var user1 = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            Console.WriteLine("1111111111111111111111111111111111111111111111", user1);
            var conversation = await _conversationService.GetOrCreateDirectConversationAsync(Guid.Parse(user1), user2);
            return Ok(conversation);
        }

        [Authorize]
        [HttpGet("{conversationId}/online-users")]
        public async Task<IActionResult> GetOnlineUsersInConversation(Guid conversationId)
        {
            var members = await _conversationService.GetConversationMembersAsync(conversationId);
            var onlineUsers = _userConnectionService
                .GetOnlineUsers(members.Select(m => m.UserId).ToList())
                .Select(id => id.ToString())
                .ToList();

            return Ok(onlineUsers);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetConverstationByUserID(Guid userId)
        {
            var converstation = await _conversationService.GetConverstationByUserId(userId);
            return Ok(converstation);
        }

        [HttpGet("message/{conversationId}")]
        public async Task<IActionResult> GetMessgaes(Guid conversationId)
        {
            var messages = await _messageService.GetConversationMessagesAsync(conversationId);
            return Ok(messages);
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest req)
        {
            var message = await _messageService.SendMessageAsync(req);
            await _hubContext.Clients.Group($"conversation_{req.ConversationId}")
                .SendAsync("ReceiveMessage", new
                {
                    Id = message.Id,
                    Content = message.Content,
                    CreatedAt = message.CreatedAt,
                    SenderId = message.SenderId,
                    MessageType = message.MessageType,
                    ConversationId = message.ConversationId,
                    SeenAt = message.SeenAt
                });
            return Ok(message);
        }
    }
}

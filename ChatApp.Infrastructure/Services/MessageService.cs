using Azure.Core;
using ChatApp.Application.DTOs;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Infrastructure.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepo;
        private readonly IConversationRepository _conversationRepository;

        public MessageService(IMessageRepository message, IConversationRepository conversationRepository)
        {
            _messageRepo = message;
            _conversationRepository = conversationRepository;
        }

        public async Task<Message> SaveMessageAsync(Message message)
        {
            message.Id = Guid.NewGuid();
            return await _messageRepo.AddAsync(message);
        }

        public async Task<List<MessageDto>> GetConversationMessagesAsync(Guid conversationId, int page = 1, int pageSize = 50)
        {
            var messageList = await _messageRepo.GetConversationMessagesAsync(conversationId, page, pageSize);

            var dtoList = messageList.Select(message => new MessageDto
            {
                Id = message.Id,
                Content = message.Content,
                CreatedAt = message.CreatedAt,
                SenderId = message.SenderId,
                MessageType = message.MessageType,
                ConversationId = message.ConversationId,
                SeenAt = message.SeenAt
            }).ToList();

            return dtoList;
        }


        public async Task MarkMessageAsSeenAsync(Guid messageId, Guid viewerId)
        {
            var message = await _messageRepo.GetAsync(messageId);
            if (message != null && message.SenderId != viewerId)
            {
                message.SeenAt = DateTime.UtcNow;
                await _messageRepo.UpdateAsync(message);
            }
        }

        public async Task<Message> GetMessageAsync(Guid messageId)
        {
            return await _messageRepo.GetAsync(messageId);
        }

        public async Task<MessageDto> SendMessageAsync(SendMessageRequest req)
        {
            var converstation = await _conversationRepository.GetByIdAsync(req.ConversationId);
            if (converstation == null)
                throw new InvalidOperationException("Converstation not found");

            var isMember = converstation.Members.Any(m => m.UserId == req.SenderId);
            Console.WriteLine("isMemberisMemberisMember", isMember);
            if (!isMember)
                throw new UnauthorizedAccessException("User is not member");

            var message = new Message
            {
                ConversationId = req.ConversationId,
                SenderId = req.SenderId,
                Content = req.Content,
                MessageType = "text",
                CreatedAt = DateTime.UtcNow,
            };

            await _messageRepo.AddAsync(message);

            return new MessageDto
            {
                Id = message.Id,
                ConversationId = message.ConversationId,
                SenderId = message.SenderId,
                Content = message.Content,
                MessageType = message.MessageType,
                CreatedAt = DateTime.UtcNow,
                SeenAt = null
            };
        }
    }
}

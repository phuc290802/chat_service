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

        public MessageService(IMessageRepository message)
        {
            _messageRepo = message;
        }

        public async Task<Message> SaveMessageAsync(Message message)
        {
            message.Id = Guid.NewGuid();
            return await _messageRepo.AddAsync(message);
        }

        public async Task<List<Message>> GetConversationMessagesAsync(Guid conversationId, int page = 1, int pageSize = 50)
        {
            return await _messageRepo.GetConversationMessagesAsync(conversationId, page, pageSize);
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
    }
}

using ChatApp.Application.DTOs;
using ChatApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Interfaces
{
    public interface IMessageRepository
    {
        Task<Message> AddAsync(Message message);
        Task<Message> GetAsync(Guid messageId);
        Task<List<Message>> GetConversationMessagesAsync(Guid conversationId, int page = 1, int pageSize = 50);
        Task UpdateAsync(Message message);
        Task MarkMessageAsSeenAsync(Guid messageId, Guid viewerId);
        Task<List<Message>> GetUnreadMessagesAsync(Guid userId, Guid conversationId);
        Task<int> GetUnreadMessageCountAsync(Guid userId, Guid? conversationId = null);
    }
}

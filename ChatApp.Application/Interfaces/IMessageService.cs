using ChatApp.Domain.Entities;

namespace ChatApp.Application.Interfaces
{
    public interface IMessageService
    {
        Task<Message> SaveMessageAsync(Message message);
        Task<List<Message>> GetConversationMessagesAsync(Guid conversationId, int page = 1, int pageSize = 50);
        Task<Message> GetMessageAsync(Guid messageId);
        Task MarkMessageAsSeenAsync(Guid messageId, Guid viewerId);
    }
}

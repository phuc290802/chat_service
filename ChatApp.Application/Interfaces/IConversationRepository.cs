using ChatApp.Domain.Entities;

namespace ChatApp.Application.Interfaces
{
    public interface IConversationRepository
    {
        Task<Conversation> GetDirectConversationAsync(Guid user1Id, Guid user2Id);
        Task<Conversation> CreateConversationAsync(Conversation conversation);
        Task AddMembersToConversationAsync(List<ConversationMember> members);
        Task<bool> IsUserInConversationAsync(Guid conversationId, Guid userId);
        Task<List<Conversation>> GetUserConversationsAsync(Guid userId);
        Task<List<ConversationMember>> GetConversationMembersAsync(Guid conversationId);
        Task <Conversation> GetByIdAsync(Guid conversationId, CancellationToken ct = default);
    }
}

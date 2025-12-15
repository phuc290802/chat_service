using ChatApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Interfaces
{
    public interface IConversationService
    {
        Task<Conversation> GetOrCreateDirectConversationAsync(Guid user1Id, Guid user2Id);
        Task<bool> IsUserInConversationAsync(Guid conversationId, Guid userId);
        Task<List<Conversation>> GetUserConversationsAsync(Guid userId);
        Task<List<ConversationMember>> GetConversationMembersAsync(Guid conversationId);
        Task<Conversation> CreateGroupConversationAsync(Guid creatorId, string name, List<Guid> memberIds);
    }
}

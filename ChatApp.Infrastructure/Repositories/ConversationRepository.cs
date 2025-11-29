using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Infrastructure.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly AppDbContext _context;

        public ConversationRepository(AppDbContext context) { 
            _context = context;
        }

        public async Task<Conversation> GetDirectConversationAsync(Guid user1Id, Guid user2Id)
        {
            return await _context.Conversations
                .FromSql($@"
                SELECT c.* FROM Conversations c
                INNER JOIN ConversationMembers cm1 ON c.Id = cm1.ConversationId
                INNER JOIN ConversationMembers cm2 ON c.Id = cm2.ConversationId
                WHERE c.IsGroup = 0 
                AND cm1.UserId = {user1Id} 
                AND cm2.UserId = {user2Id}
                AND (SELECT COUNT(*) FROM ConversationMembers WHERE ConversationId = c.Id) = 2")
                .FirstOrDefaultAsync();
        }

        public async Task<Conversation> CreateConversationAsync(Conversation conversation)
        {
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();
            return conversation;
        }

        public async Task AddMembersToConversationAsync(List<ConversationMember> members)
        {
            _context.ConversationMembers.AddRange(members);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsUserInConversationAsync(Guid conversationId, Guid userId)
        {
            return await _context.ConversationMembers
                .AnyAsync(cm => cm.ConversationId == conversationId && cm.UserId == userId);
        }

        public async Task<List<Conversation>> GetUserConversationsAsync(Guid userId)
        {
            return await _context.ConversationMembers
                .Where(cm => cm.UserId == userId)
                .Include(cm => cm.Conversation)
                .Select(cm => cm.Conversation)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<ConversationMember>> GetConversationMembersAsync(Guid conversationId)
        {
            return await _context.ConversationMembers
                .Where(cm => cm.ConversationId == conversationId)
                .Include(cm => cm.User)
                .ToListAsync();
        }
    }
}

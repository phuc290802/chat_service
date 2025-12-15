using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Infrastructure.Services
{
    public  class ConversationService : IConversationService
    {
        private readonly IConversationRepository _conversationRepo;

        public ConversationService(IConversationRepository conversationRepo)
        {
            _conversationRepo = conversationRepo;
        }

        public async Task<Conversation> GetOrCreateDirectConversationAsync(Guid user1Id, Guid user2Id)
        {
            var existingConversation = await _conversationRepo.GetDirectConversationAsync(user1Id, user2Id);
            if (existingConversation != null)
                return existingConversation;

            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                Name = null,
                IsGroup = false,
                CreatedBy = user1Id,
                CreatedAt = DateTime.UtcNow
            };

            var createdConversation = await _conversationRepo.CreateConversationAsync(conversation);

            var members = new List<ConversationMember>
        {
            new() { ConversationId = createdConversation.Id, UserId = user1Id, Role = "Member" },
            new() { ConversationId = createdConversation.Id, UserId = user2Id, Role = "Member" }
        };

            await _conversationRepo.AddMembersToConversationAsync(members);

            return createdConversation;
        }

        public async Task<bool> IsUserInConversationAsync(Guid conversationId, Guid userId)
        {
            return await _conversationRepo.IsUserInConversationAsync(conversationId, userId);
        }

        public async Task<List<Conversation>> GetUserConversationsAsync(Guid userId)
        {
            return await _conversationRepo.GetUserConversationsAsync(userId);
        }

        public async Task<List<ConversationMember>> GetConversationMembersAsync(Guid conversationId)
        {
            return await _conversationRepo.GetConversationMembersAsync(conversationId);
        }

        public async Task<Conversation> CreateGroupConversationAsync(Guid creatorId, string name, List<Guid> memberIds)
        {
            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                Name = name,
                IsGroup = true,
                CreatedBy = creatorId,
                CreatedAt = DateTime.UtcNow
            };

            var createdConversation = await _conversationRepo.CreateConversationAsync(conversation);

            var members = memberIds.Select(userId => new ConversationMember
            {
                ConversationId = createdConversation.Id,
                UserId = userId,
                Role = userId == creatorId ? "Owner" : "Member"
            }).ToList();

            await _conversationRepo.AddMembersToConversationAsync(members);

            return createdConversation;
        }
    }
}

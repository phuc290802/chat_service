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
    public  class ConversationService : IConversationService
    {
        private readonly IConversationRepository _conversationRepo;
        private readonly IUserService _userService;

        public ConversationService(IConversationRepository conversationRepo, IUserService userService)
        {
            _conversationRepo = conversationRepo;
            _userService = userService;
        }

        public async Task<ConversationDto> GetOrCreateDirectConversationAsync(Guid user1Id, Guid user2Id)
        {
            var existingConversation = await _conversationRepo.GetDirectConversationAsync(user1Id, user2Id);
            if (existingConversation != null)
                return new ConversationDto
                {
                    Id = existingConversation.Id,
                    Name = existingConversation.Name,
                    IsGroup = existingConversation.IsGroup,
                    Members = existingConversation.Members.Select(m => new ConversationMemberDto
                    {
                        UserId = m.UserId,
                        Role = m.Role
                    })
                };

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

            return new ConversationDto
            {
                Id = conversation.Id,
                Name = conversation.Name,
                IsGroup = conversation.IsGroup,
                Members = conversation.Members.Select(m => new ConversationMemberDto
                {
                    UserId = m.UserId,
                    Role = m.Role
                })
            };
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

        public async Task<List<ConverstationRespone>> GetConverstationByUserId(Guid userId)
        {
            var listConversations = await _conversationRepo.GetUserConversationsAsync(userId);

            var list = listConversations.Select(conv =>
                new ConverstationRespone(
                    id: conv.Id,
                    name: conv.Name ?? "",
                    isGroup: conv.IsGroup,
                    isDirectMessage: !conv.IsGroup,
                    createdBy: null,
                    createdAt: conv.CreatedAt,
                    lastMessage: null,
                    avatarUrl: "",
                    members: conv.Members.Select(m => new MemberConverstationRespone(
                        conversationId: conv.Id,
                        joinedAt: m.JoinedAt,
                        user: _userService.ResponeUserDto(m.User),
                        role: m.Role
                    )).ToList(),
                    unreadCount: 0
                )
            ).ToList();

            return list;
        }
    }
}

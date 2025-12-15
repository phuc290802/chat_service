using ChatApp.Application.DTOs;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserConnectionService _userConnectionService;
        private readonly IConversationRepository _conversationRepository;

        public UserService(IUserRepository userRepository, IUserConnectionService userConnectionService, IConversationRepository conversationRepository)
        {
            _userRepository = userRepository;
            _userConnectionService = userConnectionService;
            _conversationRepository = conversationRepository;
        }

        public async Task<List<UserDto>> GetAllUser(CancellationToken ct = default)
        {
            var users = await _userRepository.GetAllUser(ct);

            var userDtos = users.Select(u => new UserDto(
                Id: u.Id,
                UserName: u.UserName,
                Email: u.Email,
                DisplayName: u.DisplayName,
                AvatarUrl: u.AvatarUrl,
                isOnline: IsUserOnline(u.Id)
                )).ToList();

            return userDtos;
        }

        public UserDto ResponeUserDto(User user, CancellationToken ct = default)
        {
            var userDtos = new UserDto(
                Id: user.Id,
                UserName: user.UserName,
                Email: user.Email,
                DisplayName: user.DisplayName,
                AvatarUrl: user.AvatarUrl,
                isOnline: IsUserOnline(user.Id)
                );

            return userDtos;
        }

        public async Task CreateConversationsForNewUserAsync(User newUser, CancellationToken ct = default)
        {
            var allUsers = await _userRepository.GetAllUser();
            var otherUsers = allUsers.Where(u => u.Id != newUser.Id).ToList();

            foreach (var user in otherUsers)
            {
                var existing = await _conversationRepository
                    .GetDirectConversationAsync(newUser.Id, user.Id);

                if (existing != null)
                    continue;

                var conversation = new Conversation
                {
                    Id = Guid.NewGuid(),
                    Name = null,
                    CreatedBy = newUser.Id,
                    IsGroup = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _conversationRepository.CreateConversationAsync(conversation);

                var members = new[]
                {
                    new ConversationMember { ConversationId = conversation.Id, UserId = newUser.Id, JoinedAt = DateTime.UtcNow },
                    new ConversationMember { ConversationId = conversation.Id, UserId = user.Id, JoinedAt = DateTime.UtcNow }
                };

                await _conversationRepository.AddMembersToConversationAsync(members.ToList());
            }
        }

        private bool IsUserOnline(Guid id)
        {
            var onlineUsers = _userConnectionService.GetAllOnlineUsers();
            return onlineUsers.Contains(id);
        }

    }
}

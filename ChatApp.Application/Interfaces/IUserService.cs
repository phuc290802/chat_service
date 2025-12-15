using ChatApp.Application.DTOs;
using ChatApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUser(CancellationToken ct = default);
        UserDto ResponeUserDto(User user, CancellationToken ct = default);
        Task CreateConversationsForNewUserAsync(User newUser, CancellationToken ct = default);
    }
}

using ChatApp.Application.DTOs;

namespace ChatApp.Application.Interfaces;

public interface IAuthService
{
    Task<UserDto> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
}
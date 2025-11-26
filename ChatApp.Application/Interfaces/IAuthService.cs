using ChatApp.Application.DTOs;

namespace ChatApp.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
        Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
        Task<RefreshTokenResult> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    }
}

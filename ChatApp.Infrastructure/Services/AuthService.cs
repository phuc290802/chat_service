using Azure.Core;
using ChatApp.Application.DTOs;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;

namespace ChatApp.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;
        private readonly IConversationRepository _conversationRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            ITokenService tokenService,
            IPasswordHasher<User> passwordHasher,
            IRefreshTokenRepository refreshTokenRepository,
            ILogger<AuthService> logger,
            IConversationRepository conversationRepository,
            IUserService userService
            )
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            _refreshTokenRepository = refreshTokenRepository;
            _logger = logger;
            _conversationRepository = conversationRepository;
            _userService = userService;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var userName = request.UserName.Trim();

            if (await _userRepository.GetByEmailAsync(email, ct) is not null)
                throw new InvalidOperationException("Email already registered");

            if (await _userRepository.GetByUserNameAsync(userName, ct) is not null)
                throw new InvalidOperationException("UserName is already taken");

            var user = new User
            {
                UserName = userName,
                DisplayName = string.IsNullOrWhiteSpace(request.Displayname) ? userName : request.Displayname,
                Email = email,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken(user);

            await SaveRefreshTokenAsync(user, refreshToken, ct);

            await _userService.CreateConversationsForNewUserAsync(user);

            return new AuthResponse(accessToken, refreshToken, new UserDto(user.Id, user.UserName, user.Email, user.DisplayName, user.AvatarUrl));
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct)
                       ?? throw new InvalidOperationException("Invalid credentials");

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
                throw new InvalidOperationException("Invalid credentials");

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken(user);

            await SaveRefreshTokenAsync(user, refreshToken, ct);

            return new AuthResponse(accessToken, refreshToken, new UserDto(user.Id, user.UserName, user.Email, user.DisplayName, user.AvatarUrl));
        }

        public async Task<RefreshTokenResult> RefreshTokenAsync(string refreshToken, CancellationToken ct)
        {
            var tokenEntity = await _refreshTokenRepository.GetValidTokenAsync(refreshToken);

            if (tokenEntity == null)
            {
                _logger.LogWarning("Invalid refresh token attempt: {Token}",
                    refreshToken.Substring(0, 10) + "...");
                return RefreshTokenResult.Fail("Invalid token");
            }

            if (tokenEntity.ExpiresAt <= DateTime.UtcNow)
            {
                _logger.LogWarning("Expired refresh token used for user {UserId}",
                    tokenEntity.UserId);
                return RefreshTokenResult.Fail("Token expired");
            }

            var user = await _userRepository.GetByIdAsync(tokenEntity.UserId);
            if (user == null)
            {
                _logger.LogError("User not found for valid refresh token. UserId: {UserId}, Token: {TokenId}",
                    tokenEntity.UserId, tokenEntity.Id);
                return RefreshTokenResult.Fail("User not found");
            }

            _logger.LogInformation("Refreshing tokens for user {UserId}", user.Id);

            await _refreshTokenRepository.RemoveOldTokenAsync(refreshToken);

            try
            {
                await _refreshTokenRepository.RemoveOldTokenAsync(refreshToken);

                var newAccessToken = _tokenService.GenerateAccessToken(user);
                var newRefreshToken = _tokenService.GenerateRefreshToken(user);

                await SaveRefreshTokenAsync(user, newRefreshToken, ct);

                _logger.LogInformation("Tokens refreshed successfully for user {UserId}", user.Id);

                return RefreshTokenResult.Success(newAccessToken, newRefreshToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh tokens for user {UserId}", user.Id);
                throw;
            }
        }

        private async Task SaveRefreshTokenAsync(User user, string refreshToken, CancellationToken ct = default)
        {
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepository.AddAsync(refreshTokenEntity, ct);
            await _refreshTokenRepository.SaveChangesAsync(ct);
        }
    }
}

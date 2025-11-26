using ChatApp.Application.DTOs;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace ChatApp.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthService(
            IUserRepository userRepository,
            ITokenService tokenService,
            IPasswordHasher<User> passwordHasher,
            IRefreshTokenRepository refreshTokenRepository
            )
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            _refreshTokenRepository = refreshTokenRepository;
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

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepository.AddAsync(refreshTokenEntity, ct);
            await _refreshTokenRepository.SaveChangesAsync(ct);

            return new AuthResponse(accessToken, refreshToken, new UserDto(user.Id, user.UserName, user.Email, user.DisplayName, user.AvatarUrl, user.CreatedAt));
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

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepository.AddAsync(refreshTokenEntity, ct);
            await _refreshTokenRepository.SaveChangesAsync(ct);

            return new AuthResponse(accessToken, refreshToken, new UserDto(user.Id, user.UserName, user.Email, user.DisplayName, user.AvatarUrl, user.CreatedAt));
        }

        public async Task<RefreshTokenResult> RefreshTokenAsync(string refreshToken, CancellationToken ct)
        {
            var tokenEntity = await _refreshTokenRepository.GetValidTokenAsync(refreshToken);

            if (tokenEntity == null)
                return RefreshTokenResult.Fail("Invalid token");

            var user = await _userRepository.GetByIdAsync(tokenEntity.UserId);
            if (user == null)
                return RefreshTokenResult.Fail("User not found");

            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken(user);

            var refreshTokenEntity = new RefreshToken
            {
                Token = newRefreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepository.AddAsync(refreshTokenEntity, ct);
            await _refreshTokenRepository.SaveChangesAsync(ct);

            return RefreshTokenResult.Success(newAccessToken, newRefreshToken);
        }
    }
}

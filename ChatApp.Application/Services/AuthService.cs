using ChatApp.Application.DTOs;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;


namespace ChatApp.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthService(IUserRepository userRepository, IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
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

        return new UserDto(user.Id, user.UserName, user.Email, user.DisplayName, user.AvatarUrl, user.CreatedAt);
    }
}
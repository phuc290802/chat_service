namespace ChatApp.Application.DTOs;

public record UserDto(Guid Id, string UserName, string Email, string DisplayName, string? AvatarUrl, bool isOnline = true, DateTime? lastSeen = null);
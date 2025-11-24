namespace ChatApp.Application.DTOs;

public record RegisterRequest(string UserName, string Email, string Password, string? Displayname);
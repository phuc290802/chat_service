namespace ChatApp.Application.DTOs
{
    public record AuthResponse(string AccessToken, string RefreshToken, UserDto User);
}

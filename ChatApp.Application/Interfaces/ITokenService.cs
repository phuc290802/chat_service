namespace ChatApp.Application.Interfaces
{
    using ChatApp.Domain.Entities;

    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken(User user);
    }
}

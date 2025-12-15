using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Repositories;
using ChatApp.Infrastructure.Services;
using Moq;

public class UserServiceTests
{
    [Fact]
    public async Task GetAllUser_ShouldReturnDtos_WithCorrectOnlineStatus()
    {
        var mockUserRepo = new Mock<IUserRepository>();
        var mockuserConnection = new Mock<IUserConnectionService>();
        var mockConverSationRepo = new Mock<IConversationRepository>();

        var listUser = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                UserName = "Test1",
                Email = "Test1@gmail.com",
                DisplayName = "Test user1",
                PasswordHash = "hashed_password",
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = Guid.NewGuid(),
                UserName = "Test2",
                Email = "Test2@gmail.com",
                DisplayName = "Test user2",
                PasswordHash = "hashed_password",
                CreatedAt = DateTime.UtcNow
            }
        };

        mockUserRepo.Setup(m => m.GetAllUser(It.IsAny<CancellationToken>())).ReturnsAsync(listUser);
        mockuserConnection.Setup(m => m.GetAllOnlineUsers()).Returns(new List<Guid> { listUser[0].Id });

        var userService = new UserService(mockUserRepo.Object, mockuserConnection.Object, mockConverSationRepo.Object);

        var result = await userService.GetAllUser();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Assert.True(result.First(u => u.Id == listUser[0].Id).isOnline);
        Assert.False(result.First(u => u.Id == listUser[1].Id).isOnline);
    }
}
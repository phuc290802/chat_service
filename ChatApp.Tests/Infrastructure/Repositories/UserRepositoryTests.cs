using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Persistence;
using ChatApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Tests
{
    public class UserRepositoryTests
    {
        private AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }


        [Fact]
        public async Task AddAsync_ShouldAddUser()
        {
            var context = CreateDbContext();
            var repo = new UserRepository(context);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "Test@gmail.com",
                UserName = "Test123",
                DisplayName = "Test user",
                PasswordHash = "hashed_password",
                CreatedAt = DateTime.UtcNow
            };

            await repo.AddAsync(user);
            await repo.SaveChangesAsync();

            var result = await repo.GetByEmailAsync("Test@gmail.com");
            Assert.NotNull(result);
            Assert.Equal("Test123", result!.UserName);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUser()
        {
            var context = CreateDbContext();
            var repo = new UserRepository(context);

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = "Test",
                Email = "Test@gmail.com",
                DisplayName = "Test user",
                PasswordHash = "hashed_password",
                CreatedAt = DateTime.UtcNow
            };

            await repo.AddAsync(user);
            await repo.SaveChangesAsync();

            var result = await repo.GetByIdAsync(user.Id);

            Assert.NotNull(result);
            Assert.Equal("Test", result!.UserName);
        }

        [Fact]
        public async Task GetByUserNameAsync_ShouldReturnUser()
        {
            var context = CreateDbContext();
            var repo = new UserRepository(context);

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = "Test",
                Email = "Test@gmail.com",
                DisplayName = "Test user",
                PasswordHash = "hashed_password",
                CreatedAt = DateTime.UtcNow
            };

            await repo.AddAsync(user);
            await repo.SaveChangesAsync();

            var result = await repo.GetByUserNameAsync(user.UserName);

            Assert.NotNull(result);
            Assert.Equal("Test@gmail.com", result!.Email);
        }

        [Fact]
        public async Task GetAllUser_ShouldReturnUser()
        {
            var context = CreateDbContext();
            var repo = new UserRepository(context);

            var user1 = new User
            {
                Id = Guid.NewGuid(),
                UserName = "Test1",
                Email = "Test1@gmail.com",
                DisplayName = "Test user 1",
                PasswordHash = "hashed_password",
                CreatedAt = DateTime.UtcNow
            };

            var user2 = new User
            {
                Id = Guid.NewGuid(),
                UserName = "Test2",
                Email = "Test2@gmail.com",
                DisplayName = "Test user 2",
                PasswordHash = "hashed_password",
                CreatedAt = DateTime.UtcNow
            };

            await repo.AddAsync(user1);
            await repo.AddAsync(user2);
            await repo.SaveChangesAsync();

            var result = await repo.GetAllUser();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, u => u.UserName == "Test1");
            Assert.Contains(result, u => u.UserName == "Test2");
        }
    }
}

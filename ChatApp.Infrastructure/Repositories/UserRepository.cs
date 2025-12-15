using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public UserRepository(AppDbContext db) => _db = db;

    public Task AddAsync(User user, CancellationToken ct = default)
    {
        _db.Users.Add(user);
        return Task.CompletedTask;
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) => 
        _db.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Email == email, ct);

    public Task<User?> GetByIdAsync(Guid userId, CancellationToken ct = default) =>
        _db.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Id == userId, ct);
    
    public Task<User?> GetByUserNameAsync(string userName, CancellationToken ct = default) =>
         _db.Users.AsNoTracking().FirstOrDefaultAsync(user => user.UserName == userName, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => 
        _db.SaveChangesAsync(ct);

    public Task<List<User>> GetAllUser(CancellationToken ct = default) =>
        _db.Users.AsNoTracking().ToListAsync(ct);
}

using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _db;
        public RefreshTokenRepository(AppDbContext db) =>
            _db = db;

        public async Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default) =>
            await _db.RefreshTokens.AddAsync(refreshToken, ct);

        public async Task<RefreshToken> GetValidTokenAsync(string token, CancellationToken ct = default) =>
            await _db.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token, ct);

        public async Task SaveChangesAsync(CancellationToken ct = default) =>
            await _db.SaveChangesAsync(ct);
    }
}

using ChatApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default);
        Task<RefreshToken> GetValidTokenAsync(string token, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}

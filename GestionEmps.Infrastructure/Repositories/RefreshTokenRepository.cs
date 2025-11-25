using GestionEmps.Application.Interfaces.Repositories;
using GestionEmps.Core.Entities;
using GestionEmps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEmps.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public RefreshTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(RefreshToken token)
        {
            await _context.RefreshTokens.AddAsync(token);
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task<List<RefreshToken>> GetActiveTokensByUserAsync(string userId)
        {
            return await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.IsActive)
                .ToListAsync();
        }

        public void Update(RefreshToken token)
        {
            _context.RefreshTokens.Update(token);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
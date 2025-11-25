using GestionEmps.Core.Entities;

namespace GestionEmps.Application.Interfaces.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken token);
        
        Task<RefreshToken?> GetByTokenAsync(string token);
        
        Task<List<RefreshToken>> GetActiveTokensByUserAsync(string userId);
        
        void Update(RefreshToken token);
        
        Task SaveChangesAsync();
    }
}

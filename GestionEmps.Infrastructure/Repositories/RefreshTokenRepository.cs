using Microsoft.EntityFrameworkCore;
using SGE.Application.Interfaces.Repositories;
using SGE.Core.Entities;
using SGE.Infrastructure.Data;

namespace SGE.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for managing refresh tokens.
/// </summary>
public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    /// <summary>
    /// Initializes a new instance of the RefreshTokenRepository class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public RefreshTokenRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Retrieves a refresh token by its token string asynchronously.
    /// </summary>
    /// <param name="token">The token string to search for.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The refresh token if found, otherwise null.</returns>
    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    /// <summary>
    /// Retrieves all active refresh tokens for a specific user asynchronously.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A collection of active refresh tokens for the user.</returns>
    public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > now)
            .ToListAsync(cancellationToken);
    }
}



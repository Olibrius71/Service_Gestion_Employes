using SGE.Core.Entities;

namespace SGE.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for managing refresh tokens.
/// </summary>
public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    /// <summary>
    /// Retrieves a refresh token by its token string asynchronously.
    /// </summary>
    /// <param name="token">The token string to search for.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The refresh token if found, otherwise null.</returns>
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all active refresh tokens for a specific user asynchronously.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A collection of active refresh tokens for the user.</returns>
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}



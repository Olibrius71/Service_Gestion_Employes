using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SGE.Application.Interfaces.Repositories;
using SGE.Application.Interfaces.Services;
using SGE.Core.Entities;

namespace SGE.Application.Services;

/// <summary>
/// Service for managing JWT tokens and refresh tokens.
/// </summary>
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    /// <summary>
    /// Initializes a new instance of the TokenService class.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="tokenHandler">The JWT token handler.</param>
    /// <param name="refreshTokenRepository">The refresh token repository.</param>
    public TokenService(
        IConfiguration configuration,
        JwtSecurityTokenHandler tokenHandler,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _configuration = configuration;
        _tokenHandler = tokenHandler;
        _refreshTokenRepository = refreshTokenRepository;
    }

    /// <summary>
    /// Generates a JSON Web Token (JWT) access token for a specified user and their associated roles.
    /// </summary>
    /// <param name="user">The user for whom the access token is being generated.</param>
    /// <param name="roles">The list of roles associated with the user.</param>
    /// <returns>A string representation of the generated JWT access token.</returns>
    public string GenerateAccessToken(ApplicationUser user, IList<string> roles)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secret = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim("firstName", user.FirstName),
            new Claim("lastName", user.LastName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        // Ajout des rôles
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["AccessTokenExpiration"]!)),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(secret),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        return _tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Generates a secure refresh token that can be used to obtain a new access token.
    /// </summary>
    /// <returns>A base64-encoded string representation of the generated refresh token.</returns>
    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    /// Retrieves the claims principal from a provided expired JSON Web Token (JWT).
    /// This allows for extracting user claims from a token that is no longer valid for authentication but still contains valid claims data.
    /// </summary>
    /// <param name="token">The expired JWT from which to extract the claims principal.</param>
    /// <returns>A <see cref="ClaimsPrincipal"/> representing the claims contained in the expired token.</returns>
    /// <exception cref="SecurityTokenException">Thrown if the token is invalid or the algorithm used in the header is not HmacSha256.</exception>
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secret = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secret),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = false // Important : on ne valide pas l'expiration ici
        };

        var principal = _tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

        if (validatedToken is not JwtSecurityToken jwtToken ||
            !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Token invalide");
        }

        return principal;
    }

    /// <summary>
    /// Creates a refresh token asynchronously for user authentication and session management.
    /// </summary>
    /// <param name="userId">The unique identifier of the user for whom the refresh token is being created.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created refresh token.</returns>
    public async Task<RefreshToken> CreateRefreshTokenAsync(string userId)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var expirationDays = int.Parse(jwtSettings["RefreshTokenExpiration"]!);

        var refreshToken = new RefreshToken
        {
            Token = GenerateRefreshToken(),
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.AddAsync(refreshToken);
        return refreshToken;
    }

    /// <summary>
    /// Validates the provided refresh token for the specified user asynchronously.
    /// </summary>
    /// <param name="token">The refresh token to validate.</param>
    /// <param name="userId">The unique identifier of the user associated with the refresh token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result indicates whether the refresh token is valid.</returns>
    public async Task<bool> ValidateRefreshTokenAsync(string token, string userId)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token);
        
        if (refreshToken == null)
            return false;

        if (refreshToken.UserId != userId)
            return false;

        if (!refreshToken.IsActive)
            return false;

        return true;
    }

    /// <summary>
    /// Revokes a refresh token asynchronously for a specified reason.
    /// </summary>
    /// <param name="token">The refresh token to be revoked.</param>
    /// <param name="reason">The reason for revoking the refresh token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result indicates whether the revocation was successful.</returns>
    public async Task RevokeRefreshTokenAsync(string token, string reason)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token);
        
        if (refreshToken == null || refreshToken.IsRevoked)
            return;

        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.ReasonRevoked = reason;
        
        await _refreshTokenRepository.UpdateAsync(refreshToken);
    }

    /// <summary>
    /// Revokes all active refresh tokens associated with a specific user asynchronously.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose refresh tokens are to be revoked.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task RevokeAllUserRefreshTokensAsync(string userId)
    {
        var activeTokens = await _refreshTokenRepository.GetActiveTokensByUserIdAsync(userId);
        
        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.ReasonRevoked = "Déconnexion utilisateur";
            await _refreshTokenRepository.UpdateAsync(token);
        }
    }
}



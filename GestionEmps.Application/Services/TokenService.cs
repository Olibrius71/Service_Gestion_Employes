using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using GestionEmps.Application.Interfaces.Services;
using GestionEmps.Core.Entities;

namespace GestionEmps.Application.Services;

public class TokenService (IConfiguration configuration, JwtSecurityTokenHandler tokenHandler ) : ITokenService
{
    /// <summary>
    /// Generates a JSON Web Token (JWT) access token for a specified user and their associated roles.
    /// </summary>
    /// <param name="user">The user for whom the access token is being generated.</param>
    /// <param name="roles">The list of roles associated with the user.</param>
    /// <returns>A string representation of the generated JWT access token.</returns>
    public string GenerateAccessToken(ApplicationUser user, IList<string> roles)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secret = Encoding.ASCII.GetBytes(jwtSettings["Secret"]);
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim("firstName", user.FirstName),
        new Claim("lastName", user.LastName),
        new Claim(JwtRegisteredClaimNames.Jti,
        Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Iat,
        new
        DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
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
        Expires =
        DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["AccessTokenExpiration"])),
        Issuer = jwtSettings["Issuer"],
        Audience = jwtSettings["Audience"],
        SigningCredentials = new SigningCredentials(
        new SymmetricSecurityKey(secret),
        SecurityAlgorithms.HmacSha256Signature)
    };
    
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
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
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secret = Encoding.ASCII.GetBytes(jwtSettings["Secret"]);
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
        
    var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
    
    if (validatedToken is not JwtSecurityToken jwtToken || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
    {
        throw new SecurityTokenException("Token invalide");
    }
        return principal;
    }
    
    public async Task<RefreshToken> CreateRefreshTokenAsync(string userId)
    {
        //TODO
        throw new NotImplementedException();
    }
    
    public async Task<bool> ValidateRefreshTokenAsync(string token, string userId)
    {
        //TODO
        throw new NotImplementedException();
    }
    
    public async Task RevokeRefreshTokenAsync(string token, string reason)
    {
        //TODO
        throw new NotImplementedException();
    }
    
    public async Task RevokeAllUserRefreshTokensAsync(string userId)
    {
        //TODO
        throw new NotImplementedException();
    }
}
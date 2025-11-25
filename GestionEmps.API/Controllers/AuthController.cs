using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGE.Application.DTOs.Users;
using SGE.Application.Interfaces.Services;
using System.Security.Claims;

namespace SGE.API.Controllers;

/// <summary>
/// Controller for handling authentication and authorization operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Initializes a new instance of the AuthController class.
    /// </summary>
    /// <param name="authService">The authentication service.</param>
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registers a new user in the system.
    /// </summary>
    /// <param name="registerDto">The registration data containing user information.</param>
    /// <returns>An authentication response with access and refresh tokens.</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(200, Type = typeof(AuthResponseDto))]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
    {
        var result = await _authService.RegisterAsync(registerDto);
        return Ok(result);
    }

    /// <summary>
    /// Authenticates a user and returns access and refresh tokens.
    /// </summary>
    /// <param name="loginDto">The login credentials (email and password).</param>
    /// <returns>An authentication response with access and refresh tokens.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(200, Type = typeof(AuthResponseDto))]
    [ProducesResponseType(401)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);
        return Ok(result);
    }

    /// <summary>
    /// Refreshes the access token using a valid refresh token.
    /// </summary>
    /// <param name="refreshTokenDto">The refresh token data containing access and refresh tokens.</param>
    /// <returns>A new authentication response with updated tokens.</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(200, Type = typeof(AuthResponseDto))]
    [ProducesResponseType(401)]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        var result = await _authService.RefreshTokenAsync(refreshTokenDto);
        return Ok(result);
    }

    /// <summary>
    /// Logs out the current user by revoking all their refresh tokens.
    /// </summary>
    /// <returns>A success message indicating the user has been logged out.</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult> Logout()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        await _authService.LogoutAsync(userId);
        return Ok(new { message = "Déconnexion réussie" });
    }

    /// <summary>
    /// Revokes a specific refresh token.
    /// </summary>
    /// <param name="token">The refresh token to revoke.</param>
    /// <returns>A success message indicating the token has been revoked.</returns>
    [HttpPost("revoke")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult> RevokeToken([FromBody] string token)
    {
        await _authService.RevokeTokenAsync(token);
        return Ok(new { message = "Token révoqué avec succès" });
    }

    /// <summary>
    /// Retrieves the current authenticated user's information.
    /// </summary>
    /// <returns>The current user's information.</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(200, Type = typeof(UserDto))]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _authService.GetCurrentUserAsync(userId);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    /// <summary>
    /// Updates the current user's information.
    /// </summary>
    /// <param name="updateDto">The updated user information.</param>
    /// <returns>The updated user information.</returns>
    [HttpPut("me")]
    [Authorize]
    [ProducesResponseType(200, Type = typeof(UserDto))]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<UserDto>> UpdateCurrentUser([FromBody] UserUpdateDto updateDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _authService.UpdateUserAsync(userId, updateDto);
        return Ok(user);
    }

    /// <summary>
    /// Deletes the current user's account.
    /// </summary>
    /// <returns>A success message indicating the account has been deleted.</returns>
    [HttpDelete("me")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult> DeleteCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        await _authService.DeleteUserAsync(userId);
        return Ok(new { message = "Compte supprimé avec succès" });
    }
}



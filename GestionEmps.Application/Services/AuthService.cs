using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using SGE.Application.DTOs.Users;
using SGE.Application.Interfaces.Services;
using SGE.Core.Entities;
using SGE.Core.Exceptions;

namespace SGE.Application.Services;

/// <summary>
/// Provides authentication and authorization services such as
/// user registration, login, token generation, and token refresh.
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the AuthService class.
    /// </summary>
    /// <param name="userManager">The user manager for Identity operations.</param>
    /// <param name="tokenService">The token service for JWT operations.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public AuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IMapper mapper)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    /// <summary>
    /// Registers a new user with the provided registration details.
    /// </summary>
    /// <param name="registerDto">The user registration data (username, email, password).</param>
    /// <returns>An <see cref="AuthResponseDto"/> containing access and refresh tokens and user information.</returns>
    /// <exception cref="UserAlreadyExistsException">Thrown when the email or username already exists in the system.</exception>
    /// <exception cref="UserRegistrationException">Thrown when user creation fails due to Identity errors.</exception>
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        var existingUserByEmail = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUserByEmail != null)
            throw new UserAlreadyExistsException(registerDto.Email, "email");

        var existingUserByUsername = await _userManager.FindByNameAsync(registerDto.UserName);
        if (existingUserByUsername != null)
            throw new UserAlreadyExistsException(registerDto.UserName, "nom d'utilisateur");

        var user = _mapper.Map<ApplicationUser>(registerDto);
        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            throw new UserRegistrationException(errors);
        }

        await _userManager.AddToRoleAsync(user, "User");

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = await _tokenService.CreateRefreshTokenAsync(user.Id);

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var userDto = _mapper.Map<UserDto>(user);
        userDto.Roles = roles;

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = refreshToken.ExpiresAt,
            User = userDto
        };
    }

    /// <summary>
    /// Authenticates a user with the provided login credentials.
    /// </summary>
    /// <param name="loginDto">The login data (email and password).</param>
    /// <returns>An <see cref="AuthResponseDto"/> containing access and refresh tokens and user information.</returns>
    /// <exception cref="InvalidCredentialsException">Thrown when the email or password is incorrect.</exception>
    /// <exception cref="UserNotActiveException">Thrown when the user account is disabled or inactive.</exception>
    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            throw new InvalidCredentialsException();

        if (!user.IsActive)
            throw new UserNotActiveException();

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = await _tokenService.CreateRefreshTokenAsync(user.Id);

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var userDto = _mapper.Map<UserDto>(user);
        userDto.Roles = roles;

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = refreshToken.ExpiresAt,
            User = userDto
        };
    }

    /// <summary>
    /// Refreshes the access token using a valid refresh token.
    /// </summary>
    /// <param name="refreshTokenDto">The refresh token data (access token and refresh token).</param>
    /// <returns>An <see cref="AuthResponseDto"/> containing new access and refresh tokens and user information.</returns>
    /// <exception cref="InvalidRefreshTokenException">Thrown when the refresh token is invalid or expired.</exception>
    /// <exception cref="UserNotActiveException">Thrown when the user account is disabled or inactive.</exception>
    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(refreshTokenDto.AccessToken);
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            throw new InvalidRefreshTokenException();

        var isValidRefreshToken = await _tokenService.ValidateRefreshTokenAsync(refreshTokenDto.RefreshToken, userId);
        if (!isValidRefreshToken)
            throw new InvalidRefreshTokenException();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is not { IsActive: true })
            throw new UserNotActiveException();

        await _tokenService.RevokeRefreshTokenAsync(refreshTokenDto.RefreshToken, "Remplacé par un nouveau token");

        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
        var newRefreshToken = await _tokenService.CreateRefreshTokenAsync(user.Id);

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var userDto = _mapper.Map<UserDto>(user);
        userDto.Roles = roles;

        return new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = newRefreshToken.ExpiresAt,
            User = userDto
        };
    }

    /// <summary>
    /// Logs out a user by revoking all their active refresh tokens.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns><c>true</c> if logout succeeded, otherwise <c>false</c>.</returns>
    public async Task<bool> LogoutAsync(string userId)
    {
        await _tokenService.RevokeAllUserRefreshTokensAsync(userId);
        return true;
    }

    /// <summary>
    /// Revokes a specific refresh token.
    /// </summary>
    /// <param name="token">The refresh token string to revoke.</param>
    /// <returns><c>true</c> if the token was revoked successfully, otherwise <c>false</c>.</returns>
    public async Task<bool> RevokeTokenAsync(string token)
    {
        await _tokenService.RevokeRefreshTokenAsync(token, "Révoqué par l'utilisateur");
        return true;
    }

    /// <summary>
    /// Retrieves the details of the currently authenticated user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A <see cref="UserDto"/> representing the user, or <c>null</c> if not found.</returns>
    public async Task<UserDto?> GetCurrentUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);
        var userDto = _mapper.Map<UserDto>(user);
        userDto.Roles = roles;

        return userDto;
    }

    /// <summary>
    /// Updates user information asynchronously.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to update.</param>
    /// <param name="updateDto">The data transfer object containing the updated user information.</param>
    /// <returns>A Task that represents the asynchronous operation. The task result contains the updated user information.</returns>
    public async Task<UserDto> UpdateUserAsync(string userId, UserUpdateDto updateDto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new InvalidCredentialsException();

        if (!string.IsNullOrEmpty(updateDto.FirstName))
            user.FirstName = updateDto.FirstName;

        if (!string.IsNullOrEmpty(updateDto.LastName))
            user.LastName = updateDto.LastName;

        if (!string.IsNullOrEmpty(updateDto.Email) && updateDto.Email != user.Email)
        {
            var existingUser = await _userManager.FindByEmailAsync(updateDto.Email);
            if (existingUser != null && existingUser.Id != userId)
                throw new UserAlreadyExistsException(updateDto.Email, "email");

            user.Email = updateDto.Email;
            user.UserName = updateDto.Email; // Optionnel : synchroniser username avec email
        }

        if (!string.IsNullOrEmpty(updateDto.UserName) && updateDto.UserName != user.UserName)
        {
            var existingUser = await _userManager.FindByNameAsync(updateDto.UserName);
            if (existingUser != null && existingUser.Id != userId)
                throw new UserAlreadyExistsException(updateDto.UserName, "nom d'utilisateur");

            user.UserName = updateDto.UserName;
        }

        if (updateDto.IsActive.HasValue)
            user.IsActive = updateDto.IsActive.Value;

        if (updateDto.EmployeeId.HasValue)
            user.EmployeeId = updateDto.EmployeeId.Value;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            throw new UserRegistrationException(errors);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var userDto = _mapper.Map<UserDto>(user);
        userDto.Roles = roles;

        return userDto;
    }

    /// <summary>
    /// Deletes a user account asynchronously.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to delete.</param>
    /// <returns>A Task that represents the asynchronous operation. The task result indicates whether the deletion was successful.</returns>
    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new InvalidCredentialsException(); // Utilisateur non trouvé

        // Révoquer tous les tokens avant suppression
        await _tokenService.RevokeAllUserRefreshTokensAsync(userId);

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }
}


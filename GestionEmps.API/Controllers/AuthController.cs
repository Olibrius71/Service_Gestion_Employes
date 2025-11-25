using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using GestionEmps.Application.DTOs;
using GestionEmps.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;

namespace GestionEmps.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
    {
        var result = await authService.RegisterAsync(registerDto);
        return Ok(result);
    }
    
    [HttpPost("login")]
    [AllowAnonymous]   
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
    {
        var result = await authService.LoginAsync(loginDto);
        return Ok(result);
    }
    
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken(RefreshTokenDto refreshTokenDto)
    {
        var result = await authService.RefreshTokenAsync(refreshTokenDto);
        return Ok(result);
    }
    
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (userId == null)
            return Unauthorized();
        
        await authService.LogoutAsync(userId);
        return Ok(new { message = "Déconnexion réussie" });
    }
}

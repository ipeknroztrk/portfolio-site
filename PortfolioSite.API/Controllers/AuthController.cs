using Microsoft.AspNetCore.Mvc;
using PortfolioSite.Application.DTOs;
using PortfolioSite.Application.Interfaces;

namespace PortfolioSite.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Kullanici adi ve sifre gereklidir.");

        var result = await _authService.LoginAsync(request);

        if (result == null)
            return Unauthorized("Kullanici adi veya sifre yanlis.");

        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);

        if (result == null)
            return Unauthorized("Gecersiz veya suresi dolmus token.");

        return Ok(result);
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequestDto request)
    {
        await _authService.RevokeTokenAsync(request.RefreshToken);
        return NoContent();
    }
}
using PortfolioSite.Application.DTOs;

namespace PortfolioSite.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
    Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken);
    Task RevokeTokenAsync(string refreshToken);
}
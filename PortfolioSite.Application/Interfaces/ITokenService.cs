using PortfolioSite.Application.Entities;

namespace PortfolioSite.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(AdminUser user);
    string GenerateRefreshToken();
}
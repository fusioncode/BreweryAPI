using BreweryAPI.Models.Entities;
using System.Security.Claims;

namespace BreweryAPI.Models.Service.Interface
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user);
        ClaimsPrincipal? ValidateToken(string token);
        string GenerateRefreshToken();
        bool ValidateRefreshToken(string refreshToken);
    }
}

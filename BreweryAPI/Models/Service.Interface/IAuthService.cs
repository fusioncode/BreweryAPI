using BreweryAPI.Models.DTOs;
using BreweryAPI.Models.Entities;

namespace BreweryAPI.Models.Service.Interface
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginRequestDto loginRequest);
        Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto registerRequest);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(int id);
        bool VerifyPassword(string password, string hash);
        string HashPassword(string password);
    }
}

using BreweryAPI.Models.DTOs;
using BreweryAPI.Models.Entities;
using BreweryAPI.Models.Service.Interface;
using System.Security.Cryptography;
using System.Text;

namespace BreweryAPI.Models.Service
{
    public class AuthService : IAuthService
    {
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<AuthService> _logger;
        
        // In-memory user storage for demo purposes
        // In a real application, you would use a database
        private static readonly List<User> _users = new List<User>();
        private static int _nextUserId = 1;

        public AuthService(IJwtTokenService jwtTokenService, ILogger<AuthService> logger)
        {
            _jwtTokenService = jwtTokenService;
            _logger = logger;
            
            // Add a default admin user for testing
            if (!_users.Any())
            {
                _users.Add(new User
                {
                    Id = _nextUserId++,
                    Username = "admin",
                    Email = "admin@brewery.com",
                    PasswordHash = HashPassword("admin123"),
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                });
            }
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto loginRequest)
        {
            try
            {
                var user = await GetUserByEmailAsync(loginRequest.Email);
                
                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning("Login attempt failed: User not found or inactive for email {Email}", loginRequest.Email);
                    return null;
                }

                if (!VerifyPassword(loginRequest.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Login attempt failed: Invalid password for email {Email}", loginRequest.Email);
                    return null;
                }

                // Update last login time
                user.LastLoginAt = DateTime.UtcNow;

                var token = _jwtTokenService.GenerateToken(user);
                
                _logger.LogInformation("User {Username} logged in successfully", user.Username);

                return new AuthResponseDto
                {
                    Token = token,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60) // Should match token expiration
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email {Email}", loginRequest.Email);
                return null;
            }
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto registerRequest)
        {
            try
            {
                // Check if user already exists
                var existingUser = await GetUserByEmailAsync(registerRequest.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("Registration attempt failed: User already exists with email {Email}", registerRequest.Email);
                    return null;
                }

                // Check if username is taken
                var existingUsername = _users.FirstOrDefault(u => u.Username.Equals(registerRequest.Username, StringComparison.OrdinalIgnoreCase));
                if (existingUsername != null)
                {
                    _logger.LogWarning("Registration attempt failed: Username {Username} is already taken", registerRequest.Username);
                    return null;
                }

                var user = new User
                {
                    Id = _nextUserId++,
                    Username = registerRequest.Username,
                    Email = registerRequest.Email,
                    PasswordHash = HashPassword(registerRequest.Password),
                    Role = "User",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _users.Add(user);

                var token = _jwtTokenService.GenerateToken(user);
                
                _logger.LogInformation("User {Username} registered successfully", user.Username);

                return new AuthResponseDto
                {
                    Token = token,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60) // Should match token expiration
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email {Email}", registerRequest.Email);
                return null;
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await Task.FromResult(_users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
        }

        public bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "BreweryAPI_Salt"));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}

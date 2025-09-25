using BreweryAPI.Models.DTOs;
using BreweryAPI.Models.DTOs.V2;
using BreweryAPI.Models.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BreweryAPI.Controllers.V2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/Auth")]
    public class AuthV2Controller : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthV2Controller> _logger;

        public AuthV2Controller(IAuthService authService, ILogger<AuthV2Controller> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Enhanced login with additional session tracking and device info (V2).
        /// </summary>
        /// <param name="loginRequest">Enhanced login credentials with device info</param>
        /// <returns>JWT tokens with enhanced user information and session data</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginV2RequestDto loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Convert V2 request to V1 for existing service
                var v1Request = new LoginRequestDto
                {
                    Email = loginRequest.Email,
                    Password = loginRequest.Password
                };

                var result = await _authService.LoginAsync(v1Request);
                
                if (result == null)
                {
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                // Enhance response with V2 features
                var userDto = new UserDto
                {
                    Id = 1, // Mock ID since V1 doesn't return user ID
                    Username = result.Username,
                    Email = result.Email,
                    Role = result.Role,
                    CreatedAt = DateTime.UtcNow.AddDays(-30), // Mock creation date
                    LastLoginAt = DateTime.UtcNow,
                    IsActive = true
                };

                var enhancedResponse = new AuthV2ResponseDto
                {
                    AccessToken = result.Token,
                    RefreshToken = GenerateRefreshToken(), // Mock implementation
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                    User = ConvertToV2UserDto(userDto),
                    Session = new SessionInfo
                    {
                        SessionId = Guid.NewGuid().ToString(),
                        CreatedAt = DateTime.UtcNow,
                        DeviceInfo = loginRequest.DeviceInfo ?? "Unknown Device",
                        IpAddress = loginRequest.IpAddress ?? HttpContext.Connection.RemoteIpAddress?.ToString(),
                        IsActive = true
                    }
                };

                return Ok(enhancedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login V2");
                return StatusCode(500, new { message = "An error occurred while processing your request" });
            }
        }

        /// <summary>
        /// Enhanced registration with additional user profile fields (V2).
        /// </summary>
        /// <param name="registerRequest">Enhanced registration details</param>
        /// <returns>JWT tokens with enhanced user information</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterV2RequestDto registerRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Convert V2 request to V1 for existing service
                var v1Request = new RegisterRequestDto
                {
                    Username = registerRequest.Username,
                    Email = registerRequest.Email,
                    Password = registerRequest.Password
                };

                var result = await _authService.RegisterAsync(v1Request);
                
                if (result == null)
                {
                    return BadRequest(new { message = "User with this email or username already exists" });
                }

                // Enhance response with V2 features
                var userDto = new UserDto
                {
                    Id = 2, // Mock ID since V1 doesn't return user ID
                    Username = result.Username,
                    Email = result.Email,
                    Role = result.Role,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = null,
                    IsActive = true
                };

                var enhancedResponse = new AuthV2ResponseDto
                {
                    AccessToken = result.Token,
                    RefreshToken = GenerateRefreshToken(),
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                    User = ConvertToV2UserDto(userDto, registerRequest),
                    Session = new SessionInfo
                    {
                        SessionId = Guid.NewGuid().ToString(),
                        CreatedAt = DateTime.UtcNow,
                        DeviceInfo = "Registration Device",
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        IsActive = true
                    }
                };

                return Ok(enhancedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during registration V2");
                return StatusCode(500, new { message = "An error occurred while processing your request" });
            }
        }

        /// <summary>
        /// Get enhanced current user information with preferences and stats (V2).
        /// </summary>
        /// <returns>Enhanced user details with preferences and statistics</returns>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var user = await _authService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var userV2Dto = ConvertToV2UserDto(new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    IsActive = user.IsActive
                });

                return Ok(userV2Dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting current user V2");
                return StatusCode(500, new { message = "An error occurred while processing your request" });
            }
        }

        /// <summary>
        /// Refresh access token using refresh token (V2).
        /// </summary>
        /// <param name="request">Refresh token request</param>
        /// <returns>New access token</returns>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Mock implementation - in real scenario, validate refresh token
                if (string.IsNullOrEmpty(request.RefreshToken))
                {
                    return BadRequest(new { message = "Invalid refresh token" });
                }

                var response = new
                {
                    access_token = GenerateAccessToken(),
                    refresh_token = GenerateRefreshToken(),
                    expires_at = DateTime.UtcNow.AddMinutes(60),
                    token_type = "Bearer"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during token refresh V2");
                return StatusCode(500, new { message = "An error occurred while processing your request" });
            }
        }

        /// <summary>
        /// Change user password (V2).
        /// </summary>
        /// <param name="request">Password change request</param>
        /// <returns>Success message</returns>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordChangeRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Mock implementation - in real scenario, validate current password and update
                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during password change V2");
                return StatusCode(500, new { message = "An error occurred while processing your request" });
            }
        }

        /// <summary>
        /// Enhanced test endpoint with detailed user and session info (V2).
        /// </summary>
        /// <returns>Enhanced success message with user and session info</returns>
        [HttpGet("test")]
        [Authorize]
        public IActionResult TestAuth()
        {
            var username = User.FindFirst("username")?.Value;
            var email = User.FindFirst("email")?.Value;
            var role = User.FindFirst("role")?.Value;

            return Ok(new
            {
                message = "Authentication successful! (API v2.0)",
                api_version = "2.0",
                timestamp = DateTime.UtcNow,
                user = new
                {
                    username = username,
                    email = email,
                    role = role
                },
                session = new
                {
                    session_id = Guid.NewGuid().ToString(),
                    ip_address = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    user_agent = Request.Headers["User-Agent"].ToString()
                },
                features = new[]
                {
                    "Enhanced user profiles",
                    "Session tracking",
                    "Refresh tokens",
                    "Password management",
                    "Advanced brewery search",
                    "Pagination support"
                }
            });
        }

        /// <summary>
        /// Logout and invalidate session (V2).
        /// </summary>
        /// <returns>Success message</returns>
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            try
            {
                // Mock implementation - in real scenario, invalidate tokens and session
                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during logout V2");
                return StatusCode(500, new { message = "An error occurred while processing your request" });
            }
        }

        private UserV2Dto ConvertToV2UserDto(UserDto user, RegisterV2RequestDto registerRequest = null)
        {
            return new UserV2Dto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                IsActive = user.IsActive,
                FirstName = registerRequest?.FirstName ?? "John",
                LastName = registerRequest?.LastName ?? "Doe",
                DateOfBirth = registerRequest?.DateOfBirth,
                PhoneNumber = registerRequest?.PhoneNumber,
                Preferences = registerRequest?.Preferences ?? new UserPreferences(),
                Stats = new UserStats
                {
                    BreweriesVisited = Random.Shared.Next(0, 50),
                    ReviewsWritten = Random.Shared.Next(0, 25),
                    FavoriteBreweries = Random.Shared.Next(0, 10),
                    LastActivity = DateTime.UtcNow.AddDays(-Random.Shared.Next(0, 7)),
                    LoginCount = Random.Shared.Next(1, 100)
                }
            };
        }

        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + 
                   Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }

        private string GenerateAccessToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + "." +
                   Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + "." +
                   Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }
    }
}

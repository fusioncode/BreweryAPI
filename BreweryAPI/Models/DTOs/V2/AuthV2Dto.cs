using System.ComponentModel.DataAnnotations;

namespace BreweryAPI.Models.DTOs.V2
{
    public class LoginV2RequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        public bool RememberMe { get; set; } = false;
        public string DeviceInfo { get; set; }
        public string IpAddress { get; set; }
    }

    public class RegisterV2RequestDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public UserPreferences Preferences { get; set; } = new UserPreferences();
    }

    public class AuthV2ResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public UserV2Dto User { get; set; }
        public SessionInfo Session { get; set; }
    }

    public class UserV2Dto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
        public DateTime? DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public UserPreferences Preferences { get; set; }
        public UserStats Stats { get; set; }
    }

    public class UserPreferences
    {
        public string PreferredBreweryType { get; set; }
        public List<string> FavoriteBreweryTypes { get; set; } = new List<string>();
        public bool EmailNotifications { get; set; } = true;
        public bool PushNotifications { get; set; } = true;
        public string PreferredLanguage { get; set; } = "en";
        public string TimeZone { get; set; }
        public DistanceUnit PreferredDistanceUnit { get; set; } = DistanceUnit.Kilometers;
    }

    public class UserStats
    {
        public int BreweriesVisited { get; set; }
        public int ReviewsWritten { get; set; }
        public int FavoriteBreweries { get; set; }
        public DateTime? LastActivity { get; set; }
        public int LoginCount { get; set; }
    }

    public class SessionInfo
    {
        public string SessionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DeviceInfo { get; set; }
        public string IpAddress { get; set; }
        public bool IsActive { get; set; }
    }

    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; }
    }

    public class PasswordChangeRequestDto
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; }
    }

    public enum DistanceUnit
    {
        Kilometers,
        Miles
    }
}

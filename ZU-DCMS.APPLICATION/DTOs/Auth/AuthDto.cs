
namespace ZU_DCMS.APPLICATION.DTOs.Auth
{
     
    // __ DTO for authentication __ //
    public class AuthDto
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string RedirectUrl { get; set; } = string.Empty;
    }
}

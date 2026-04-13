
namespace ZU_DCMS.APPLICATION.DTOs.Auth
{
     
    // __ DTO for authentication __ //
    public class AuthDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string RedirectUrl { get; set; } = string.Empty;
    }
}

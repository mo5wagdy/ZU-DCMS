
namespace ZU_DCMS.APPLICATION.DTOs.Auth
{
    // __ DTO for user login __ //
    public class LoginPatientDto
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
    }
}
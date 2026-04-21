
namespace ZU_DCMS.APPLICATION.DTOs.Auth
{
    // __ DTO for user login __ //
    public class LoginPatientDto
    {
        // Gonna be the phone number instead of username.
        public string PhoneNumber { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
    }
}

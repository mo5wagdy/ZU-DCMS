
namespace ZU_DCMS.APPLICATION.DTOs.Admin
{
    // __ DTO for Staff User information __ //
    public class CreateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? AcademicYear { get; set; }
    }
}

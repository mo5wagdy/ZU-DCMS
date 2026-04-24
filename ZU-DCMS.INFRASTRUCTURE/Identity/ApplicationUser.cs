using Microsoft.AspNetCore.Identity;
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.INFRASTRUCTURE.Identity
{
    /*
     * Custom application user class that extends IdentityUser.
     * This class can be extended with additional properties as needed 
     */
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public UserType UserType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

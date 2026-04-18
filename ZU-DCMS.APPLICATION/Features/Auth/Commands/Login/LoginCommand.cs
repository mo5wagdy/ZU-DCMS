using ZU_DCMS.APPLICATION.DTOs.Auth;

namespace ZU_DCMS.APPLICATION.Features.Auth.Commands.Login
{
    public class LoginCommand
    {
        public LoginPatientDto Dto { get; set; } = null!;
    }
}

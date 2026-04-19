using ZU_DCMS.APPLICATION.DTOs.Patient;

namespace ZU_DCMS.APPLICATION.Features.Patients.Commands.UpdateProfile
{
    public class UpdateProfileCommand
    {
        public int Id { get; set; }
        public UpdatePatientDto Dto { get; set; } = null!;
    }
}

using ZU_DCMS.APPLICATION.DTOs.Diagnosis;

namespace ZU_DCMS.APPLICATION.Features.Diagnosis.Commands.AssignStudent
{
    public class AssignStudentCommand
    {
        public string InternDoctorId { get; set; } = null!;
        public AssignStudentDto Dto { get; set; } = null!;
    }
}

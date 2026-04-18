using ZU_DCMS.APPLICATION.DTOs.Diagnosis;

namespace ZU_DCMS.APPLICATION.Features.Diagnosis.Commands.DiagnosePatient
{
    public class DiagnosePatientCommand
    {
        public string InternDoctorId { get; set; } = null!;
        public CreateDiagnosisDto Dto { get; set; } = null!;
    }
}

using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Diagnosis;

namespace ZU_DCMS.APPLICATION.Features.Diagnosis.Commands.DiagnosePatient
{
    public record DiagnosePatientCommand(string InternDoctorId, CreateDiagnosisDto Dto) : IRequest<Result<DiagnosisRecordDto>>;
}

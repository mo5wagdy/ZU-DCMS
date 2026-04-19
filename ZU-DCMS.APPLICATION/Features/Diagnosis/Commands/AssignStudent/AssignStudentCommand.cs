using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Case;
using ZU_DCMS.APPLICATION.DTOs.Diagnosis;

namespace ZU_DCMS.APPLICATION.Features.Diagnosis.Commands.AssignStudent
{
    public record AssignStudentCommand(string InternDoctorId, AssignStudentDto Dto) : IRequest<Result<CaseAssignmentDto>>;
}

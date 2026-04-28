using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Case;

namespace ZU_DCMS.APPLICATION.Features.Cases.Queries.GetStudentTodayPatients
{
    public record GetStudentTodayPatientsQuery(string StudentApplicationUserId) : IRequest<Result<List<CaseAssignmentDto>>>;
}

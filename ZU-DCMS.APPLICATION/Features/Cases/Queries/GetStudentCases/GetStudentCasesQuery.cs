using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Case;

namespace ZU_DCMS.APPLICATION.Features.Cases.Queries.GetStudentCases
{
    public record GetStudentCasesQuery(int StudentId) : IRequest<Result<List<CaseAssignmentDto>>>;
}

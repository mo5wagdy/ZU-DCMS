using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Student;

namespace ZU_DCMS.APPLICATION.Features.Admin.Queries.GetStudentRequirements
{
    public record GetStudentRequirementsQuery(int StudentId, int TermId) : IRequest<Result<List<StudentRequirementDto>>>;
}

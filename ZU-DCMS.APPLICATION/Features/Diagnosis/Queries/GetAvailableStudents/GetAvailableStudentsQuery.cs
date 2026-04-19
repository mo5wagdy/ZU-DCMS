using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Student;

namespace ZU_DCMS.APPLICATION.Features.Diagnosis.Queries.GetAvailableStudents
{
    public record GetAvailableStudentsQuery(int ClinicId, int TermId) : IRequest<Result<List<StudentPriorityDto>>>;
}

using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Pagination;
using ZU_DCMS.APPLICATION.DTOs.Student;

namespace ZU_DCMS.APPLICATION.Features.Students.Queries.GetAllStudents
{
    public record GetAllStudentsQuery(PagedRequest Request, int? AcademicYear = null) : IRequest<Result<PagedResult<StudentDto>>>;
}

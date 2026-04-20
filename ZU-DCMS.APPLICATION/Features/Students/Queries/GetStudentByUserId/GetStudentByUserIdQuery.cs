using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Student;

namespace ZU_DCMS.APPLICATION.Features.Students.Queries.GetStudentByUserId
{
    public record GetStudentByUserIdQuery(string UserId) : IRequest<Result<StudentDto>>;
}

using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Student;

namespace ZU_DCMS.APPLICATION.Features.Students.Queries.GetStudentById
{
    public record GetStudentByIdQuery(int StudentId) : IRequest<Result<StudentDto>>;
}

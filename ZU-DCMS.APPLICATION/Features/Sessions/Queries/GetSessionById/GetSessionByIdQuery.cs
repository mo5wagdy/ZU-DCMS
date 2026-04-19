using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Session;

namespace ZU_DCMS.APPLICATION.Features.Sessions.Queries.GetSessionById
{
    public record GetSessionByIdQuery(int SessionId) : IRequest<Result<SessionDto>>;
}

using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Session;

namespace ZU_DCMS.APPLICATION.Features.Sessions.Queries.GetTodaySessions
{
    public record GetTodaySessionsQuery : IRequest<Result<List<SessionDto>>>;
}

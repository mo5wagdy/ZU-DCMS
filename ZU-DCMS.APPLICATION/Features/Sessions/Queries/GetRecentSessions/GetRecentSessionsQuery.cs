using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Session;

namespace ZU_DCMS.APPLICATION.Features.Sessions.Queries.GetRecentSessions
{
    public record GetRecentSessionsQuery(int DaysCount = 7) : IRequest<Result<List<SessionDto>>>;
}

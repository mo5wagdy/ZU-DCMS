using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Session;

namespace ZU_DCMS.APPLICATION.Features.Sessions.Queries.FindSession
{
    public record FindSessionQuery(DateTime Date, string TimeSlot) : IRequest<Result<SessionDto>>;
}

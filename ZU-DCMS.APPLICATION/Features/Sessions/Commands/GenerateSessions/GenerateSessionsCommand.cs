using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Session;

namespace ZU_DCMS.APPLICATION.Features.Sessions.Commands.GenerateSessions
{
    public record GenerateSessionsCommand(DateTime StartDate, int DaysCount = 1) : IRequest<Result<List<SessionDto>>>;
}

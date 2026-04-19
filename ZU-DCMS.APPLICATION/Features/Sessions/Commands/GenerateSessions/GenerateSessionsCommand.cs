using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Session;

namespace ZU_DCMS.APPLICATION.Features.Sessions.Commands.GenerateSessions
{
    public record GenerateSessionsCommand(DateTime Date) : IRequest<Result<List<SessionDto>>>;
}

using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.APPLICATION.Features.Sessions.Queries.IsSessionAvailable
{
    public record IsSessionAvailableQuery(int SessionId, BookingType BookingType) : IRequest<Result<bool>>
}

using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Session;
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.APPLICATION.Features.Sessions.Queries.GetAvailableSlots
{
    public record GetAvailableSlotsQuery(BookingType BookingType) : IRequest<Result<List<AvailableSlotDto>>>;
}

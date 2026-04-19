using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Booking;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Bookings.Queries.GetSessionBookings
{
    public class GetSessionBookingsHandler : IRequestHandler<GetSessionBookingsQuery, Result<PagedResult<BookingDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IAppLogger<GetSessionBookingsHandler> _logger;

        public GetSessionBookingsHandler(IUnitOfWork uow, IMapper mapper, IAppLogger<GetSessionBookingsHandler> logger)
        {
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
        }

        // __ Get confirmed bookings for the session orderd by first booking created for the session, including patient details __ //
        public async Task<Result<PagedResult<BookingDto>>> Handle(GetSessionBookingsQuery query, CancellationToken cancellationToken)
        {
            _logger.LogInfo("Fetching confirmed bookings for SessionId: {SessionId}", query.SessionId);

            // __ Get confirmed bookings for the session, ordered by creation time, including patient details __ //
            var (items, total) = await _uow.Repository<Domain.Entities.Booking>().GetPagedListAsync
                (
                    (query.Request.Page - 1) * query.Request.PageSize,
                    query.Request.PageSize,
                    b => b.SessionId == query.SessionId && b.Status == BookingStatus.Confirmed,
                    true,
                    q => q.OrderBy(b => b.CreatedAt),
                    b => b.Patient
                );

            // __ Map to DTOs __ //
            var dtos = _mapper.Map<List<BookingDto>>(items);

            // __ Return paged result __ //
            var pagedResult = PagedResult<BookingDto>.Create(dtos, total, query.Request);

            _logger.LogInfo("Fetched {Count} confirmed bookings for SessionId: {SessionId}", dtos.Count, query.SessionId);

            return Result.Success(pagedResult);
        }
    }
}

using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Booking;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Bookings.Queries.GetPatientBookings
{
    public class GetPatientBookingsHandler : IRequestHandler<GetPatientBookingsQuery, Result<PagedResult<BookingDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IAppLogger<GetPatientBookingsHandler> _logger;

        public GetPatientBookingsHandler(IUnitOfWork uow, IMapper mapper, IAppLogger<GetPatientBookingsHandler> logger)
        {
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
        }

        // __ Get patient bookings with pagination __ //
        public async Task<Result<PagedResult<BookingDto>>> Handle(GetPatientBookingsQuery query, CancellationToken cancellationToken)
        {
            _logger.LogInfo("Fetching bookings for patient {PatientId} - Page: {Page}, PageSize: {PageSize}", query.PatientId, query.Request.Page, query.Request.PageSize);

            // __ Get total count and paged items in one query __ //
            var (items, total) = await _uow.Repository<Domain.Entities.Booking>().GetPagedListAsync
                (
                    (query.Request.Page - 1) * query.Request.PageSize,
                    query.Request.PageSize,
                    b => b.PatientId == query.PatientId,
                    true,
                    q => q.OrderByDescending(b => b.CreatedAt),
                    b => b.Session,
                    b => b.Payment
                );

            // __ Map to DTOs __ //
            var dtos = _mapper.Map<List<BookingDto>>(items);

            // __ Return paged result __ // 
            var pagedResult = PagedResult<BookingDto>.Create(dtos, total, query.Request);

            _logger.LogInfo("Fetched {Count} bookings for patient {PatientId}", dtos.Count, query.PatientId);

            return Result.Success<PagedResult<BookingDto>>(pagedResult);
        }
    }
}

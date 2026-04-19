using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Booking;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Bookings.Queries.GetBookingById
{
    public class GetBookingByIdHandler : IRequestHandler<GetBookingByIdQuery, Result<BookingDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IAppLogger<GetBookingByIdHandler> _logger;

        public GetBookingByIdHandler(IUnitOfWork uow, IMapper mapper, IAppLogger<GetBookingByIdHandler> logger)
        {
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
        }

        // __ Get booking by ID with full details __ //
        public async Task<Result<BookingDto>> Handle(GetBookingByIdQuery query, CancellationToken cancellationToken)
        {
            _logger.LogInfo("Fetching booking details for BookingId: {BookingId}", query.BookingId);

            // __ Load booking with related entities __ //
            var booking = await _uow.Repository<Domain.Entities.Booking>().GetFirstOrDefaultAsync
            (
                b => b.Id == query.BookingId,
                false,
                b => b.Patient,
                b => b.Session,
                b => b.Payment
            );

            // __ Check if booking exists __ //
            if (booking is null)
            {
                _logger.LogWarning("Booking not found for BookingId: {BookingId}", query.BookingId);
               
                return Result.Failure<BookingDto>("الحجز غير موجود");
            }

            _logger.LogInfo("Booking details fetched for BookingId: {BookingId}", query.BookingId);

            // __ Map to DTO and return __ //
            return Result.Success(_mapper.Map<BookingDto>(booking));
        }
    }
}

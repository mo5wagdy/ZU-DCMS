using AutoMapper;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Booking;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Booking.Queries.GetBookingById
{
    public class GetBookingByIdHandler
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

        public async Task<Result<BookingDto>> Handle(GetBookingByIdQuery query)
        {
            _logger.LogInfo("Fetching booking details for BookingId: {BookingId}", query.BookingId);

            var booking = await _uow.Repository<Domain.Entities.Booking>().GetFirstOrDefaultAsync
            (
                b => b.Id == query.BookingId,
                false,
                b => b.Patient,
                b => b.Session,
                b => b.Payment
            );

            if (booking is null)
            {
                _logger.LogWarning("Booking not found for BookingId: {BookingId}", query.BookingId);
                return Result.Failure<BookingDto>("الحجز غير موجود");
            }

            _logger.LogInfo("Booking details fetched for BookingId: {BookingId}", query.BookingId);

            return Result.Success(_mapper.Map<BookingDto>(booking));
        }
    }
}

using AutoMapper;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Payment;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Payment.Queries.GetPaymentByBookingId
{
    public class GetPaymentByBookingIdHandler
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IAppLogger<GetPaymentByBookingIdHandler> _logger;

        public GetPaymentByBookingIdHandler(
            IUnitOfWork uow,
            IMapper mapper,
            IAppLogger<GetPaymentByBookingIdHandler> logger)
        {
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<PaymentDto>> Handle(GetPaymentByBookingIdQuery query)
        {
            var bookingId = query.BookingId;

            _logger.LogInfo("Retrieving payment for BookingId: {BookingId}", bookingId);

            var payment = await _uow.Repository<Domain.Entities.Payment>().GetFirstOrDefaultAsync(p => p.BookingId == bookingId);

            if (payment == null)
            {
                _logger.LogWarning("No payment found for BookingId: {BookingId}", bookingId);
                return Result.Failure<PaymentDto>("الدفع غير موجود");
            }

            return Result.Success(_mapper.Map<PaymentDto>(payment));
        }
    }
}

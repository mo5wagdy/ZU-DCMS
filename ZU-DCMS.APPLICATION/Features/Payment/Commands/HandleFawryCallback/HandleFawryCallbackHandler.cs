using ZU_DCMS.APPLICATION.Background_Jobs.Events;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Payment.Events;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.Services.Interfaces;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Payment.Commands.HandleFawryCallback
{
    public class HandleFawryCallbackHandler
    {
        private readonly IUnitOfWork _uow;
        private readonly IFawrySignatureService _signatureService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IAppLogger<HandleFawryCallbackHandler> _logger;

        public HandleFawryCallbackHandler(
            IUnitOfWork uow,
            IFawrySignatureService signatureService,
            IEventPublisher eventPublisher,
            IAppLogger<HandleFawryCallbackHandler> logger)
        {
            _uow = uow;
            _signatureService = signatureService;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task<Result> Handle(HandleFawryCallbackCommand command)
        {
            var dto = command.Dto;

            _logger.LogInfo("Handling Fawry callback for PaymentCode: {PaymentCode} with Amount: {Amount}", dto.PaymentCode, dto.Amount);

            if (!_signatureService.IsValid(dto))
            {
                _logger.LogWarning("Invalid signature for payment {Code}", dto.PaymentCode);
                return Result.Failure("طلب غير موثوق");
            }

            var payment = await _uow.Repository<Domain.Entities.Payment>().GetFirstOrDefaultAsync
                (
                    p => p.PaymentCode == dto.PaymentCode,
                    false,
                    p => p.Booking
                );

            if (payment == null)
            {
                _logger.LogWarning("Unknown payment code: {Code}", dto.PaymentCode);
                return Result.Success();
            }

            if (payment.Status == PaymentStatus.Paid)
            {
                _logger.LogInfo("Payment already marked as paid for PaymentId: {PaymentId}", payment.Id);
                return Result.Success();
            }

            if (payment.Amount != dto.Amount)
            {
                _logger.LogError("Amount mismatch for Payment.");
                return Result.Failure("مبلغ غير صحيح");
            }

            await _uow.BeginTransactionAsync();

            try
            {
                payment.Status = PaymentStatus.Paid;
                payment.PaidAt = dto.PaidAt;
                payment.GatewayReference = dto.GatewayReference;

                _uow.Repository<Domain.Entities.Payment>().Update(payment);

                if (payment.Booking != null)
                {
                    payment.Booking.Status = BookingStatus.Confirmed;
                    _uow.Repository<Booking>().Update(payment.Booking);
                }

                await _uow.CommitTransactionAsync();

                await _eventPublisher.PublishAsync(new PaymentCompletedEvent(payment.Id, payment.BookingId));

                return Result.Success();
            }
            catch (Exception ex)
            {
                await _uow.RollbackTransactionAsync();
                _logger.LogError("Error in callback for PaymentId: {PaymentId}", ex, payment.Id);
                return Result.Failure("خطأ في الدفع");
            }
        }
    }
}

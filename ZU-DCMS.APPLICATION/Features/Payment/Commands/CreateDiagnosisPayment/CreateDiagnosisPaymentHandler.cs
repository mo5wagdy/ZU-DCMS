using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Payment.Commands.CreateDiagnosisPayment
{
    public class CreateDiagnosisPaymentHandler
    {
        private readonly IUnitOfWork _uow;
        private readonly IAppLogger<CreateDiagnosisPaymentHandler> _logger;

        public CreateDiagnosisPaymentHandler(
            IUnitOfWork uow,
            IAppLogger<CreateDiagnosisPaymentHandler> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<Result<Domain.Entities.Payment>> Handle(CreateDiagnosisPaymentCommand command)
        {
            var patientId = command.PatientId;
            var bookingId = command.BookingId;
            var amount = command.Amount;

            _logger.LogInfo("Creating diagnosis payment for BookingId: {BookingId} with Amount: {Amount}", bookingId, amount);

            var payment = new Domain.Entities.Payment
            {
                PatientId = patientId,
                BookingId = bookingId,
                Amount = amount,
                Type = PaymentType.Diagnosis,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.Repository<Domain.Entities.Payment>().AddAsync(payment);

            _logger.LogInfo("Payment created for BookingId: {BookingId}", bookingId);

            return Result.Success(payment);
        }
    }
}

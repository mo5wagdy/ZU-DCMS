using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.Services.Interfaces;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Payment.Commands.GeneratePaymentCode
{
    public class GeneratePaymentCodeHandler
    {
        private readonly IUnitOfWork _uow;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IAppLogger<GeneratePaymentCodeHandler> _logger;

        public GeneratePaymentCodeHandler(
            IUnitOfWork uow,
            IPaymentGateway paymentGateway,
            IAppLogger<GeneratePaymentCodeHandler> logger)
        {
            _uow = uow;
            _paymentGateway = paymentGateway;
            _logger = logger;
        }

        public async Task<Result> Handle(GeneratePaymentCodeCommand command)
        {
            var payment = command.Payment;

            _logger.LogInfo("Generating payment code for PaymentId: {PaymentId}", payment.Id);

            if (payment.Status != PaymentStatus.Pending)
            {
                _logger.LogWarning("Cannot generate payment code for PaymentId: {PaymentId} because it is not pending", payment.Id);
                return Result.Failure("لا يمكن توليد كود دفع");
            }

            var result = await _paymentGateway.GeneratePaymentCodeAsync(payment.Id, payment.Amount);

            if (!result.IsSuccess)
            {
                _logger.LogError("Payment gateway failed for Payment");
                return Result.Failure("فشل توليد كود الدفع");
            }

            payment.PaymentCode = result.PaymentCode;
            payment.GatewayName = result.GatewayName;
            payment.GatewayReference = result.GatewayReference;

            _uow.Repository<Domain.Entities.Payment>().Update(payment);

            return Result.Success();
        }
    }
}


using AutoMapper;
using ZU_DCMS.APPLICATION.Background_Jobs.Events;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Payment.Events;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Payment;
using ZU_DCMS.APPLICATION.Services.Interfaces;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IFawrySignatureService _signatureService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IMapper _mapper;
        private readonly IAppLogger<PaymentService> _logger;

        public PaymentService(
            IUnitOfWork uow,
            IPaymentGateway paymentGateway,
            IFawrySignatureService signatureService,
            IEventPublisher eventPublisher,
            IMapper mapper,
            IAppLogger<PaymentService> logger)
        {
            _uow = uow;
            _paymentGateway = paymentGateway;
            _signatureService = signatureService;
            _eventPublisher = eventPublisher;
            _mapper = mapper;
            _logger = logger;
        }

        // __ Create a payment for diagnosis based on the provided booking ID and amount. __ //
        public async Task<Result<Payment>> CreateDiagnosisPaymentAsync(int patientId, int bookingId, decimal amount)
        {
            _logger.LogInfo("Creating diagnosis payment for BookingId: {BookingId} with Amount: {Amount}", bookingId, amount);

            // __ Create a new payment record with the provided details and set its status to pending. __ //
            var payment = new Payment
            {
                PatientId = patientId,
                BookingId = bookingId,
                Amount = amount,
                Type = PaymentType.Diagnosis,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            // __ Add the new payment record to the database asynchronously. __ //
            await _uow.Repository<Payment>().AddAsync(payment);

            _logger.LogInfo("Payment created for BookingId: {BookingId}", bookingId);

            return Result.Success(payment);
        }

        // __ Generate a payment code for the given payment if its status is pending. __ //
        public async Task<Result> GeneratePaymentCodeAsync(Payment payment)
        {
            _logger.LogInfo("Generating payment code for PaymentId: {PaymentId}", payment.Id);

            // __ Check if the payment status is pending before attempting to generate a payment code. __ //
            if (payment.Status != PaymentStatus.Pending)
            {
                _logger.LogWarning("Cannot generate payment code for PaymentId: {PaymentId} because it is not pending", payment.Id);

                return Result.Failure("لا يمكن توليد كود دفع");
            }

            // __ Call the payment gateway to generate a payment code for the given payment ID and amount. __ //
            var result = await _paymentGateway.GeneratePaymentCodeAsync(payment.Id, payment.Amount);

            // __ If the payment gateway fails to generate a payment code, log an error and return a failure result. __ //
            if (!result.IsSuccess)
            {
                _logger.LogError("Payment gateway failed for Payment");
                
                return Result.Failure("فشل توليد كود الدفع");
            }

            // __ Update the payment record with the generated payment code, gateway name, and gateway reference, then save the changes to the database. __ //
            payment.PaymentCode = result.PaymentCode;
            payment.GatewayName = result.GatewayName;
            payment.GatewayReference = result.GatewayReference;

            // __ Update the payment record in the database with the new information. __ //
            _uow.Repository<Payment>().Update(payment);

            return Result.Success();
        }

        // __ Retrieve payment details based on the provided booking ID. __ //
        public async Task<Result<PaymentDto>> GetByBookingIdAsync(int bookingId)
        {
            _logger.LogInfo("Retrieving payment for BookingId: {BookingId}", bookingId);

            // __ Fetch the first payment record from the database that matches the given booking ID. __ //
            var payment = await _uow.Repository<Payment>().GetFirstOrDefaultAsync(p => p.BookingId == bookingId);

            // __ If no payment record is found for the given booking ID, log a warning and return a failure result. __ //
            if (payment == null)
            {
                _logger.LogWarning("No payment found for BookingId: {BookingId}", bookingId);
                
                return Result.Failure<PaymentDto>("الدفع غير موجود");
            }

            // __ If a payment record is found, map it to a PaymentDto and return it as a success result. __ //
            return Result.Success(_mapper.Map<PaymentDto>(payment));
        }

        // __ Handle the callback from Fawry payment gateway by validating the payment code, amount, and updating the payment status accordingly. __ // 
        public async Task<Result> HandleFawryCallbackAsync(FawryCallbackDto dto)
        {
            _logger.LogInfo("Handling Fawry callback for PaymentCode: {PaymentCode} with Amount: {Amount}", dto.PaymentCode, dto.Amount);

            // __ Validate the signature of the callback data to ensure it is from a trusted source. If the signature is invalid, log a warning and return a failure result indicating an untrusted request. __ //
            if (!_signatureService.IsValid(dto))
            {
                _logger.LogWarning("Invalid signature for payment {Code}", dto.PaymentCode);
               
                return Result.Failure("طلب غير موثوق");
            }

            // __ Retrieve the payment record from the database that matches the provided payment code, including its associated booking information. __ //
            var payment = await _uow.Repository<Payment>().GetFirstOrDefaultAsync
                (
                    p => p.PaymentCode == dto.PaymentCode,
                    false,
                    p => p.Booking
                );

            // __ If no payment record is found for the provided payment code, log a warning and return a success result to acknowledge the callback without processing further. __ //
            if (payment == null)
            {
                _logger.LogWarning("Unknown payment code: {Code}", dto.PaymentCode);
                
                return Result.Success();
            }

            // __ If the payment status is already marked as paid, log an informational message and return a success result to avoid duplicate processing. __ //
            if (payment.Status == PaymentStatus.Paid)
            {
                _logger.LogInfo("Payment already marked as paid for PaymentId: {PaymentId}", payment.Id);
                
                return Result.Success();
            }

            // __ Validate that the amount received in the callback matches the amount of the payment record. If there is a mismatch, log an error and return a failure result indicating an incorrect amount. __ //
            if (payment.Amount != dto.Amount)
            {
                _logger.LogError("Amount mismatch for Payment.");
              
                return Result.Failure("مبلغ غير صحيح");
            }

            // __ Begin a database transaction to ensure that the payment status update and any related booking status updates are atomic operations. __ //
            await _uow.BeginTransactionAsync();

            try
            {
                // __ Update the payment status to paid, set the paid timestamp and gateway reference, then save the changes to the database. __ //
                payment.Status = PaymentStatus.Paid;
                payment.PaidAt = dto.PaidAt;
                payment.GatewayReference = dto.GatewayReference;

                _uow.Repository<Payment>().Update(payment);

                // __ If the payment is associated with a booking, update the booking status to confirmed and save the changes to the database. __ //
                if (payment.Booking != null)
                {
                    payment.Booking.Status = BookingStatus.Confirmed;
                   
                    _uow.Repository<Booking>().Update(payment.Booking);
                }

                // __ Commit the database transaction to persist the changes. __ //
                await _uow.CommitTransactionAsync();

                // __ After successfully processing the payment, publish a PaymentCompletedEvent to notify other parts of the system about the completed payment. __ //
                await _eventPublisher.PublishAsync(new PaymentCompletedEvent(payment.Id, payment.BookingId));

                return Result.Success();
            }

            // __ If any exception occurs during the transaction, roll back the transaction, log the error, and return a failure result indicating an error in processing the payment. __ //
            catch (Exception ex)
            {
                await _uow.RollbackTransactionAsync();

                _logger.LogError("Error in callback for PaymentId: {PaymentId}", ex, payment.Id);

                return Result.Failure("خطأ في الدفع");
            }
        }
    }
}


using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Payment;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.APPLICATION.Services.Interfaces
{
    // __ This interface defines the contract for payment-related operations in the application. __ //
    public interface IPaymentService
    {
        // __ Creates a payment for a patient diagnosis based on the provided booking ID and amount. __ //
        Task<Result<Payment>> CreateDiagnosisPaymentAsync(int patientId, int bookingId, decimal amount);

        /* 
         * Generates a payment code for a specific booking,
         * allowing users to initiate the payment process for their booking and receive the necessary information to complete the payment.
         */
        Task<Result> GeneratePaymentCodeAsync(Payment payment);

        // __ Retrieves detailed information about a specific payment based on the booking ID, __ //
        Task<Result<PaymentDto>> GetByBookingIdAsync(int bookingId);

        // __ Handles the callback from Fawry, a payment gateway, by processing the provided callback data. __ //
        Task<Result> HandleFawryCallbackAsync(FawryCallbackDto dto);
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.DTOs.Payment;

namespace ZU_DCMS.APPLICATION.Services.Interfaces
{
    // This interface defines the contract for payment-related operations in the application.
    public interface IPaymentService
    {
        // Generates a payment code for a specific booking,
        // allowing users to initiate the payment process for their booking and receive the necessary information to complete the payment.
        Task<PaymentDto> GeneratePaymentCodeAsync(int bookingId);

        // Retrieves detailed information about a specific payment based on the booking ID,
        Task<PaymentDto?> GetByBookingIdAsync(int bookingId);

        // Handles the callback from Fawry, a payment gateway, by processing the provided callback data,
        Task HandleFawryCallbackAsync(FawryCallbackDto dto);
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.ValueObjects;

namespace ZU_DCMS.APPLICATION.Contracts
{
    // This interface defines the contract for a payment gateway service that can generate payment codes, cancel payment codes, and verify payments.
    public interface IPaymentGateway
    {
        // This method generates a payment code for a given booking and amount.
        Task<PaymentResult> GeneratePaymentCodeAsync(int bookingId, decimal amount);

        // This method cancels a payment code, returning true if the cancellation was successful.
        Task<bool> CancelPaymentCodeAsync(string paymentCode);

        // This method verifies a payment using the gateway reference, returning true if the payment is valid.
        Task<bool> VerifyPaymentAsync(string gatewayReference);
    }
}

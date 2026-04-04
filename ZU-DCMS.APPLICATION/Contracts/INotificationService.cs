using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.Contracts
{
    // This interface defines the contract for a notification service that can send various types of notifications
    // (SMS, WhatsApp, Email) and specific booking-related notifications.
    public interface INotificationService
    {
        // This method sends an SMS message to a specified phone number with the given message content.
        Task<bool> SendSmsAsync(string phoneNumber, string message);

        // This method sends a WhatsApp message to a specified phone number with the given message content.
        Task<bool> SendWhatsAppAsync(string phoneNumber, string message);

        // This method sends an email to a specified email address with the given subject and message content.
        Task<bool> SendEmailAsync(string email, string subject, string message);

        // This method sends a notification to the student when a booking is confirmed, using the booking ID to retrieve necessary details.
        Task SendBookingConfirmedAsync(int bookingId);

        // This method sends a notification to the student when they are assigned to a case, using the case assignment ID to retrieve necessary details.
        Task SendStudentAssignedAsync(int caseAssignmentId);

        // This method sends a notification to the student when a case they are assigned to is completed, using the case assignment ID to retrieve necessary details.
        Task SendCaseCompletedAsync(int caseAssignmentId);

        // This method sends a notification to the student when a booking is cancelled, using the booking ID to retrieve necessary details.
        Task SendBookingCancelledAsync(int bookingId);

        // This method sends a notification to the student when a booking is postponed, using the booking ID and reason for postponement to retrieve necessary details.
        Task SendBookingPostponedAsync(int bookingId, string reason);
    }
}

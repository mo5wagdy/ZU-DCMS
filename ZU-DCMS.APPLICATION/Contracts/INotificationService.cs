using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.Contracts
{
    /* 
     * This interface defines the contract for a notification service that can send various types of notifications
     * (SMS, WhatsApp, Email) and specific booking-related notifications.
    */
    public interface INotificationService
    {
        /* 
         * These methods are responsible for sending different types of notifications to users.
         * Each method returns a Task<bool> indicating the success or failure of the notification sending operation.
         */
        Task<bool> SendSmsAsync(string phoneNumber, string message); // => This method sends an SMS message to a specified phone number with the given message content.
        Task<bool> SendWhatsAppAsync(string phoneNumber, string message); // => This method sends a WhatsApp message to a specified phone number with the given message content.
        Task<bool> SendEmailAsync(string email, string subject, string message); // => This method sends an email to a specified email address with the given subject and message content.

        /*
         * These methods are responsible for sending specific notifications related to bookings and case assignments.
         * Each method takes relevant identifiers (like booking ID or case assignment ID) to retrieve necessary details for the notification.  
         */

        // __ Booking-related notifications __ //
        Task SendBookingConfirmedAsync(int bookingId); // => This method sends a notification to the Intern Doctor and patient when a booking is confirmed, using the booking ID to retrieve necessary details
        Task SendBookingCancelledAsync(int bookingId); // => This method sends a notification to the Intern Doctor and patient when a booking is cancelled, using the booking ID to retrieve necessary details.
        Task SendBookingPostponedAsync(int bookingId, string reason); // => This method sends a notification to the all when a booking is postponed, using the booking ID and reason for postponement to retrieve necessary details.

        // __ Payment-related notifications __ //
        Task SendPaymentPaidAsync(int bookingId); // => This method sends a notification to the patient when a payment for a booking is completed, using the booking ID to retrieve necessary details.

        // __ Diagnosis-related notifications __ //
        Task SendDiagnosisCreatedAsync(int diagnosisId); // => This method sends a notification when a diagnosis is created, using the diagnosis ID to retrieve necessary details.

        // __ Case assignment-related notifications __ //
        Task SendStudentAssignedAsync(int caseAssignmentId); // => This method sends a notification to the student and patient when they are assigned to a case, using the case assignment ID to retrieve necessary details.

        // __ Case progress-related notifications __ //
        Task SendCasePartiallyCompletedAsync(int caseAssignmentId);
        Task SendCaseCompletedAsync(int caseAssignmentId); // => This method sends a notification to the Intern Doctor when a case they are assigned to is completed, using the case assignment ID to retrieve necessary details.
    }
}

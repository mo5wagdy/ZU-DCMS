using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Booking;
using ZU_DCMS.APPLICATION.DTOs.Session;

namespace ZU_DCMS.APPLICATION.Services.Interfaces
{
    /* 
     * Interface for booking service to handle operations related to booking appointments,
     * retrieving available slots, and managing patient bookings
    */
    public interface IBookingService
    {
        /* 
         * Method to create a new booking for a patient using the provided patient ID and booking data transfer object,
         * returning the created booking as a data transfer object
        */
        Task<Result<BookingDto>> CreateBookingAsync(int patientId, CreateBookingDto dto);

        /* 
         * Method to retrieve booking information by booking ID, returning a data transfer object if found
        */
        Task<Result<BookingDto>> GetByIdAsync(int id);

        /* 
         * Method to retrieve a paginated list of bookings for a specific patient using the patient ID and pagination request,
         * returning a paged result containing booking data transfer objects
        */
        Task<Result<PagedResult<BookingDto>>> GetPatientBookingsAsync(int patientId, PagedRequest request);

        /* 
         * Method to retrieve a paginated list of bookings for a specific session using the session ID and pagination request,
         * returning a paged result containing booking data transfer objects
        */
        Task<Result<PagedResult<BookingDto>>> GetSessionBookingsAsync(int sessionId, PagedRequest request);

        // __ Method to cancel a booking for a specific booking ID and patient ID, performing the cancellation operation asynchronously __ //
        Task<Result> CancelBookingAsync(int bookingId, int patientId);

        // __ Method to postpone a booking for a specific booking ID, providing a reason for postponement and the admin ID responsible for the action. __ //
        Task<Result> PostponeBookingAsync(int bookingId, string reason, string adminId);
    }
}

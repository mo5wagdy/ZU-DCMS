using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Booking;

namespace ZU_DCMS.APPLICATION.Services.Interfaces
{
    // Interface for booking service to handle operations related to booking appointments, retrieving available slots, and managing patient bookings
    public interface IBookingService
    {
        // Method to create a new booking for a patient using the provided patient ID and booking data transfer object,
        // returning the created booking as a data transfer object
        Task<BookingDto> CreateBookingAsync(int patientId, CreateBookingDto dto);

        // Method to retrieve a list of available booking slots for a specific booking type, returning a list of available slot data transfer objects
        Task<List<AvailableSlotDto>> GetAvailableSlotsAsync(string bookingType);

        // Method to retrieve booking information by booking ID, returning a data transfer object if found
        Task<BookingDto?> GetByIdAsync(int id);

        // Method to retrieve a paginated list of bookings for a specific patient using the patient ID and pagination request,
        // returning a paged result containing booking data transfer objects
        Task<PagedResult<BookingDto>> GetPatientBookingsAsync(int patientId, PagedRequest request);

        // Method to cancel a booking for a specific booking ID and patient ID, performing the cancellation operation asynchronously
        Task CancelBookingAsync(int bookingId, int patientId);

        // Method to postpone a booking for a specific booking ID, providing a reason for postponement and the admin ID responsible for the action,
        Task PostponeBookingAsync(int bookingId, string reason, string adminId);
    }
}

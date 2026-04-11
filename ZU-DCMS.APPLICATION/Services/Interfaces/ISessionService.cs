using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Booking;
using ZU_DCMS.APPLICATION.DTOs.Session;
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.APPLICATION.Services.Interfaces
{
    /* 
     * Interface for session service to handle operations related to managing sessions,
     * retrieving session information, and generating sessions for specific dates 
     */
    public interface ISessionService
    {
        Task<Result<List<SessionDto>>> GenerateSessionsAsync(DateTime date); // => Method to generate sessions for a specific date, creating the necessary session entries in the system based on the provided date
        Task<Result<List<AvailableSlotDto>>> GetAvailableSlotsAsync(BookingType bookingType); // => Available Slots (for booking screen)
        Task<Result<List<SessionDto>>> GetTodaySessionsAsync(); // => Method to retrieve a list of sessions scheduled for the current day, returning a list of session data transfer objects
        Task<Result<bool>> IsSessionAvailableAsync(int sessionId, BookingType bookingType); // => Method to check if a specific session is available for booking based on the session ID and booking type, returning a boolean indicating availability
        Task<Result<SessionDto>> GetByIdAsync(int id); // => Method to retrieve session information by session ID, returning a data transfer object if found
    }
}

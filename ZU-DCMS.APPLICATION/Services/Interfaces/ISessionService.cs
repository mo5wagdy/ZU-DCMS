using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.DTOs.Session;

namespace ZU_DCMS.APPLICATION.Services.Interfaces
{
    // Interface for session service to handle operations related to managing sessions, retrieving session information, and generating sessions for specific dates
    public interface ISessionService
    {
        // Method to retrieve a list of sessions scheduled for the current day, returning a list of session data transfer objects
        Task<List<SessionDto>> GetTodaySessionsAsync();

        // Method to retrieve session information by session ID, returning a data transfer object if found
        Task<SessionDto?> GetByIdAsync(int id);

        // Method to generate sessions for a specific date, creating the necessary session entries in the system based on the provided date
        Task GenerateSessionsAsync(DateTime date);

        // Method to check if a specific session is available for booking based on the session ID and booking type, returning a boolean indicating availability
        Task<bool> IsSessionAvailableAsync(int sessionId, string bookingType);
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.DTOs.Dashboard;
using ZU_DCMS.APPLICATION.DTOs.Student;

namespace ZU_DCMS.APPLICATION.Services.Interfaces
{
    // This interface defines the contract for dashboard-related operations in the application,
    public interface IDashboardService
    {
        // Retrieves today's statistics for the dashboard,
        // allowing users to view key metrics and insights related to the application's performance and usage for the current day.
        Task<DashboardDto> GetTodayStatsAsync();

        // Retrieves the status of clinics for the dashboard,
        Task<List<ClinicStatusDto>> GetClinicsStatusAsync();

        // Retrieves the progress of students for a specific term for the dashboard,
        Task<List<StudentProgressDto>> GetStudentsProgressAsync(int termId);
    }
}

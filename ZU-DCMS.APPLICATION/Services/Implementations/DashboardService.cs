using ZU_DCMS.APPLICATION.DTOs.Dashboard;
using ZU_DCMS.APPLICATION.DTOs.Student;
using ZU_DCMS.APPLICATION.Services.Interfaces;

namespace ZU_DCMS.APPLICATION.Services.Implementations
{
    internal class DashboardService : IDashboardService
    {
        public async Task<DashboardDto> GetTodayStatsAsync()
        {
            /*
            1. Define today's date range (00:00 → 23:59)
            2. Fetch:
                - Total sessions today
                - Completed cases today
                - Active students today
                - New assignments today
                - Any cancellations / follow-ups

            3. Aggregate metrics into DashboardDto
            4. (Optional) Pull cached stats first (Redis)
            5. Return DTO
            */

            throw new NotImplementedException();
        }

        public async Task<List<ClinicStatusDto>> GetClinicsStatusAsync()
        {
            /*
            1. Fetch all clinics
            2. For each clinic:
                - Count active students
                - Count ongoing cases
                - Count completed sessions today
                - Check availability/load status

            3. Map each clinic → ClinicStatusDto
            4. Return list ordered by load / activity
            */

            throw new NotImplementedException();
        }

        public async Task<List<StudentProgressDto>> GetStudentsProgressAsync(int termId)
        {
            /*
            1. Validate term exists (optional but recommended)
            2. Fetch all students assigned in term
            3. Include:
                - CaseAssignments
                - Sessions
                - Completion status

            4. For each student:
                - Calculate total assigned cases
                - Calculate completed cases
                - Calculate progress percentage

            5. Map to StudentProgressDto
            6. Sort by progress (desc or asc depending on dashboard UX)
            7. Return list
            */

            throw new NotImplementedException();
        }
    }
}
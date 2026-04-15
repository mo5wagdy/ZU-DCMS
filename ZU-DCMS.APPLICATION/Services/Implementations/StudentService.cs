using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Student;
using ZU_DCMS.APPLICATION.Services.Interfaces;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Services.Implementations
{
    internal class StudentService : IStudentService
    {
        private readonly IRawSqlExecutor _sql;

        public StudentService(IRawSqlExecutor sql)
        {
            _sql = sql;   
        }
        public async Task<StudentDto?> GetByIdAsync(int id)
        {
            /*
            1. Fetch student by Id
            2. Include:
                - User info
                - Department / Clinic (if exists)
                - Current term data
                - Case assignments summary

            3. If not found → return null
            4. Map to StudentDto
            5. Return result
            */

            throw new NotImplementedException();
        }

        public async Task<StudentDto?> GetByUserIdAsync(string userId)
        {
            /*
            1. Fetch student where Student.UserId == userId
            2. Include same related data as GetByIdAsync
            3. If not found → return null
            4. Map to StudentDto
            5. Return result
            */

            throw new NotImplementedException();
        }

        public async Task<PagedResult<StudentDto>> GetAllAsync(PagedRequest request)
        {
            /*
            1. Build base query from Students table
            2. Apply filters if exists:
                - Search by name / email / code
                - Filter by clinic / department / term (if needed)

            3. Apply pagination:
                - Skip = (Page - 1) * PageSize
                - Take = PageSize

            4. Include:
                - User info
                - Status
                - Progress summary (lightweight only)

            5. Map to StudentDto list

            6. Return PagedResult:
                - Items
                - TotalCount
            */

            throw new NotImplementedException();
        }

        public async Task<List<StudentRequirementDto>> GetRequirementsAsync(int studentId, int termId)
        {
            /*
            1. Fetch student requirements where:
                - StudentId == studentId
                - TermId == termId

            2. Include:
                - Requirement type
                - Progress status
                - Completion state

            3. Map to StudentRequirementDto
            4. Return list ordered by priority or creation date
            */

            throw new NotImplementedException();
        }

        public async Task IncrementRequirementAsync(int studentId, int clinicId, int termId)
        {
            var sql = @"
            UPDATE TermRequirements
            SET CompletedCount = CompletedCount + 1
            WHERE StudentId = @studentId
            AND ClinicId = @clinicId
            AND TermId = @termId";

            await _sql.ExecuteAsync(sql, new { studentId, clinicId, termId });
        }

    }
}
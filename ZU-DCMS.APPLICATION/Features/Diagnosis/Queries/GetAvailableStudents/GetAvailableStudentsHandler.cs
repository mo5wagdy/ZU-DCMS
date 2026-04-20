using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.DTOs.Student;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Diagnosis.Queries.GetAvailableStudents
{
    public class GetAvailableStudentsHandler : IRequestHandler<GetAvailableStudentsQuery, Result<List<StudentPriorityDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IAiAgentService _aiAgent;
        private readonly IAppLogger<GetAvailableStudentsHandler> _logger;

        public GetAvailableStudentsHandler(
            IUnitOfWork uow,
            IAiAgentService aiAgent,
            IAppLogger<GetAvailableStudentsHandler> logger)
        {
            _uow = uow;
            _aiAgent = aiAgent;
            _logger = logger;
        }

        /* 
         * This method is called by the intern doctor when they want to see the list of available students for case assignment.
         * It retrieves the term requirements for the clinic and term, calls the AI agent to get a prioritized list of student IDs,
         * and returns the list of students with their priority and completion status.
         */
        public async Task<Result<List<StudentPriorityDto>>> Handle(GetAvailableStudentsQuery query, CancellationToken cancellationToken)
        {
            var clinicId = query.ClinicId;
            
            var termId = query.TermId;

            // __ Get term requirements for the clinic and term, including student details __ //
            var requirements = await _uow.Repository<TermRequirement>()
                .GetListAsync
                (
                    r => r.ClinicId == clinicId && r.TermId == termId,
                    true,
                    r => r.Student
                );

            // __ If no requirements found, return empty list __ //
            if (!requirements.Any())
                return Result.Success(new List<StudentPriorityDto>());

            // __ List to hold prioritized student IDs from AI agent __ //
            IEnumerable<int> prioritizedIds;

            // __ Call AI agent to get prioritized list of student IDs based on requirements __ //
            try
            {
                prioritizedIds = await _aiAgent.GetStudentPriorityListAsync(clinicId, termId);
            }

            // __ If AI agent fails (e.g., due to an error or timeout), log the error and fall back to a default prioritization based on the Priority field in the requirements __ //
            catch (Exception ex)
            {
                _logger.LogError("AI fallback for clinic {ClinicId}", ex,  clinicId);

                prioritizedIds = requirements
                    .OrderBy(r => r.Priority)
                    .Select(r => r.StudentId);
            }

            // __ Create a dictionary of requirements for quick lookup by student ID __ //
            var dict = requirements.ToDictionary(r => r.StudentId);

            // __ Build the list of StudentPriorityDto based on the prioritized IDs, including completion status and priority ranking __ //
            var dtos = prioritizedIds
                .Where(dict.ContainsKey)
                .Select((studentId, index) =>
                {
                    var req = dict[studentId];

                    return new StudentPriorityDto
                    {
                        StudentId = studentId,
                        FullName = req.Student.FullName,
                        StudentCode = req.Student.StudentCode,
                        CompletedCases = req.CompletedCount,
                        RequiredCases = req.RequiredCount,
                        Priority = index + 1,
                        IsComplete = req.IsSatisfied
                    };
                }).ToList();

            // __ Return the list of students with their priority and completion status __ //
            return Result.Success(dtos);
        }
    }
}

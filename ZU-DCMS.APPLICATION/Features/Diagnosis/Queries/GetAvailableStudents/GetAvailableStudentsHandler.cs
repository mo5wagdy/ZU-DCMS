using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Student;
using ZU_DCMS.APPLICATION.Services.Interfaces;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Diagnosis.Queries.GetAvailableStudents
{
    public class GetAvailableStudentsHandler
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

        public async Task<Result<List<StudentPriorityDto>>> Handle(GetAvailableStudentsQuery query)
        {
            var clinicId = query.ClinicId;
            var termId = query.TermId;

            var requirements = await _uow.Repository<TermRequirement>()
                .GetListAsync
                (
                    r => r.ClinicId == clinicId && r.TermId == termId,
                    true,
                    r => r.Student
                );

            if (!requirements.Any())
                return Result.Success(new List<StudentPriorityDto>());

            List<int> prioritizedIds;

            try
            {
                prioritizedIds = await _aiAgent.GetStudentPriorityListAsync(clinicId, termId);
            }
            catch (Exception ex)
            {
                _logger.LogError("AI fallback for clinic {ClinicId}", ex,  clinicId);

                prioritizedIds = requirements
                    .OrderBy(r => r.Priority)
                    .Select(r => r.StudentId)
                    .ToList();
            }

            var dict = requirements.ToDictionary(r => r.StudentId);

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
                })
                .ToList();

            return Result.Success(dtos);
        }
    }
}

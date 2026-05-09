
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts.Engine;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Diagnosis.Engine
{
    public class AutoAssignmentEngine : IAutoAssignmentEngine
    {
        private readonly IUnitOfWork _uow;
        private readonly IAppLogger<AutoAssignmentEngine> _logger;

        public AutoAssignmentEngine(IUnitOfWork uow, IAppLogger<AutoAssignmentEngine> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<Result<int>> GetBestStudentForCaseAsync(DiagnosisRecord diagnosis)
        {
            try
            {
                // __ 1. Get clinic data to check constraints (Max Cases and Academic Year) __ //
                var clinic = await _uow.Repository<Clinic>().GetByIdAsync(diagnosis.ClinicId);
                if (clinic == null)
                {
                    _logger.LogWarning($"Clinic {diagnosis.ClinicId} not found for auto-assignment.");
                    return Result.Failure<int>("Clinic not found.");
                }

                // __ 2. Get all active students who meet academic year constraints __ //
                var studentsWithRequirements = await _uow.Repository<Student>().GetListAsync(
                    s => s.IsActive 
                      && s.ActiveTermId != null 
                      && s.AcademicYear >= clinic.MinAcademicYear 
                      && s.AcademicYear <= clinic.MaxAcademicYear,
                    true,
                    s => s.TermRequirements,
                    s => s.CaseAssignments
                );

                // __ 2.5. Load global requirement templates for fallback __ //
                var globalRequirements = await _uow.Repository<TermRequirement>().GetListAsync(
                    r => r.StudentId == null && r.ClinicId == clinic.Id && !r.IsDeleted
                );

                // __ 3. Filter students who haven't satisfied their requirements in this clinic __ //
                // __ Check individual requirements first, fallback to global templates __ //
                var eligibleStudents = new List<(Student Student, TermRequirement Requirement)>();

                foreach (var s in studentsWithRequirements)
                {
                    // Try individual requirement first
                    var req = s.TermRequirements.FirstOrDefault(r => r.ClinicId == clinic.Id && r.TermId == s.ActiveTermId);

                    // Fallback to global template if no individual requirement exists
                    if (req == null)
                    {
                        var globalReq = globalRequirements.FirstOrDefault(r => r.AcademicYear == s.AcademicYear && r.TermId == s.ActiveTermId);
                        if (globalReq != null)
                        {
                            // Calculate completed count dynamically for global templates
                            var completedCount = s.CaseAssignments.Count(ca => ca.ClinicId == clinic.Id && ca.TermId == s.ActiveTermId && ca.Status == CaseStatus.Approved);
                            
                            // Create a virtual requirement to evaluate eligibility
                            req = new TermRequirement
                            {
                                ClinicId = clinic.Id,
                                TermId = globalReq.TermId,
                                RequiredCount = globalReq.RequiredCount,
                                CompletedCount = completedCount,
                                AcademicYear = s.AcademicYear
                            };
                        }
                    }

                    if (req != null && !req.IsSatisfied)
                    {
                        eligibleStudents.Add((s, req));
                    }
                }

                if (!eligibleStudents.Any())
                {
                    _logger.LogInfo($"No eligible students with unsatisfied requirements found for Clinic {clinic.Id}.");
                    return Result.Failure<int>("No available students currently need this case.");
                }

                // __ 4. Exclude students who exceeded the maximum active cases in this clinic __ //
                // __ Active cases are: InProgress or PendingAssignmentApproval __ //
                var studentsUnderWorkloadLimit = eligibleStudents
                    .Select(x => new
                    {
                        x.Student,
                        x.Requirement,
                        ActiveCasesInClinic = x.Student.CaseAssignments.Count(ca => 
                            ca.ClinicId == clinic.Id && 
                            (ca.Status == CaseStatus.InProgress || ca.Status == CaseStatus.PendingAssignmentApproval))
                    })
                    .Where(x => x.ActiveCasesInClinic < clinic.MaxCasesPerStudent)
                    .ToList();

                if (!studentsUnderWorkloadLimit.Any())
                {
                    _logger.LogInfo($"All eligible students for Clinic {clinic.Id} have reached their MaxCasesPerStudent limit.");
                    return Result.Failure<int>("All available students have reached the maximum number of cases in this clinic.");
                }

                // __ 5. Calculate average completion speed for each student (in days) __ //
                // __ If the student hasn't completed any case, give a default average value to ensure fairness __ //
                var rankedStudents = studentsUnderWorkloadLimit.Select(x =>
                {
                    var completedCases = x.Student.CaseAssignments
                        .Where(ca => ca.Status == CaseStatus.Completed && ca.CompletedAt.HasValue)
                        .ToList();

                    double averageCompletionDays = 10.0; // Default value for new students
                    if (completedCases.Any())
                    {
                        averageCompletionDays = completedCases.Average(ca => (ca.CompletedAt!.Value - ca.AssignedAt).TotalDays);
                    }

                    var lastAssignedAt = x.Student.CaseAssignments.Any()
                        ? x.Student.CaseAssignments.Max(ca => ca.AssignedAt)
                        : DateTime.MinValue;

                    return new
                    {
                        x.Student.Id,
                        Priority = x.Requirement.Priority,              // 1 = Highest Priority
                        SpeedScore = averageCompletionDays,             // Lower is better (faster)
                        x.ActiveCasesInClinic,                          // Lower is better
                        LastAssigned = lastAssignedAt                   // Older date is better
                    };
                })
                .OrderBy(x => x.Priority)                // __ Primary filter: Requirement priority __ //
                .ThenBy(x => x.SpeedScore)               // __ Secondary filter: Case completion speed __ //
                .ThenBy(x => x.ActiveCasesInClinic)      // __ Tertiary filter: Number of currently active cases in clinic __ //
                .ThenBy(x => x.LastAssigned)             // __ Quaternary filter: Oldest assignment date __ //
                .ToList();

                // __ 6. Select the best student (first in the ranked list) __ //
                var bestStudent = rankedStudents.First();
                
                _logger.LogInfo($"Auto-Assignment selected Student {bestStudent.Id} for Diagnosis {diagnosis.Id} in Clinic {clinic.Id}.");
                
                return Result.Success(bestStudent.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in AutoAssignmentEngine for Diagnosis {diagnosis.Id}", ex);
                return Result.Failure<int>("An error occurred while trying to automatically select the appropriate student.");
            }
        }
    }
}

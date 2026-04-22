using MediatR;
using ZiggyCreatures.Caching.Fusion;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Cache;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.DTOs.Student;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Diagnosis.Queries.GetAvailableStudents
{
    /// <summary>
    /// Handles the retrieval of available students for case assignment.
    /// 
    /// This query applies strict filtering based on:
    /// 1. Academic Year Constraints: Student's year must fall within clinic's min/max range
    /// 2. Unsatisfied Requirements: Student must have an unsatisfied TermRequirement for this clinic
    /// 3. Workload Limits: Student cannot have more active cases than clinic's max per student
    /// 4. Priority Ranking: Prioritizes students with highest need (fewer completed cases)
    /// 
    /// Workflow:
    /// 1. Load clinic to get academic year and workload constraints
    /// 2. Load all TermRequirements for the clinic/term
    /// 3. Filter students by academic year range
    /// 4. Filter students by unsatisfied requirements
    /// 5. Filter students by workload availability
    /// 6. Order by priority (AI agent or fallback)
    /// 7. Return ranked list to caller
    /// </summary>
    public class GetAvailableStudentsHandler : IRequestHandler<GetAvailableStudentsQuery, Result<List<StudentPriorityDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IFusionCache _cache;
        private readonly IAiAgentService _aiAgent;
        private readonly IAppLogger<GetAvailableStudentsHandler> _logger;

        public GetAvailableStudentsHandler
        (
            IUnitOfWork uow,
            IFusionCache cache,
            IAiAgentService aiAgent,
            IAppLogger<GetAvailableStudentsHandler> logger
        )
        {
            _uow = uow;
            _cache = cache;
            _aiAgent = aiAgent;
            _logger = logger;
        }

        /// <summary>
        /// Main handler method for retrieving available students with strict validation.
        /// </summary>
        public async Task<Result<List<StudentPriorityDto>>> Handle(GetAvailableStudentsQuery query, CancellationToken cancellationToken)
        {
            var clinicId = query.ClinicId;
            var termId = query.TermId;

            // _____________ STEP 1: Validate Clinic Exists & Load Constraints _____________ //
            /// <summary>
            /// Load the clinic to access academic year constraints (MinAcademicYear, MaxAcademicYear)
            /// and workload limit (MaxCasesPerStudent).
            /// If clinic doesn't exist, return failure immediately.
            /// </summary>
            var clinic = await _uow.Repository<Clinic>().GetByIdAsync(clinicId);

            if (clinic is null)
            {
                _logger.LogWarning($"Clinic {clinicId} not found");
                
                return Result.Failure<List<StudentPriorityDto>>("عيادة غير موجودة");
            }

            if (!clinic.IsActive)
            {
                _logger.LogWarning($"Clinic {clinicId} is inactive");
                
                return Result.Failure<List<StudentPriorityDto>>("العيادة غير نشطة");
            }

            // _____________ STEP 2: Check Cache Before Database Query _____________ //
            /// <summary>
            /// Use cache to reduce database calls.
            /// Cache is keyed by clinic and includes all eligible students.
            /// Cache duration is SHORT to ensure relatively fresh data.
            /// </summary>
            var cacheKey = CacheKeys.AvailableStudents(clinicId);

            var result = await _cache.GetOrSetAsync
            (
                cacheKey,
                async _ => 
                {
                    // _____________ STEP 3: Load All Requirements & Students _____________ //
                    /// <summary>
                    /// Fetch all TermRequirements for this clinic and term.
                    /// Include student data via navigation property for quick access.
                    /// This includes active students with unsatisfied requirements.
                    /// </summary>
                    var requirements = await _uow.Repository<TermRequirement>().GetListAsync
                        (
                            r => r.ClinicId == clinicId && r.TermId == termId,
                            true,  // AsNoTracking for read-only operation
                            r => r.Student
                        );

                    // Return empty list if no requirements defined for this clinic/term
                    if (!requirements.Any())
                    {
                        _logger.LogInfo($"No requirements found for clinic {clinicId} term {termId}");
                        
                        return Result.Success(new List<StudentPriorityDto>());
                    }

                    // _____________ STEP 4: Apply Academic Year Validation _____________ //
                    /// <summary>
                    /// Filter students based on their academic year.
                    /// Only students within [MinAcademicYear, MaxAcademicYear] range are eligible.
                    /// 
                    /// Example:
                    /// - Clinic.MinAcademicYear = 2
                    /// - Clinic.MaxAcademicYear = 4
                    /// - Only students in years 2, 3, 4 can be assigned
                    /// - 1st year students are filtered out
                    /// </summary>
                    var academicYearFiltered = requirements.Where
                    (r => 
                       {
                          var student = r.Student;
                          var isInRange = student.AcademicYear >= clinic.MinAcademicYear &&  student.AcademicYear <= clinic.MaxAcademicYear;

                          if (!isInRange)
                          {
                              _logger.LogInfo($"Student {student.Id} academic year {student.AcademicYear} outside clinic range [{clinic.MinAcademicYear}-{clinic.MaxAcademicYear}]");
                          }

                          return isInRange;
                       }
                     ).ToList();

                    if (academicYearFiltered.Count == 0)
                    {
                        _logger.LogWarning($"No students in academic year range [{clinic.MinAcademicYear}-{clinic.MaxAcademicYear}] for clinic {clinicId}");
                        
                        return Result.Success(new List<StudentPriorityDto>());
                    }

                    // _____________ STEP 5: Filter by Unsatisfied Requirements _____________ //
                    /// <summary>
                    /// Only include students with unsatisfied requirements.
                    /// 
                    /// Reason: If a student has already satisfied the clinic requirement,
                    /// they should not be assigned more cases (unless they volunteer for extra).
                    /// 
                    /// IsSatisfied = (CompletedCount + TransferredCount) >= RequiredCount
                    /// If IsSatisfied = true: Skip this student
                    /// If IsSatisfied = false: Include this student
                    /// </summary>
                    var requirementFiltered = academicYearFiltered
                        .Where(r => !r.IsSatisfied)
                        .ToList();

                    if (requirementFiltered.Count == 0)
                    {
                        _logger.LogInfo($"All students have satisfied requirements for clinic {clinicId}");
                       
                        return Result.Success(new List<StudentPriorityDto>());
                    }

                    // _____________ STEP 6: Apply Workload Check _____________ //
                    /// <summary>
                    /// Filter students based on their current active case count in this clinic.
                    /// A student cannot have more than Clinic.MaxCasesPerStudent active (InProgress) cases.
                    /// 
                    /// Process:
                    /// 1. For each student, count their active CaseAssignments in this clinic
                    /// 2. If count < MaxCasesPerStudent, they can accept more cases
                    /// 3. If count >= MaxCasesPerStudent, they are overloaded and cannot accept more
                    /// 
                    /// Reason: Prevents student overload and ensures case quality.
                    /// </summary>
                    var workloadFilteredRequirements = new List<TermRequirement>();
                    
                    foreach (var req in requirementFiltered)
                    {
                        // Load active cases for this student in this clinic
                        var activeCasesCount = await _uow.Repository<CaseAssignment>().CountAsync
                        (
                            ca => 
                                ca.StudentId == req.StudentId && 
                                ca.ClinicId == clinicId && 
                                ca.Status == CaseStatus.InProgress
                        );

                        // Only include if student has capacity for more cases
                        if (activeCasesCount < clinic.MaxCasesPerStudent)
                        {
                            workloadFilteredRequirements.Add(req);
                        }
                        else
                        {
                            _logger.LogInfo($"Student {req.StudentId} at capacity: {activeCasesCount}/{clinic.MaxCasesPerStudent} in clinic {clinicId}");
                        }
                    }

                    if (workloadFilteredRequirements.Count == 0)
                    {
                        _logger.LogWarning($"All eligible students are at workload capacity in clinic {clinicId}");
                       
                        return Result.Success(new List<StudentPriorityDto>());
                    }

                    // _____________ STEP 7: Get AI-Prioritized Student Order _____________ //
                    /// <summary>
                    /// Call the AI agent to determine optimal student priority based on:
                    /// - Current case count and type
                    /// - Historical performance and completion rate
                    /// - Specialty alignment
                    /// - Other factors determined by the AI algorithm
                    /// 
                    /// Fallback: If AI fails, sort by requirement Priority field:
                    /// - Priority 1 (0 cases): Highest need
                    /// - Priority 2 (1 case): Medium-high need
                    /// - Priority 3 (2 cases): Medium need
                    /// - Priority 4 (3+ cases): Lower need
                    /// </summary>
                    IEnumerable<int> prioritizedIds;

                    try
                    {
                        // Call AI agent for intelligent prioritization
                        prioritizedIds = await _aiAgent.GetStudentPriorityListAsync(clinicId, termId);
                        
                        _logger.LogInfo($"AI prioritization returned {prioritizedIds.Count()} students for clinic {clinicId}");
                    }
                    catch (Exception ex)
                    {
                        // Fallback to simple priority sort if AI fails
                        _logger.LogWarning($"AI agent failed for clinic {clinicId}, using fallback prioritization", ex);

                        prioritizedIds = workloadFilteredRequirements
                            .OrderBy(r => r.Priority)  // Lower priority number = higher need
                            .ThenBy(r => r.CompletedCount)  // If same priority, fewer completed = higher priority
                            .Select(r => r.StudentId);
                    }

                    // _____________ STEP 8: Build Response DTOs _____________ //
                    /// <summary>
                    /// Create StudentPriorityDto objects in the order determined by AI/fallback.
                    /// Each DTO includes:
                    /// - Student identification (Id, Name, Code)
                    /// - Progress info (CompletedCases, RequiredCases)
                    /// - Priority rank (1 = first choice)
                    /// - Completion status (IsComplete = already satisfied or not)
                    /// </summary>
                    var dict = workloadFilteredRequirements.ToDictionary(r => r.StudentId);

                    var dtos = prioritizedIds
                        .Where(dict.ContainsKey)  // Only include students from filtered list
                        .Select((studentId, index) =>
                        {
                            var req = dict[studentId];
                            return new StudentPriorityDto
                            {
                                StudentId = studentId,
                                FullName = req.Student.FullName,
                                StudentCode = req.Student.StudentCode,
                                AcademicYear = req.Student.AcademicYear,
                                CompletedCases = req.CompletedCount,
                                RequiredCases = req.RequiredCount,
                                Priority = index + 1,  // Rank starts at 1 (first choice)
                                IsComplete = req.IsSatisfied,
                                RequirementPriority = req.Priority  // Internal priority for sorting
                            };
                        })
                        .ToList();

                    _logger.LogInfo($"Returned {dtos.Count()} available students for clinic {clinicId}");
                    return Result.Success(dtos);
                },
                CacheDuration.Short,
                cancellationToken
            );

            return result!;
        }
    }
}

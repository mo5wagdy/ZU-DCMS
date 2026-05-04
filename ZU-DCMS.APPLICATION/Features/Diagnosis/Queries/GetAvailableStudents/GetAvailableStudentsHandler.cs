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
                    // __ Handles the retrieval of available students for case assignment. __ //
    public class GetAvailableStudentsHandler : IRequestHandler<GetAvailableStudentsQuery, Result<List<StudentPriorityDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IFusionCache _cache;
        //private readonly IAiAgentService _aiAgent;
        private readonly IAppLogger<GetAvailableStudentsHandler> _logger;

        public GetAvailableStudentsHandler
        (
            IUnitOfWork uow,
            IFusionCache cache,
            //IAiAgentService aiAgent,
            IAppLogger<GetAvailableStudentsHandler> logger
        )
        {
            _uow = uow;
            _cache = cache;
           // _aiAgent = aiAgent;
            _logger = logger;
        }

                    // __ Main handler method for retrieving available students with strict validation. __ //
        public async Task<Result<List<StudentPriorityDto>>> Handle(GetAvailableStudentsQuery query, CancellationToken cancellationToken)
        {
            var clinicId = query.ClinicId;
            var termId = query.TermId;

            if (!termId.HasValue)
            {
                var activeTerm = await _uow.Repository<Term>().GetFirstOrDefaultAsync(t => t.IsActive);
                if (activeTerm == null)
                {
                    return Result.Failure<List<StudentPriorityDto>>("لا يوجد فصل دراسي نشط حالياً");
                }
                termId = activeTerm.Id;
            }

            // _____________ STEP 1: Validate Clinic Exists & Load Constraints _____________ //
                    // __ Load the clinic to access academic year constraints (MinAcademicYear, MaxAcademicYear) __ //
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
                    // __ Use cache to reduce database calls. __ //
            var cacheKey = CacheKeys.AvailableStudents(clinicId);

            var result = await _cache.GetOrSetAsync
            (
                cacheKey,
                async _ => 
                {
                    // _____________ STEP 3: Load All Requirements & Students _____________ //
                    // __ Fetch all TermRequirements for this clinic and term. __ //
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
                    // __ Filter students based on their academic year. __ //
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
                    // __ Only include students with unsatisfied requirements. __ //
                    var requirementFiltered = academicYearFiltered
                        .Where(r => !r.IsSatisfied)
                        .ToList();

                    if (requirementFiltered.Count == 0)
                    {
                        _logger.LogInfo($"All students have satisfied requirements for clinic {clinicId}");
                       
                        return Result.Success(new List<StudentPriorityDto>());
                    }

                    // _____________ STEP 6: Apply Workload Check _____________ //
                    // __ Filter students based on their current active case count in this clinic. __ //
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
                    // __ Call the AI agent to determine optimal student priority based on: __ //
                    // IEnumerable<int> prioritizedIds;

                    //try
                    //{
                    //    // Call AI agent for intelligent prioritization
                    //    prioritizedIds = await _aiAgent.GetStudentPriorityListAsync(clinicId, termId);
                        
                    //    _logger.LogInfo($"AI prioritization returned {prioritizedIds.Count()} students for clinic {clinicId}");
                    //}
                    //catch (Exception ex)
                    //{
                    //    // Fallback to simple priority sort if AI fails
                    //    _logger.LogWarning($"AI agent failed for clinic {clinicId}, using fallback prioritization", ex);

                    //    prioritizedIds = workloadFilteredRequirements
                    //        .OrderBy(r => r.Priority)  // Lower priority number = higher need
                    //        .ThenBy(r => r.CompletedCount)  // If same priority, fewer completed = higher priority
                    //        .Select(r => r.StudentId);
                    //}


                    // _____________ STEP 7: Calculate Performance Metrics & Prioritize _____________ //
                    var studentIds = workloadFilteredRequirements.Select(r => r.StudentId).ToList();

                    // Fetch all approved cases for these students in this term
                    var completedCases = await _uow.Repository<CaseAssignment>().GetListAsync(c =>
                        studentIds.Contains(c.StudentId) &&
                        c.TermId == termId &&
                        c.Status == CaseStatus.Approved &&
                        c.CompletedAt != null
                    );

                    // Calculate metrics per student
                    var studentMetrics = completedCases
                        .GroupBy(c => c.StudentId)
                        .ToDictionary(g => g.Key, g => 
                        {
                            double avgDays = g.Average(c => (c.CompletedAt!.Value - c.AssignedAt).TotalDays);
                            string label = avgDays <= 7 ? "ممتاز" :
                                           avgDays <= 14 ? "جيد" :
                                           avgDays <= 21 ? "مقبول" : "بطيء";
                            
                            double score = avgDays <= 7 ? -0.3 :
                                           avgDays <= 14 ? -0.1 :
                                           avgDays <= 21 ? 0.1 : 0.3;
                                           
                            return (AverageDays: avgDays, Label: label, Score: score);
                        });

                    // Sort students using combined metrics
                    var prioritizedStudents = workloadFilteredRequirements
                        .OrderBy(r => 
                        {
                            double completionRatio = r.RequiredCount > 0 ? (double)r.CompletedCount / r.RequiredCount : 0;
                            double performanceScore = studentMetrics.TryGetValue(r.StudentId, out var m) ? m.Score : 0;
                            return completionRatio + performanceScore;
                        })
                        .ThenBy(r => r.Priority) // fallback to requirement priority if scores tie
                        .ToList();





                    // _____________ STEP 8: Build Response DTOs _____________ //
                    // __ Create StudentPriorityDto objects in the order determined by AI/fallback. __ //
                    var dtos = prioritizedStudents
                        .Select((req, index) =>
                        {
                            var hasMetrics = studentMetrics.TryGetValue(req.StudentId, out var metrics);
                            return new StudentPriorityDto
                            {
                                StudentId = req.StudentId,
                                FullName = req.Student.FullName,
                                StudentCode = req.Student.StudentCode,
                                AcademicYear = req.Student.AcademicYear,
                                CompletedCases = req.CompletedCount,
                                RequiredCases = req.RequiredCount,
                                Priority = index + 1,
                                IsComplete = req.IsSatisfied,
                                RequirementPriority = req.Priority,
                                AverageCompletionDays = hasMetrics ? metrics.AverageDays : null,
                                PerformanceLabel = hasMetrics ? metrics.Label : "لا يوجد",
                                IsAvailable = true,
                                AvailabilityStatus = "متاح"
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

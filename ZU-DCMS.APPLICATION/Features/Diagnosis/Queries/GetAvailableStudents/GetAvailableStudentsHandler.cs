using MediatR;
using ZiggyCreatures.Caching.Fusion;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Cache;
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
        private readonly IAppLogger<GetAvailableStudentsHandler> _logger;

        public GetAvailableStudentsHandler
        (
            IUnitOfWork uow,
            IFusionCache cache,
            IAppLogger<GetAvailableStudentsHandler> logger
        )
        {
            _uow = uow;
            _cache = cache;
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
                if (activeTerm is null)
                {
                    return Result.Failure<List<StudentPriorityDto>>("لا يوجد فصل دراسي نشط حالياً");
                }
                termId = activeTerm.Id;
            }

            var clinic = await _uow.Repository<Clinic>().GetByIdAsync(clinicId);
            if (clinic is null || !clinic.IsActive)
            {
                return Result.Failure<List<StudentPriorityDto>>("العيادة غير موجودة أو غير نشطة");
            }

            // _____________ STEP 2: Check Cache for Full List _____________ //
            var cacheKey = CacheKeys.AvailableStudents(clinicId);

            var fullList = await _cache.GetOrSetAsync
            (
                cacheKey,
                async _ => 
                {
                    // _____________ STEP 3: Load & Filter Requirements _____________ //
                    var requirements = await _uow.Repository<TermRequirement>().GetListAsync
                        (
                            r => r.ClinicId == clinicId && r.TermId == termId && r.StudentId != null,
                            true,
                            r => r.Student
                        );

                    var eligible = requirements.Where
                        (r => 
                            r.Student.AcademicYear >= clinic.MinAcademicYear && 
                            r.Student.AcademicYear <= clinic.MaxAcademicYear &&
                           !r.IsSatisfied
                        ).ToList();

                    if (eligible.Count == 0) return new List<StudentPriorityDto>();

                    // _____________ STEP 4: Capacity Check _____________ //
                    var studentIds = eligible.Select(r => r.StudentId!.Value).ToList();

                    var activeCaseList = await _uow.Repository<CaseAssignment>().GetListAsync
                    (
                        ca => studentIds.Contains(ca.StudentId) &&
                        ca.ClinicId == clinicId &&
                        ca.Status == CaseStatus.InProgress
                    );
                    var activeCounts = activeCaseList.GroupBy
                    (
                        ca => ca.StudentId
                    )
                    .Select
                    (
                        g => new
                        {
                            StudentId = g.Key,
                            Count = g.Count()
                        }
                    )
                    .ToDictionary
                    (
                        x => x.StudentId,
                        x => x.Count
                    );

                    // _____________ STEP 5: Performance & Prioritization _____________ //
                    var completedCases = await _uow.Repository<CaseAssignment>().GetListAsync
                    (c =>
                        studentIds.Contains(c.StudentId) &&
                        c.TermId == termId &&
                        c.Status == CaseStatus.Approved &&
                        c.CompletedAt != null
                    );

                    var metrics = completedCases
                        .GroupBy(c => c.StudentId)
                        .ToDictionary(g => g.Key, g => 
                        {
                            double avg = g.Average(c => (c.CompletedAt!.Value - c.AssignedAt).TotalDays);
                            string label = avg <= 7 ? "ممتاز" : avg <= 14 ? "جيد" : avg <= 21 ? "مقبول" : "بطيء";
                            double score = avg <= 7 ? -0.3 : avg <= 14 ? -0.1 : avg <= 21 ? 0.1 : 0.3;
                            return (Avg: avg, Label: label, Score: score);
                        });

                    return eligible.OrderBy
                    (r => 
                        {
                          double ratio = r.RequiredCount > 0 ? (double)r.CompletedCount / r.RequiredCount : 0;
                          double pScore = metrics.TryGetValue(r.StudentId!.Value, out var m) ? m.Score : 0;
                          return ratio + pScore;
                        }
                    )
                    .ThenBy(r => r.Priority)
                    .Select((req, idx) => 
                    {
                        metrics.TryGetValue(req.StudentId!.Value, out var m);
                        return new StudentPriorityDto 
                        {
                            StudentId = req.StudentId!.Value,
                            FullName = req.Student!.FullName,
                            StudentCode = req.Student.StudentCode,
                            AcademicYear = req.Student.AcademicYear,
                            CompletedCases = req.CompletedCount,
                            RequiredCases = req.RequiredCount,
                            Priority = idx + 1,
                            IsComplete = req.IsSatisfied,
                            RequirementPriority = req.Priority,
                            AverageCompletionDays = m.Avg > 0 ? m.Avg : null,
                            PerformanceLabel = m.Label ?? "لا يوجد",
                            IsAvailable = true,
                            AvailabilityStatus = "متاح"
                        };
                    })
                    .ToList();
                },
                CacheDuration.Short,
                cancellationToken
            );

            // _____________ STEP 6: Apply Search Outside Cache _____________ //
            if (fullList == null) return Result.Success(new List<StudentPriorityDto>());

            var filtered = fullList;
            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var st = query.SearchTerm.Trim().ToLower();
                filtered = fullList.Where(s => 
                    s.FullName.ToLower().Contains(st) || 
                    s.StudentCode.ToLower().Contains(st)
                ).ToList();
            }

            return Result.Success(filtered);
        }
    }
}

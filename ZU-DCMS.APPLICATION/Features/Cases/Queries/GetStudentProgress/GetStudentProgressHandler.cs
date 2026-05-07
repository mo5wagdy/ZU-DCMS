using AutoMapper;
using MediatR;
using ZiggyCreatures.Caching.Fusion;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Cache;
using ZU_DCMS.APPLICATION.Contracts.Cache;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.DTOs.Student;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Cases.Queries.GetStudentProgress
{
    public class GetStudentProgressHandler : IRequestHandler<GetStudentProgressQuery, Result<StudentProgressDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IFusionCache _cache;
        private readonly IMapper _mapper;
        private readonly IAppLogger<GetStudentProgressHandler> _logger;

        public GetStudentProgressHandler
        (
            IUnitOfWork uow,
            IFusionCache cache,
            IMapper mapper,
            IAppLogger<GetStudentProgressHandler> logger
        )
        {
            _uow = uow;
            _cache = cache;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<StudentProgressDto>> Handle(GetStudentProgressQuery query, CancellationToken cancellationToken)
        {
            var studentId = query.StudentId;
            var termId = query.TermId;

            _logger.LogInfo("Calculating progress for student ID: {StudentId} in term ID: {TermId}", studentId, termId);

            var cacheKey = CacheKeys.StudentProgress(studentId, termId);

            var result = await _cache.GetOrSetAsync
            (
                cacheKey,
                async _ =>
                {
                    // __ Fetch individual requirements __ //
                    var requirements = await _uow.Repository<TermRequirement>().GetListAsync
                        (
                            r => r.StudentId == studentId && r.TermId == termId,
                            true,
                            r => r.Clinic
                        );

                    bool isGlobal = false;
                    // __ Fallback to Global if no individual ones __ //
                    if (requirements == null || requirements.Count == 0)
                    {
                        var stInfo = await _uow.Repository<Student>().GetByIdAsync(studentId);
                        if (stInfo != null)
                        {
                            requirements = await _uow.Repository<TermRequirement>().GetListAsync
                                (
                                    r =>  r.TermId == termId && r.AcademicYear == stInfo.AcademicYear,
                                    true,
                                    r => r.Clinic
                                );
                            isGlobal = true;
                        }
                    }

                    if (requirements == null || requirements.Count == 0)
                    {
                        _logger.LogWarning("No requirements found for student ID: {StudentId} in term ID: {TermId}", studentId, termId);
                        return Result.Failure<StudentProgressDto>("None متطلبات محددة لهذا الطالب");
                    }

                    // __ Calculate progress dynamically for global templates __ //
                    if (isGlobal)
                    {
                        foreach (var req in requirements)
                        {
                            req.CompletedCount = await _uow.Repository<CaseAssignment>().CountAsync(ca => ca.StudentId == studentId && ca.TermId == termId && ca.ClinicId == req.ClinicId && ca.Status == CaseStatus.Approved);
                        }
                    }

                    var student = await _uow.Repository<Student>().GetByIdAsync(studentId);
                    if (student is null) return Result.Failure<StudentProgressDto>("Student not found");

                    var res = new StudentProgressDto
                    {
                        StudentId = studentId,
                        FullName = student.FullName,
                        StudentCode = student.StudentCode,
                        TotalRequired = requirements.Sum(x => x.RequiredCount),
                        TotalCompleted = requirements.Sum(x => x.CompletedCount),
                        TotalTransferred = requirements.Sum(x => x.TransferredCount),
                        IsTermComplete = requirements.All(x => x.IsSatisfied),
                        Requirements = _mapper.Map<List<StudentRequirementDto>>(requirements)
                    };

                    return Result.Success(res);
                },
                CacheDuration.Short,
                cancellationToken
            );

            return result!;
        }
    }
}

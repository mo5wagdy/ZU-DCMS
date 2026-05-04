using AutoMapper;
using MediatR;
using ZiggyCreatures.Caching.Fusion;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Cache;
using ZU_DCMS.APPLICATION.DTOs.Student;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Admin.Queries.GetStudentRequirements
{
    public class GetStudentRequirementsHandler : IRequestHandler<GetStudentRequirementsQuery, Result<List<StudentRequirementDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IFusionCache _cache;
        private readonly IMapper _mapper;

        public GetStudentRequirementsHandler
        (
            IUnitOfWork uow,
            IFusionCache cache,
            IMapper mapper
        )
        {
            _uow = uow;
            _cache = cache;
            _mapper = mapper;
        }

        public async Task<Result<List<StudentRequirementDto>>> Handle(GetStudentRequirementsQuery query, CancellationToken cancellationToken)
        {
            var cacheKey = CacheKeys.StudentRequirements(query.StudentId, query.TermId);

            var result = await _cache.GetOrSetAsync
            (
                cacheKey,
                async _ => 
                {
                    // __ Fetch individual requirements __ //
                    var requirements = await _uow.Repository<TermRequirement>().GetListAsync
                        (r =>
                            r.StudentId == query.StudentId &&
                            r.TermId == query.TermId,
                            false,
                            r => r.Clinic
                        );

                    bool isGlobal = false;
                    if (requirements == null || requirements.Count == 0)
                    {
                        var student = await _uow.Repository<Student>().GetByIdAsync(query.StudentId);
                        if (student != null)
                        {
                            requirements = await _uow.Repository<TermRequirement>().GetListAsync
                                (r =>
                                    r.StudentId == null &&
                                    r.TermId == query.TermId &&
                                    r.AcademicYear == student.AcademicYear,
                                    false,
                                    r => r.Clinic
                                );
                            isGlobal = true;
                        }
                    }

                    if (isGlobal && requirements != null)
                    {
                        foreach (var req in requirements)
                        {
                            req.CompletedCount = await _uow.Repository<CaseAssignment>().CountAsync(ca => ca.StudentId == query.StudentId && ca.TermId == query.TermId && ca.ClinicId == req.ClinicId && ca.Status == CaseStatus.Approved);
                        }
                    }

                    return Result.Success(_mapper.Map<List<StudentRequirementDto>>(requirements ?? new List<TermRequirement>()));
                },
                CacheDuration.Medium,
                cancellationToken
            );

            return result!;
        }
    }
}
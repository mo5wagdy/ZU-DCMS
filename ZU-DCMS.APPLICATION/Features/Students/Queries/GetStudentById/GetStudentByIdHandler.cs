using AutoMapper;
using MediatR;
using ZiggyCreatures.Caching.Fusion;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Cache;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.DTOs.Student;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Students.Queries.GetStudentById
{
    public class GetStudentByIdHandler : IRequestHandler<GetStudentByIdQuery, Result<StudentDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IFusionCache _cache;
        private readonly IMapper _mapper;
        private readonly IAppLogger<GetStudentByIdHandler> _logger;

        public GetStudentByIdHandler
        (
            IUnitOfWork uow,
            IFusionCache cache,
            IMapper mapper,
            IAppLogger<GetStudentByIdHandler> logger
        )
        {
            _uow = uow;
            _cache = cache;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<StudentDto>> Handle(GetStudentByIdQuery query, CancellationToken cancellationToken)
        {
            var studentId = query.StudentId;

            _logger.LogInfo("Fetching student with ID {StudentId}.", studentId);

            // __ Try Fetching From Cache First __ //
            var cacheKey = CacheKeys.StudentById(studentId);

            var result = await _cache.GetOrSetAsync
            (
                cacheKey,
                async _ => // => If Not Found In Cache Fetch From DB
                {
                    // __ Fetch student with related data __ //
                    var student = await _uow.Repository<Student>().GetFirstOrDefaultAsync
                        (
                            st => st.Id == studentId,
                            true,
                            s => s.TermRequirements.Where(tr => tr.Term.IsActive),
                            s => s.CaseAssignments.Where(ca => ca.Clinic.IsActive)
                        );

                    // __ Handle not found __ //
                    if (student is null)
                    {
                        _logger.LogWarning("Student with ID {StudentId} not found.", studentId);

                        return Result.Failure<StudentDto>("Student not found.");
                    }

                    _logger.LogInfo("Student with ID {StudentId} found. Mapping to DTO.", studentId);

                    // __ Map to DTO and return __ //
                    return Result.Success(_mapper.Map<StudentDto>(student));
                },
                CacheDuration.Short,
                cancellationToken
            );

            // __ Cache result __ //
            return result!;
        }
    }
}

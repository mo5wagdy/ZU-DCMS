using AutoMapper;
using MediatR;
using System.Linq.Expressions;
using ZiggyCreatures.Caching.Fusion;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Cache;
using ZU_DCMS.APPLICATION.Common.Pagination;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.DTOs.Student;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Students.Queries.GetAllStudents
{
    public class GetAllStudentsHandler : IRequestHandler<GetAllStudentsQuery, Result<PagedResult<StudentDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IFusionCache _cache;
        private readonly IMapper _mapper;
        private readonly IAppLogger<GetAllStudentsHandler> _logger;

        public GetAllStudentsHandler
        (
            IUnitOfWork uow,
            IFusionCache cache,
            IMapper mapper,
            IAppLogger<GetAllStudentsHandler> logger
        )
        {
            _uow = uow;
            _cache = cache;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<PagedResult<StudentDto>>> Handle(GetAllStudentsQuery query, CancellationToken cancellationToken)
        {
            var request = query.Request;

            _logger.LogInfo("Fetching students with pagination. Page: {Page}, PageSize: {PageSize}", request.Page, request.PageSize);

            // __ Cache version __ //
            var version = await _cache.GetOrSetAsync("students:version", _ => Task.FromResult(1));

            // __ Fetching From Cache If Available __ //
            var cacheKey = CacheKeys.StudentsPage(request.Page, request.PageSize, request.SearchTerm, request.SortBy, version);

            var result = await _cache.GetOrSetAsync
            (
                cacheKey,
                async _ => // => If Not Found In Cache Fetch From DB
                {
                    // __ Expression for filtering __ //
                    Expression<Func<Student, bool>>? filter = null;

                    if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                    {
                        var term = request.SearchTerm.Trim().ToLower();

                        filter = s => s.FullName.ToLower().Contains(term) || s.StudentCode.ToLower().Contains(term);
                    }

                    // __ Func for ordering according to user choice __ // 
                    Func<IQueryable<Student>, IOrderedQueryable<Student>> orderBy = q =>
                    {
                        var sortBy = request.SortBy?.ToLower();

                        // __ If Descending __ //
                        if (request.SortDescending)
                        {
                            return sortBy switch
                            {
                                "fullname" => q.OrderByDescending(s => s.FullName),
                                "studentcode" => q.OrderByDescending(s => s.StudentCode),
                                "academicyear" => q.OrderByDescending(s => s.AcademicYear),
                                _ => q.OrderByDescending(s => s.Id)
                            };
                        }
                        else // __ If Ascending __ //
                        {
                            return sortBy switch
                            {
                                "fullname" => q.OrderBy(s => s.FullName),
                                "studentcode" => q.OrderBy(s => s.StudentCode),
                                "academicyear" => q.OrderBy(s => s.AcademicYear),
                                _ => q.OrderBy(s => s.Id)
                            };
                        }
                        ;
                    };

                    // __ Fetching according to the paginated request using filtering & ordering settings __ //
                    var (item, totalCount) = await _uow.Repository<Student>().GetPagedListAsync
                        (
                            skip: (request.Page - 1) * request.PageSize,
                            take: (request.PageSize),
                            predicate: filter,
                            orderBy: orderBy,
                            disabledTracking: true
                        );

                    // __ Mapping to DTO __ //
                    var studentDtos = _mapper.Map<List<StudentDto>>(item);

                    // __ Creating the paged result __ //
                    var pagedResult = PagedResult<StudentDto>.Create(studentDtos, totalCount, request);

                    _logger.LogInfo("Fetched {Count} students out of {TotalCount} total students.", studentDtos.Count, totalCount);

                    return Result.Success(pagedResult);
                },
                CacheDuration.Medium,
                cancellationToken
            );

            // __ Cache result __ //
            return result!;
        }
    }
}

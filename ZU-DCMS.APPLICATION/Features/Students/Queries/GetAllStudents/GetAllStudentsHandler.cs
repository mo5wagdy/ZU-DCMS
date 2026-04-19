using AutoMapper;
using System.Linq.Expressions;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Student;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Student.Queries.GetAllStudents
{
    public class GetAllStudentsHandler
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IAppLogger<GetAllStudentsHandler> _logger;

        public GetAllStudentsHandler(IUnitOfWork uow, IMapper mapper, IAppLogger<GetAllStudentsHandler> logger)
        {
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<PagedResult<StudentDto>>> Handle(GetAllStudentsQuery query)
        {
            var request = query.Request;

            _logger.LogInfo("Fetching students with pagination. Page: {Page}, PageSize: {PageSize}", request.Page, request.PageSize);

            Expression<Func<Domain.Entities.Student, bool>>? filter = null;

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                filter = s => s.FullName.ToLower().Contains(term) || s.StudentCode.ToLower().Contains(term);
            }

            Func<IQueryable<Domain.Entities.Student>, IOrderedQueryable<Domain.Entities.Student>> orderBy = q =>
            {
                var sortBy = request.SortBy?.ToLower();

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
                else
                {
                    return sortBy switch
                    {
                        "fullname" => q.OrderBy(s => s.FullName),
                        "studentcode" => q.OrderBy(s => s.StudentCode),
                        "academicyear" => q.OrderBy(s => s.AcademicYear),
                        _ => q.OrderBy(s => s.Id)
                    };
                };
            };

            var (item, totalCount) = await _uow.Repository<Domain.Entities.Student>().GetPagedListAsync
                (
                    skip: (request.Page - 1) * request.PageSize,
                    take: (request.PageSize),
                    predicate: filter,
                    orderBy : orderBy,
                    disabledTracking: true
                );

            var studentDtos = _mapper.Map<List<StudentDto>>(item);
            var pagedResult = PagedResult<StudentDto>.Create(studentDtos, totalCount, request);

            _logger.LogInfo("Fetched {Count} students out of {TotalCount} total students.", studentDtos.Count, totalCount);

            return Result.Success(pagedResult);
        }
    }
}

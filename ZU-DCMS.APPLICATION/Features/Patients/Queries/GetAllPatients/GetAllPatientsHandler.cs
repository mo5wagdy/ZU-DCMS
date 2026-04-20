using AutoMapper;
using MediatR;
using System.Linq.Expressions;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Pagination;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.DTOs.Patient;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Patients.Queries.GetAllPatients
{
    public class GetAllPatientsHandler : IRequestHandler<GetAllPatientsQuery, Result<PagedResult<PatientDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IAppLogger<GetAllPatientsHandler> _logger;

        public GetAllPatientsHandler(
            IUnitOfWork uow,
            IMapper mapper,
            IAppLogger<GetAllPatientsHandler> logger)
        {
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
        }

        // ________________ Get All (Intern Doctor) ________________ //
        public async Task<Result<PagedResult<PatientDto>>> Handle(GetAllPatientsQuery query, CancellationToken cancellationToken)
        {
            var request = query.Request;

            _logger.LogInfo("Fetching patients with pagination. Page: {Page}, PageSize: {PageSize}, SortBy: {SortBy}, SortDescending: {SortDescending}, SearchTerm: {SearchTerm}", request.Page, request.PageSize, request.SortBy!, request.SortDescending, request.SearchTerm!);

            // ________________ Search ________________ //
            Expression<Func<Patient, bool>>? filter = null;

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();

                filter = p =>
                         p.FullName.Contains(term) ||
                         p.PatientCode.Contains(term) ||
                         p.PhoneNumber.Contains(term);
            }

            // ________________ Sorting ________________ //
            Func<IQueryable<Patient>, IOrderedQueryable<Patient>> orderBy = q =>
            {
                var sortBy = request.SortBy?.ToLower();
                if (request.SortDescending)
                {
                    return sortBy switch
                    {
                        "name" => q.OrderByDescending(p => p.FullName),
                        "code" => q.OrderByDescending(p => p.PatientCode),
                        _ => q.OrderByDescending(p => p.CreatedAt)
                    };
                }
                return sortBy switch
                {
                    "name" => q.OrderBy(p => p.FullName),
                    "code" => q.OrderBy(p => p.PatientCode),
                    _ => q.OrderBy(p => p.CreatedAt)
                };
            };

            // ________________ Pagination ________________ //
            var (items, totalCount) = await _uow.Repository<Patient>().GetPagedListAsync(
                skip: (request.Page - 1) * request.PageSize,
                take: request.PageSize,
                predicate: filter,
                orderBy: orderBy,
                disabledTracking: true 
            );

            // __ Mapping and return __ //
            var dtos = _mapper.Map<List<PatientDto>>(items);

            var pagedResult = PagedResult<PatientDto>.Create(dtos, totalCount, request);

            _logger.LogInfo("Fetched {Count} patients out of {TotalCount}. Page: {Page}/{TotalPages}", dtos.Count, totalCount, pagedResult.Page, pagedResult.TotalPages);
            
            return Result.Success<PagedResult<PatientDto>>(pagedResult);
        }
    }
}

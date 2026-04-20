using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Admin;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Admin.Queries.GetAllTerms
{
    public class GetAllTermsHandler : IRequestHandler<GetAllTermsQuery, Result<List<TermDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetAllTermsHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Result<List<TermDto>>> Handle(GetAllTermsQuery query, CancellationToken cancellationToken)
        {
            // __ Fetch all terms __ //
            var terms = await _uow.Repository<Term>().GetListAsync();

            // __ Order by StartDate desc __ //
            var ordered = terms.OrderByDescending(t => t.StartDate);

            // __ Map to DTO __ //
            return Result.Success(_mapper.Map<List<TermDto>>(ordered));
        }
    }
}

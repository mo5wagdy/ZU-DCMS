using AutoMapper;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Admin;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Admin.Queries.GetAllTerms
{
    public class GetAllTermsHandler
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetAllTermsHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Result<List<TermDto>>> Handle(GetAllTermsQuery query)
        {
            var terms = await _uow.Repository<Term>().GetListAsync();

            var ordered = terms.OrderByDescending(t => t.StartDate);

            return Result.Success(_mapper.Map<List<TermDto>>(ordered));
        }
    }
}

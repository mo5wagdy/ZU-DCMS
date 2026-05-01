using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Admin;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Lookups.Queries.GetActiveTerm
{
    public class GetActiveTermHandler : IRequestHandler<GetActiveTermQuery, Result<TermDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetActiveTermHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Result<TermDto>> Handle(GetActiveTermQuery query, CancellationToken cancellationToken)
        {
            var activeTerm = await _uow.Repository<Term>().GetFirstOrDefaultAsync(t => t.IsActive);
            
            if (activeTerm == null)
                return Result.Failure<TermDto>("No active term found");

            return Result.Success(_mapper.Map<TermDto>(activeTerm));
        }
    }
}

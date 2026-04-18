using AutoMapper;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Admin;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Admin.Queries.GetTermById
{
    public class GetTermByIdHandler
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetTermByIdHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Result<TermDto>> Handle(GetTermByIdQuery query)
        {
            var term = await _uow.Repository<Term>().GetByIdAsync(query.TermId);

            if (term is null)
                return Result.Failure<TermDto>("الترم غير موجود");

            return Result.Success(_mapper.Map<TermDto>(term));
        }
    }
}

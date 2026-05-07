using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Admin;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Admin.Queries.GetTermById
{
    public class GetTermByIdHandler : IRequestHandler<GetTermByIdQuery, Result<TermDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetTermByIdHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Result<TermDto>> Handle(GetTermByIdQuery query, CancellationToken cancellationToken)
        {
            // __ Fetch term by Id __ //
            var term = await _uow.Repository<Term>().GetByIdAsync(query.TermId);

            // __ If null → return failure __ //
            if (term is null)
                return Result.Failure<TermDto>("Term not found");

            // __ Map to DTO __ //
            return Result.Success(_mapper.Map<TermDto>(term));
        }
    }
}

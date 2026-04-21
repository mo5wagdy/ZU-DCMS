using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Case;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Cases.Queries.GetCaseReviews
{
    public class GetCaseReviewsHandler : IRequestHandler<GetCaseReviewsQuery, Result<List<ReviewCaseDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetCaseReviewsHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Result<List<ReviewCaseDto>>> Handle(GetCaseReviewsQuery request, CancellationToken cancellationToken)
        {
            var reviews = await _uow.Repository<CaseReview>().GetListAsync(r => r.CaseAssignmentId == request.CaseAssignmentId);

            var dtos = _mapper.Map<List<ReviewCaseDto>>(reviews);

            return Result.Success(dtos);
        }
    }
}

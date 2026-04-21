
using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Case;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Cases.Queries.GetCasesForReview
{
    public class GetCasesForReviewHandler : IRequestHandler<GetCasesForReviewQuery, Result<List<CaseAssignmentDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetCasesForReviewHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Result<List<CaseAssignmentDto>>> Handle(GetCasesForReviewQuery query, CancellationToken cancellationToken)
        {
            // Get only pending cases
            var cases = await _uow.Repository<CaseAssignment>().GetListAsync(c => c.Status == CaseStatus.PendingReview);

            var dtos = _mapper.Map<List<CaseAssignmentDto>>(cases);

            return Result.Success(dtos);
        }
    }
}

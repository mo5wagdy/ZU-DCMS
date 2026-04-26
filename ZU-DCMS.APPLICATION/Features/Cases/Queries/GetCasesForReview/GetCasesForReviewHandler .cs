
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
            var activeTerm = await _uow.Repository<Term>().GetFirstOrDefaultAsync(t => t.IsActive);

            if (activeTerm == null)
                return Result.Failure<List<CaseAssignmentDto>>("لا يوجد ترم نشط");

            // __ Get only pending cases __ //
            var cases = await _uow.Repository<CaseAssignment>().GetListAsync
            (
                c => c.Status == CaseStatus.PendingReview && c.TermId == activeTerm.Id,
                false,
                c => c.DiagnosisRecord.Booking.Patient,
                c => c.DiagnosisRecord.DiagnosisType,
                c => c.Clinic,
                c => c.Student,
                c => c.Sessions
            );
            
            var dtos = _mapper.Map<List<CaseAssignmentDto>>(cases);

            return Result.Success(dtos);
        }
    }
}

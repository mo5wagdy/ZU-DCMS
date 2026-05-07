using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Case;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Cases.Queries.GetReviewedCases
{
    // __ Returns all cases reviewed by a specific Teaching Assistant (approved or rejected) __ //
    public class GetReviewedCasesHandler : IRequestHandler<GetReviewedCasesQuery, Result<List<CaseAssignmentDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetReviewedCasesHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Result<List<CaseAssignmentDto>>> Handle(GetReviewedCasesQuery query, CancellationToken cancellationToken)
        {
            // __ Find the TA entity by user ID __ //
            var ta = await _uow.Repository<TeachingAssistant>().GetFirstOrDefaultAsync(
                t => t.ApplicationUserId == query.TeachingAssistantUserId,
                true
            );

            if (ta is null)
                return Result.Failure<List<CaseAssignmentDto>>("Teaching assistant not found");

            // __ Get all reviews done by this TA __ //
            var reviews = await _uow.Repository<CaseReview>().GetListAsync(
                r => r.TeachingAssistantId == ta.Id,
                true
            );

            if (!reviews.Any())
                return Result.Success(new List<CaseAssignmentDto>());

            var reviewedCaseIds = reviews.Select(r => r.CaseAssignmentId).Distinct().ToList();

            // __ Fetch the full case assignment records __ //
            var cases = await _uow.Repository<CaseAssignment>().GetListAsync(
                c => reviewedCaseIds.Contains(c.Id) &&
                     (c.Status == CaseStatus.Approved || c.Status == CaseStatus.Completed || c.Status == CaseStatus.InProgress),
                true,
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

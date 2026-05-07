using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Case;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Cases.Queries.GetReviewedAssignments
{
    // __ Returns all preliminary assignments reviewed by a specific Teaching Assistant (approved, escalated, or transferred) __ //
    public class GetReviewedAssignmentsHandler : IRequestHandler<GetReviewedAssignmentsQuery, Result<List<CaseAssignmentDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetReviewedAssignmentsHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Result<List<CaseAssignmentDto>>> Handle(GetReviewedAssignmentsQuery query, CancellationToken cancellationToken)
        {
            // __ Find the TA entity by user ID __ //
            var ta = await _uow.Repository<TeachingAssistant>().GetFirstOrDefaultAsync(
                t => t.ApplicationUserId == query.TeachingAssistantUserId,
                true
            );

            if (ta is null)
                return Result.Failure<List<CaseAssignmentDto>>("Teaching assistant not found");

            // __ Fetch the full case assignment records that were reviewed by this TA __ //
            var cases = await _uow.Repository<CaseAssignment>().GetListAsync(
                c => c.ReviewedByTAId == ta.Id,
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

using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.DTOs.Case;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Cases.Queries.GetCaseById
{
    public class GetCaseByIdHandler : IRequestHandler<GetCaseByIdQuery, Result<CaseAssignmentDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IAppLogger<GetCaseByIdHandler> _logger;

        public GetCaseByIdHandler(IUnitOfWork uow, IMapper mapper, IAppLogger<GetCaseByIdHandler> logger)
        {
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<CaseAssignmentDto>> Handle(GetCaseByIdQuery query, CancellationToken cancellationToken)
        {
            var caseAssignmentId = query.CaseAssignmentId;

            _logger.LogInfo("Fetching case assignment with ID: {CaseAssignmentId}", caseAssignmentId);

            // __ Fetch case assignment with related data __ //
            var assignment = await _uow.Repository<CaseAssignment>().GetFirstOrDefaultAsync
                (
                    c => c.Id == caseAssignmentId,
                    false,
                    c => c.DiagnosisRecord,
                    c => c.DiagnosisRecord.Booking.Patient,
                    c => c.DiagnosisRecord.DiagnosisType,
                    c => c.Clinic,
                    c => c.AssignedByIntern,
                    c => c.Sessions
                );

            // __ Handle case assignment not found __ //
            if (assignment is null)
            {
                _logger.LogWarning("Case assignment with ID: {CaseAssignmentId} not found", caseAssignmentId);
               
                return Result.Failure<CaseAssignmentDto>("الحالة غير موجودة");
            }

            _logger.LogInfo("Successfully fetched case assignment with ID: {CaseAssignmentId}", caseAssignmentId);

            // __ Return mapped __ //
            return Result.Success(_mapper.Map<CaseAssignmentDto>(assignment));
        }
    }
}

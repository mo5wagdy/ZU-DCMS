using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.DTOs.Case;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Cases.Queries.GetPendingAssignments
{
    public class GetPendingAssignmentsHandler : IRequestHandler<GetPendingAssignmentsQuery, Result<List<CaseAssignmentDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IAppLogger<GetPendingAssignmentsHandler> _logger;

        public GetPendingAssignmentsHandler(IUnitOfWork uow, IMapper mapper, IAppLogger<GetPendingAssignmentsHandler> logger)
        {
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<CaseAssignmentDto>>> Handle(GetPendingAssignmentsQuery request, CancellationToken cancellationToken)
        {
            // __ Retrieve cases using GetListAsync from the Repository to ensure adherence to project architecture __ //
            var pendingCases = await _uow.Repository<CaseAssignment>().GetListAsync(
                c => c.Status == CaseStatus.PendingAssignmentApproval,
                disabledTracking: true,
                c => c.DiagnosisRecord,
                c => c.DiagnosisRecord.Booking.Patient,
                c => c.AssignedByIntern,
                c => c.Student,
                c => c.Clinic,
                c => c.DiagnosisRecord.DiagnosisType
            );

            var resultDtos = _mapper.Map<List<CaseAssignmentDto>>(pendingCases);

            return Result.Success(resultDtos);
        }
    }
}

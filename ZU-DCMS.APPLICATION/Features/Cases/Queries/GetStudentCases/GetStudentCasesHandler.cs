using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.DTOs.Case;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Cases.Queries.GetStudentCases
{
    public class GetStudentCasesHandler : IRequestHandler<GetStudentCasesQuery, Result<List<CaseAssignmentDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IAppLogger<GetStudentCasesHandler> _logger;

        public GetStudentCasesHandler(IUnitOfWork uow, IMapper mapper, IAppLogger<GetStudentCasesHandler> logger)
        {
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<CaseAssignmentDto>>> Handle(GetStudentCasesQuery query, CancellationToken cancellationToken)
        {
            var studentId = query.StudentId;

            _logger.LogInfo("Fetching cases for student with ID: {StudentId}", studentId);

            // __ Fetch cases with related data __ //
            var cases = await _uow.Repository<CaseAssignment>().GetListAsync
            (
                c => c.StudentId == studentId,
                true,
                c => c.DiagnosisRecord,
                c => c.DiagnosisRecord.Booking.Patient,
                c => c.DiagnosisRecord.DiagnosisType,
                c => c.Clinic,
                c => c.AssignedByIntern,
                c => c.Sessions
            );

            // __ Handle no cases found __ //
            if (cases is null)
            {
                _logger.LogWarning("No cases found for student with ID: {StudentId}", studentId);
               
                return Result.Failure<List<CaseAssignmentDto>>("لا يوجد حالات لهذا الطالب");
            }

            _logger.LogInfo("Successfully fetched {Count} cases for student with ID: {StudentId}", cases.Count, studentId);
            
            // __ Return mapped __ //
            return Result.Success(_mapper.Map<List<CaseAssignmentDto>>(cases));
        }
    }
}

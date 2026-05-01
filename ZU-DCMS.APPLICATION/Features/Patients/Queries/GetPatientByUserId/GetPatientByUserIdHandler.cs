using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.DTOs.Patient;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Patients.Queries.GetPatientByUserId
{
    public class GetPatientByUserIdHandler : IRequestHandler<GetPatientByUserIdQuery, Result<PatientDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IAppLogger<GetPatientByUserIdHandler> _logger;

        public GetPatientByUserIdHandler(
            IUnitOfWork uow,
            IMapper mapper,
            IAppLogger<GetPatientByUserIdHandler> logger)
        {
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
        }

        // ________________ Get By Identity User Id ________________ //
        public async Task<Result<PatientDto>> Handle(GetPatientByUserIdQuery query, CancellationToken cancellationToken)
        {
            var userId = query.UserId;

            _logger.LogInfo("Fetching patient by User ID: {UserId}", userId);

            // __ Fetching by app user id using provided id __ //
            var patient = await _uow.Repository<Patient>().GetFirstOrDefaultAsync(p => p.ApplicationUserId == userId);

            // __ If null return failure __ //
            if (patient is null)
            {
                _logger.LogWarning("Patient not found with User ID: {UserId}", userId);
                
                return Result.Failure<PatientDto>("المريض غير موجود");
            }
            
            _logger.LogInfo("Patient found: {FullName} (User ID: {UserId})", patient.FullName, userId);
            
            var dto = _mapper.Map<PatientDto>(patient);
            
            // __ Check for active bookings (Pending, Confirmed, Delayed) __ //
            dto.HasActiveBooking = await _uow.Repository<Booking>().ExistsAsync
                (
                    b => 
                    b.PatientId == patient.Id && 
                    (b.Status == BookingStatus.Pending || 
                    b.Status == BookingStatus.Confirmed || 
                    b.Status == BookingStatus.Delayed)
                );

            // __ Check for active cases (Not Approved) __ //
            dto.HasActiveCase = await _uow.Repository<CaseAssignment>().ExistsAsync
                (
                    ca => 
                    ca.DiagnosisRecord.Booking.PatientId == patient.Id && 
                    ca.Status != CaseStatus.Approved && 
                    ca.Status != CaseStatus.Transferred
                );

            return Result.Success<PatientDto>(dto);
        }
    }
}

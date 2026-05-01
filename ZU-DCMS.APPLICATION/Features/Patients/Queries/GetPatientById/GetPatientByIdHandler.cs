using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.DTOs.Patient;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Patients.Queries.GetPatientById
{
    public class GetPatientByIdHandler : IRequestHandler<GetPatientByIdQuery, Result<PatientDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IAppLogger<GetPatientByIdHandler> _logger;

        public GetPatientByIdHandler(
            IUnitOfWork uow,
            IMapper mapper,
            IAppLogger<GetPatientByIdHandler> logger)
        {
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
        }

        // ________________ Get By Patient Id ________________ //
        public async Task<Result<PatientDto>> Handle(GetPatientByIdQuery query, CancellationToken cancellationToken)
        {
            var id = query.Id;

            _logger.LogInfo("Fetching patient by ID: {Id}", id);

            // __ Fetching by id __ // 
            var patient = await _uow.Repository<Patient>().GetByIdAsync(id);

            if (patient is null)
            {
                _logger.LogWarning("Patient not found with ID: {Id}", id);
               
                return Result.Failure<PatientDto>("المريض غير موجود");
            }
            
            _logger.LogInfo("Patient found: {FullName} (ID: {Id})", patient.FullName, patient.Id);
            
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

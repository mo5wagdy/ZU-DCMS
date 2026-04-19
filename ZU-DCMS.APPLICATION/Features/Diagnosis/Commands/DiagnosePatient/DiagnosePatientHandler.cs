using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Background_Jobs.Events;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Diagnosis.Events;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Diagnosis;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Diagnosis.Commands.DiagnosePatient
{
    public class DiagnosePatientHandler : IRequestHandler<DiagnosePatientCommand, Result<DiagnosisRecordDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IEventPublisher _eventPublisher;
        private readonly IMapper _mapper;
        private readonly IAppLogger<DiagnosePatientHandler> _logger;

        public DiagnosePatientHandler(
            IUnitOfWork uow,
            IEventPublisher eventPublisher,
            IMapper mapper,
            IAppLogger<DiagnosePatientHandler> logger)
        {
            _uow = uow;
            _eventPublisher = eventPublisher;
            _mapper = mapper;
            _logger = logger;
        }

        /* 
         * This method is called by the intern doctor when they submit a diagnosis for a patient.
         * It validates the input, checks the booking and payment status,
         * creates a diagnosis record, and emits an event for further processing. 
         */
        public async Task<Result<DiagnosisRecordDto>> Handle(DiagnosePatientCommand command, CancellationToken cancellationToken)
        {
            var internDoctorId = command.InternDoctorId;
            
            var dto = command.Dto;

            // __ Validate booking existence and related data __ //
            var booking = await _uow.Repository<Booking>()
                .GetFirstOrDefaultAsync
                (
                    b => b.Id == dto.BookingId,
                    true,
                    b => b.Patient,
                    b => b.Payment
                );

            // __ Log and return failure if booking not found __ //
            if (booking is null)
            {
                _logger.LogWarning("Booking {BookingId} not found for diagnosis by intern {InternDoctorId}", dto.BookingId, internDoctorId);
                
                return Result.Failure<DiagnosisRecordDto>("الحجز غير موجود");
            }

            // __ Check if a diagnosis already exists for the booking __ //
            var alreadyDiagnosed = await _uow.Repository<DiagnosisRecord>().ExistsAsync(d => d.BookingId == dto.BookingId);

            // __ Log and return failure if diagnosis already exists for the booking __ //
            if (alreadyDiagnosed)
            {
                _logger.LogWarning("Attempt to diagnose booking {BookingId} that has already been diagnosed by intern {InternDoctorId}", dto.BookingId, internDoctorId);
                
                return Result.Failure<DiagnosisRecordDto>("تم تشخيص هذا المريض بالفعل");
            }

            // __ Validate intern doctor existence __ //
            var intern = await _uow.Repository<InternDoctor>().GetFirstOrDefaultAsync(i => i.ApplicationUserId == internDoctorId);

            // __ Log and return failure if intern doctor not found __ //
            if (intern is null) 
            {
                _logger.LogWarning("Intern doctor with ApplicationUserId {InternDoctorId} not found", internDoctorId);
                
                return Result.Failure<DiagnosisRecordDto>("طبيب الامتياز غير موجود");
            }

            // __ Validate clinic existence and active status __ //
            var clinic = await _uow.Repository<Clinic>().GetByIdAsync(dto.ClinicId);

            // __ Log and return failure if clinic not found or inactive __ //
            if (clinic is null || !clinic.IsActive)
            {
                _logger.LogWarning("Clinic with Id {ClinicId} not found or inactive", dto.ClinicId);
               
                return Result.Failure<DiagnosisRecordDto>("العيادة غير موجودة أو غير نشطة");
            }

            // __ Validate diagnosis type existence and __ //
            var diagnosisType = await _uow.Repository<DiagnosisType>()
                .GetFirstOrDefaultAsync
                (
                    d => d.Id == dto.DiagnosisTypeId &&
                         d.ClinicId == dto.ClinicId &&
                         d.IsActive
                );

            // __ Log and return failure if diagnosis type not found or inactive __ //
            if (diagnosisType is null)
            {
                _logger.LogWarning("Diagnosis type with Id {DiagnosisTypeId} not found or inactive", dto.DiagnosisTypeId);
                
                return Result.Failure<DiagnosisRecordDto>("نوع التشخيص غير موجود");
            }

            // __ Create diagnosis record __ //
            var diagnosis = new DiagnosisRecord
            {
                BookingId = dto.BookingId,
                InternDoctorId = intern.Id,
                ClinicId = dto.ClinicId,
                DiagnosisTypeId = dto.DiagnosisTypeId,
                Complaint = dto.Complaint.Trim(),
                Notes = dto.Notes?.Trim(),
                IsAssigned = false,
                DiagnosedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            // __ Add diagnosis record to repository __ //
            await _uow.Repository<DiagnosisRecord>().AddAsync(diagnosis);

            // __ Save diagnosis record to database __ //
            await _uow.SaveChangesAsync();

            // __ Emit Event for background processing (e.g., updating patient history, notifying supervisors, etc.) __ //
            await _eventPublisher.PublishAsync(new DiagnosisCreatedEvent(diagnosis.Id, diagnosis.BookingId));

            // __ Retrieve the full diagnosis record with related data for returning to client __ //
            var full = await _uow.Repository<DiagnosisRecord>()
                .GetFirstOrDefaultAsync
                (
                    d => d.Id == diagnosis.Id,
                    true,
                    d => d.Booking,
                    d => d.Booking.Patient,
                    d => d.InternDoctor,
                    d => d.Clinic,
                    d => d.DiagnosisType
                );

            return Result.Success(_mapper.Map<DiagnosisRecordDto>(full));
        }
    }
}

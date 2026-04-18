using AutoMapper;
using ZU_DCMS.APPLICATION.Background_Jobs.Events;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Diagnosis.Events;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Diagnosis;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Diagnosis.Commands.DiagnosePatient
{
    public class DiagnosePatientHandler
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

        public async Task<Result<DiagnosisRecordDto>> Handle(DiagnosePatientCommand command)
        {
            var internDoctorId = command.InternDoctorId;
            var dto = command.Dto;

            var booking = await _uow.Repository<Domain.Entities.Booking>()
                .GetFirstOrDefaultAsync
                (
                    b => b.Id == dto.BookingId,
                    true,
                    b => b.Patient,
                    b => b.Payment
                );

            if (booking is null)
            {
                _logger.LogWarning("Booking {BookingId} not found for diagnosis by intern {InternDoctorId}", dto.BookingId, internDoctorId);
                return Result.Failure<DiagnosisRecordDto>("الحجز غير موجود");
            }

            if (booking.Payment?.Status != PaymentStatus.Paid)
            {
                _logger.LogWarning("Attempt to diagnose booking {BookingId} with unpaid status by intern {InternDoctorId}", dto.BookingId, internDoctorId);
                return Result.Failure<DiagnosisRecordDto>("لم يتم الدفع بعد");
            }

            var alreadyDiagnosed = await _uow.Repository<DiagnosisRecord>().ExistsAsync(d => d.BookingId == dto.BookingId);

            if (alreadyDiagnosed)
            {
                _logger.LogWarning("Attempt to diagnose booking {BookingId} that has already been diagnosed by intern {InternDoctorId}", dto.BookingId, internDoctorId);
                return Result.Failure<DiagnosisRecordDto>("تم تشخيص هذا المريض بالفعل");
            }

            var intern = await _uow.Repository<InternDoctor>().GetFirstOrDefaultAsync(i => i.ApplicationUserId == internDoctorId);

            if (intern is null) 
            {
                _logger.LogWarning("Intern doctor with ApplicationUserId {InternDoctorId} not found", internDoctorId);
                return Result.Failure<DiagnosisRecordDto>("طبيب الامتياز غير موجود");
            }

            var clinic = await _uow.Repository<Clinic>().GetByIdAsync(dto.ClinicId);

            if (clinic is null || !clinic.IsActive)
            {
                _logger.LogWarning("Clinic with Id {ClinicId} not found or inactive", dto.ClinicId);
                return Result.Failure<DiagnosisRecordDto>("العيادة غير موجودة أو غير نشطة");
            }

            var diagnosisType = await _uow.Repository<DiagnosisType>()
                .GetFirstOrDefaultAsync
                (
                    d => d.Id == dto.DiagnosisTypeId &&
                         d.ClinicId == dto.ClinicId &&
                         d.IsActive
                );

            if (diagnosisType is null)
            {
                _logger.LogWarning("Diagnosis type with Id {DiagnosisTypeId} not found or inactive", dto.DiagnosisTypeId);
                return Result.Failure<DiagnosisRecordDto>("نوع التشخيص غير موجود");
            }

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

            await _uow.Repository<DiagnosisRecord>().AddAsync(diagnosis);
            
            await _uow.SaveChangesAsync();

            await _eventPublisher.PublishAsync(new DiagnosisCreatedEvent(diagnosis.Id, diagnosis.BookingId));

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

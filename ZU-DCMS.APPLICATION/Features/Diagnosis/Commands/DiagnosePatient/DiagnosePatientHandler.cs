using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Background_Jobs.Events;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.DTOs.Diagnosis;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;
using ZU_DCMS.APPLICATION.Contracts.Engine;

namespace ZU_DCMS.APPLICATION.Features.Diagnosis.Commands.DiagnosePatient
{
    public class DiagnosePatientHandler : IRequestHandler<DiagnosePatientCommand, Result<DiagnosisRecordDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IEventPublisher _eventPublisher;
        private readonly IMapper _mapper;
        private readonly IAppLogger<DiagnosePatientHandler> _logger;
        private readonly IAutoAssignmentEngine _autoAssignmentEngine;

        public DiagnosePatientHandler(
            IUnitOfWork uow,
            IEventPublisher eventPublisher,
            IMapper mapper,
            IAppLogger<DiagnosePatientHandler> logger,
            IAutoAssignmentEngine autoAssignmentEngine)
        {
            _uow = uow;
            _eventPublisher = eventPublisher;
            _mapper = mapper;
            _logger = logger;
            _autoAssignmentEngine = autoAssignmentEngine;
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
                    b => b.Patient
                );

            // __ Log and return failure if booking not found __ //
            if (booking is null)
            {
                _logger.LogWarning("Booking {BookingId} not found for diagnosis by intern {InternDoctorId}", dto.BookingId, internDoctorId);
                
                return Result.Failure<DiagnosisRecordDto>("Booking not found");
            }

            // __ Check if a diagnosis already exists for the booking __ //
            var alreadyDiagnosed = await _uow.Repository<DiagnosisRecord>().ExistsAsync(d => d.BookingId == dto.BookingId);

            // __ Log and return failure if diagnosis already exists for the booking __ //
            if (alreadyDiagnosed)
            {
                _logger.LogWarning("Attempt to diagnose booking {BookingId} that has already been diagnosed by intern {InternDoctorId}", dto.BookingId, internDoctorId);
                
                return Result.Failure<DiagnosisRecordDto>("This patient has already been diagnosed");
            }

            // __ Validate intern doctor existence __ //
            var intern = await _uow.Repository<InternDoctor>().GetFirstOrDefaultAsync(i => i.ApplicationUserId == internDoctorId);

            // __ Log and return failure if intern doctor not found __ //
            if (intern is null) 
            {
                _logger.LogWarning("Intern doctor with ApplicationUserId {InternDoctorId} not found", internDoctorId);
                
                return Result.Failure<DiagnosisRecordDto>("Intern doctor not found");
            }

            // __ Validate diagnosis Type existence and activity __ //
            var diagnosisType = await _uow.Repository<DiagnosisType>()
                .GetFirstOrDefaultAsync
                (
                    d => d.Id == dto.DiagnosisTypeId && d.IsActive
                );

            if (diagnosisType is null)
            {
                _logger.LogWarning("Diagnosis Type with Id {DiagnosisTypeId} not found or inactive", dto.DiagnosisTypeId);
                return Result.Failure<DiagnosisRecordDto>("Diagnosis type not found");
            }

            var isValidLink = await _uow.Repository<ClinicDiagnosisType>().ExistsAsync
                (
                    x =>
                        x.ClinicId        == dto.ClinicId &&
                        x.DiagnosisTypeId == dto.DiagnosisTypeId &&
                        x.IsActive
                );

            if (!isValidLink)
            {
                _logger.LogWarning("Diagnosis Type {DiagnosisTypeId} is not linked to Clinic {ClinicId}", dto.DiagnosisTypeId, dto.ClinicId);
                
                return Result.Failure<DiagnosisRecordDto>("Diagnosis type does not belong to this clinic");
            }

            // __ Validate clinic existence and active status __ //
            var clinic = await _uow.Repository<Clinic>().GetByIdAsync(dto.ClinicId);

            if (clinic is null || !clinic.IsActive)
            {
                _logger.LogWarning("Clinic with Id {ClinicId} not found or inactive", dto.ClinicId);
                return Result.Failure<DiagnosisRecordDto>("Clinic not found or inactive");
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
            await _uow.SaveChangesAsync(cancellationToken: cancellationToken);

            // __ Run auto-assignment engine synchronously to select the best student __ //
            var assignmentResult = await _autoAssignmentEngine.GetBestStudentForCaseAsync(diagnosis);
            
            if (assignmentResult.IsSuccess)
            {
                var studentId = assignmentResult.Value;
                var student = await _uow.Repository<Student>().GetByIdAsync(studentId);
                
                if (student != null && student.ActiveTermId.HasValue)
                {
                    // __ Create preliminary assignment and add it to TA pending queue __ //
                    var caseAssignment = new CaseAssignment
                    {
                        DiagnosisRecordId = diagnosis.Id,
                        StudentId = studentId,
                        ClinicId = dto.ClinicId,
                        TermId = student.ActiveTermId.Value,
                        AssignedByInternId = intern.Id,
                        Status = CaseStatus.PendingAssignmentApproval,
                        IsAutoAssigned = true,
                        AssignedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        Notes = "Auto-assigned - Pending TA Review"
                    };

                    await _uow.Repository<CaseAssignment>().AddAsync(caseAssignment);
                    
                    // __ Update diagnosis status to (Assigned) __ //
                    diagnosis.IsAssigned = true;
                    _uow.Repository<DiagnosisRecord>().Update(diagnosis);
                    
                    await _uow.SaveChangesAsync(cancellationToken: cancellationToken);
                    
                    // __ Link assignment to the booking __ //
                    booking.CaseAssignmentId = caseAssignment.Id;
                    _uow.Repository<Booking>().Update(booking);
                    
                    await _uow.SaveChangesAsync(cancellationToken: cancellationToken);
                }
            }

            // __ Emit Event for background processing (e.g., updating patient history, notifying supervisors, etc.) __ //
            //await _eventPublisher.PublishAsync(new DiagnosisCreatedEvent(diagnosis.Id, diagnosis.BookingId));

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

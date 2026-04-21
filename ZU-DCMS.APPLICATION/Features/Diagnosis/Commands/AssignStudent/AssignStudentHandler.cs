using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Background_Jobs.Events;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Case;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.DTOs.Case;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Diagnosis.Commands.AssignStudent
{
    public class AssignStudentHandler : IRequestHandler<AssignStudentCommand, Result<CaseAssignmentDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IEventPublisher _eventPublisher;
        private readonly IMapper _mapper;
        private readonly IAppLogger<AssignStudentHandler> _logger;

        public AssignStudentHandler(
            IUnitOfWork uow,
            IEventPublisher eventPublisher,
            IMapper mapper,
            IAppLogger<AssignStudentHandler> logger)
        {
            _uow = uow;
            _eventPublisher = eventPublisher;
            _mapper = mapper;
            _logger = logger;
        }

        /* 
         * This method is called by the intern doctor when they assign a student to a diagnosed case. It validates the input,
         * checks the diagnosis record and student status, creates a case assignment, updates the diagnosis record,
         * emits an event for further processing, and returns the details of the created assignment.
         */
        public async Task<Result<CaseAssignmentDto>> Handle(AssignStudentCommand command, CancellationToken cancellationToken)
        {
            var internDoctorId = command.InternDoctorId;
            
            var dto = command.Dto;

            // __ Validate diagnosis record existence and related data __ //
            var diagnosis = await _uow.Repository<DiagnosisRecord>()
                .GetFirstOrDefaultAsync
                (
                    d => d.Id == dto.DiagnosisRecordId,
                    true,
                    d => d.Booking,
                    d => d.Booking.Patient,
                    d => d.Clinic,
                    d => d.DiagnosisType
                );

            // __ Log and return failure if diagnosis record not found __ //
            if (diagnosis is null)
            {
                _logger.LogWarning("Diagnosis record {DiagnosisRecordId} not found for case assignment by intern {InternDoctorId}", dto.DiagnosisRecordId, internDoctorId);
                
                return Result.Failure<CaseAssignmentDto>("سجل التشخيص غير موجود");
            }

            // __ Log and return failure if diagnosis record is already assigned to a student __ //
            if (diagnosis.IsAssigned)
            {
                _logger.LogWarning("Attempt to assign student to diagnosis record {DiagnosisRecordId} that is already assigned by intern {InternDoctorId}", dto.DiagnosisRecordId, internDoctorId);
               
                return Result.Failure<CaseAssignmentDto>("تم التعيين بالفعل");
            }

            // __ Validate intern doctor existence __ //
            var intern = await _uow.Repository<InternDoctor>().GetFirstOrDefaultAsync(i => i.ApplicationUserId == internDoctorId);

            // __ Log and return failure if intern doctor not found __ //
            if (intern is null)
            {
                _logger.LogWarning("Intern doctor {InternDoctorId} not found for case assignment", internDoctorId);
            
                return Result.Failure<CaseAssignmentDto>("طبيب الامتياز غير موجود");
            }

            // __ Validate student existence and active status __ //
            var student = await _uow.Repository<Student>().GetByIdAsync(dto.StudentId);

            // __ Log and return failure if student not found or inactive __ //
            if (student is null || !student.IsActive)
            {
                _logger.LogWarning("Student with Id {StudentId} not found or inactive for case assignment by intern {InternDoctorId}", dto.StudentId, internDoctorId);
               
                return Result.Failure<CaseAssignmentDto>("الطالب غير صالح");
            }

            // __ Log and return failure if student does not have an active term __ //
            if (student.ActiveTermId is null)
            {
                _logger.LogWarning("Student with Id {StudentId} does not have an active term for case assignment by intern {InternDoctorId}", dto.StudentId, internDoctorId);
                
                return Result.Failure<CaseAssignmentDto>("لا يوجد ترم نشط");
            }

            // __ To ensure data integrity, we will perform the following operations within a transaction: __ //
            await _uow.BeginTransactionAsync();

            try
            {
                // __ Create case assignment record __ //
                var assignment = new CaseAssignment
                {
                    DiagnosisRecordId = dto.DiagnosisRecordId,
                    StudentId = dto.StudentId,
                    ClinicId = dto.ClinicId,
                    AssignedByInternId = intern.Id,
                    Status = CaseStatus.InProgress,
                    Notes = dto.Notes?.Trim(),
                    AssignedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                // __ Add case assignment to repository __ //
                await _uow.Repository<CaseAssignment>().AddAsync(assignment);

                // __ Update diagnosis record to mark it as assigned __ //
                diagnosis.IsAssigned = true;

                // __ Update diagnosis record in repository __ //
                _uow.Repository<DiagnosisRecord>().Update(diagnosis);

                await _uow.CommitTransactionAsync(internDoctorId);

                // __ Emit Event for background processing (e.g., notifying the assigned student, updating case load, etc.) __ //
                await _eventPublisher.PublishAsync(new CaseAssignedEvent
                (
                    assignment.Id,
                    dto.StudentId,
                    dto.ClinicId
                ));

                // __ Retrieve the full case assignment with related data for returning to client __ //
                var full = await _uow.Repository<CaseAssignment>()
                    .GetFirstOrDefaultAsync
                    (
                        a => a.Id == assignment.Id,
                        true,
                        a => a.DiagnosisRecord,
                        a => a.DiagnosisRecord.Booking.Patient,
                        a => a.Clinic,
                        a => a.AssignedByIntern,
                        a => a.DiagnosisRecord.DiagnosisType
                    );

                return Result.Success(_mapper.Map<CaseAssignmentDto>(full));
            }

            // __ If any error occurs during the process, roll back the transaction, log the error, and return failure __ //
            catch (Exception ex)
            {
                await _uow.RollbackTransactionAsync();
               
                _logger.LogError("Error assigning student", ex);
               
                return Result.Failure<CaseAssignmentDto>("فشل التعيين");
            }
        }
    }
}

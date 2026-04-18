using AutoMapper;
using ZU_DCMS.APPLICATION.Background_Jobs.Events;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Case;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Case;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Diagnosis.Commands.AssignStudent
{
    public class AssignStudentHandler
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

        public async Task<Result<CaseAssignmentDto>> Handle(AssignStudentCommand command)
        {
            var internDoctorId = command.InternDoctorId;
            var dto = command.Dto;

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

            if (diagnosis is null)
            {
                _logger.LogWarning("Diagnosis record {DiagnosisRecordId} not found for case assignment by intern {InternDoctorId}", dto.DiagnosisRecordId, internDoctorId);
                return Result.Failure<CaseAssignmentDto>("سجل التشخيص غير موجود");
            }

            if (diagnosis.IsAssigned)
            {
                _logger.LogWarning("Attempt to assign student to diagnosis record {DiagnosisRecordId} that is already assigned by intern {InternDoctorId}", dto.DiagnosisRecordId, internDoctorId);
                return Result.Failure<CaseAssignmentDto>("تم التعيين بالفعل");
            }

            var intern = await _uow.Repository<InternDoctor>().GetFirstOrDefaultAsync(i => i.ApplicationUserId == internDoctorId);

            if (intern is null)
            {
                _logger.LogWarning("Intern doctor {InternDoctorId} not found for case assignment", internDoctorId);
                return Result.Failure<CaseAssignmentDto>("طبيب الامتياز غير موجود");
            }

            var student = await _uow.Repository<Domain.Entities.Student>().GetByIdAsync(dto.StudentId);

            if (student is null || !student.IsActive)
            {
                _logger.LogWarning("Student with Id {StudentId} not found or inactive for case assignment by intern {InternDoctorId}", dto.StudentId, internDoctorId);
                return Result.Failure<CaseAssignmentDto>("الطالب غير صالح");
            }

            if (student.ActiveTermId is null)
            {
                _logger.LogWarning("Student with Id {StudentId} does not have an active term for case assignment by intern {InternDoctorId}", dto.StudentId, internDoctorId);
                return Result.Failure<CaseAssignmentDto>("لا يوجد ترم نشط");
            }

            await _uow.BeginTransactionAsync();

            try
            {
                var assignment = new CaseAssignment
                {
                    DiagnosisRecordId = dto.DiagnosisRecordId,
                    StudentId = dto.StudentId,
                    ClinicId = dto.ClinicId,
                    AssignedByInternId = intern.Id,
                    Status = CaseStatus.Active,
                    Notes = dto.Notes?.Trim(),
                    AssignedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await _uow.Repository<CaseAssignment>().AddAsync(assignment);

                diagnosis.IsAssigned = true;

                _uow.Repository<DiagnosisRecord>().Update(diagnosis);

                await _uow.CommitTransactionAsync(internDoctorId);

                await _eventPublisher.PublishAsync(new CaseAssignedEvent
                (
                    assignment.Id,
                    dto.StudentId,
                    dto.ClinicId
                ));

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
            catch (Exception ex)
            {
                await _uow.RollbackTransactionAsync();
                _logger.LogError("Error assigning student", ex);
                return Result.Failure<CaseAssignmentDto>("فشل التعيين");
            }
        }
    }
}

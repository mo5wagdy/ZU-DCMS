using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Background_Jobs.Events;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.DTOs.Case;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;
using ZU_DCMS.APPLICATION.Common.Cache;
using ZiggyCreatures.Caching.Fusion;

namespace ZU_DCMS.APPLICATION.Features.Diagnosis.Commands.AssignStudent
{
        // __ Handles the assignment of a student to a diagnosed case (DiagnosisRecord). __ //
    public class AssignStudentHandler : IRequestHandler<AssignStudentCommand, Result<CaseAssignmentDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IFusionCache _cache;
        private readonly IEventPublisher _eventPublisher;
        private readonly IMapper _mapper;
        private readonly IAppLogger<AssignStudentHandler> _logger;

        public AssignStudentHandler(
            IUnitOfWork uow,
            IFusionCache cache,
            IEventPublisher eventPublisher,
            IMapper mapper,
            IAppLogger<AssignStudentHandler> logger)
        {
            _uow = uow;
            _cache = cache;
            _eventPublisher = eventPublisher;
            _mapper = mapper;
            _logger = logger;
        }

        // __ Main handler method for assigning a student to a case. __ //
        public async Task<Result<CaseAssignmentDto>> Handle(AssignStudentCommand command, CancellationToken cancellationToken)
        {
            var internDoctorId = command.InternDoctorId;
            var dto = command.Dto;

            // _____________ VALIDATION STEP 1: Validate DiagnosisRecord _____________ //
        // __ Load the DiagnosisRecord and its related data. __ //
            var diagnosis = await _uow.Repository<DiagnosisRecord>().GetFirstOrDefaultAsync
                (
                    d => d.Id == dto.DiagnosisRecordId,
                    disabledTracking: true,
                    d => d.Booking,
                    d => d.Booking.Patient,
                    d => d.Clinic,
                    d => d.DiagnosisType
                );

            if (diagnosis is null)
            {
                _logger.LogWarning($"Diagnosis record {dto.DiagnosisRecordId} not found for case assignment by intern {internDoctorId}");
                
                return Result.Failure<CaseAssignmentDto>("سجل التشخيص غير موجود");
            }

            if (diagnosis.IsAssigned)
            {
                _logger.LogWarning($"Diagnosis record {dto.DiagnosisRecordId} is already assigned. Duplicate assignment attempted by intern {internDoctorId}");
                
                return Result.Failure<CaseAssignmentDto>("تم التعيين بالفعل");
            }

            // _____________ AUTO-CLINIC SELECTION STEP 2: Extract ClinicId from DiagnosisRecord _____________ //
        // __ CRITICAL: ClinicId is NOT provided in the request. __ //
            var clinicId = diagnosis.ClinicId;
            var clinic = diagnosis.Clinic;

            if (clinic is null || !clinic.IsActive)
            {
                _logger.LogWarning($"Clinic {clinicId} is null or inactive");
               
                return Result.Failure<CaseAssignmentDto>("العيادة غير صالحة");
            }

            // _____________ VALIDATION STEP 2.1: Block Diagnosis Clinic (ID 1) _____________ //
            if (clinicId == 1)
            {
                _logger.LogWarning($"Attempted to assign student {dto.StudentId} to Diagnosis Clinic (ID 1)");
                
                return Result.Failure<CaseAssignmentDto>("لا يمكن تعيين الطلاب في عيادة التشخيص");
            }

            // _____________ VALIDATION STEP 3: Validate InternDoctor _____________ //
        // __ Verify that the intern doctor making the assignment exists and is valid. __ //
            var intern = await _uow.Repository<InternDoctor>().GetFirstOrDefaultAsync(i => i.ApplicationUserId == internDoctorId);

            if (intern is null)
            {
                _logger.LogWarning($"Intern doctor {internDoctorId} not found");
               
                return Result.Failure<CaseAssignmentDto>("طبيب الامتياز غير موجود");
            }

            // _____________ VALIDATION STEP 4: Validate Student Basic Requirements _____________ //
        // __ Load student and check: __ //
            var student = await _uow.Repository<Student>().GetByIdAsync(dto.StudentId);

            if (student is null || !student.IsActive)
            {
                _logger.LogWarning($"Student {dto.StudentId} not found or inactive");
                
                return Result.Failure<CaseAssignmentDto>("الطالب غير صالح");
            }

            if (student.ActiveTermId is null)
            {
                _logger.LogWarning($"Student {dto.StudentId} does not have an active term");
               
                return Result.Failure<CaseAssignmentDto>("لا يوجد ترم نشط");
            }

            // _____________ VALIDATION STEP 5: Academic Year Constraint _____________ //
        // __ Validate that student's academic year falls within the clinic's allowed range. __ //
            if (student.AcademicYear < clinic.MinAcademicYear || student.AcademicYear > clinic.MaxAcademicYear)
            {
                _logger.LogWarning($"Student {dto.StudentId} academic year {student.AcademicYear} outside clinic range [{clinic.MinAcademicYear}-{clinic.MaxAcademicYear}]");

                return Result.Failure<CaseAssignmentDto>($"الطالب في سنة دراسية غير مناسبة للعيادة (متطلب: {clinic.MinAcademicYear}-{clinic.MaxAcademicYear}, الطالب في: {student.AcademicYear})");
            }

            // _____________ VALIDATION STEP 6: Unsatisfied TermRequirement Check _____________ //
        // __ Verify that the student has an unsatisfied TermRequirement for this clinic. __ //
            var termRequirement = await _uow.Repository<TermRequirement>().GetFirstOrDefaultAsync
                (
                    r => r.StudentId == dto.StudentId &&
                         r.TermId == student.ActiveTermId &&
                         r.ClinicId == clinicId,
                         disabledTracking: true
                );

            if (termRequirement is null)
            {
                _logger.LogWarning($"No TermRequirement found for student {dto.StudentId} in clinic {clinicId} for term {student.ActiveTermId}");

                return Result.Failure<CaseAssignmentDto>("الطالب غير مسجل في هذه العيادة لهذا الترم");
            }

            if (termRequirement.IsSatisfied)
            {
                _logger.LogInfo($"Student {dto.StudentId} has already satisfied clinic {clinicId} requirement");

                return Result.Failure<CaseAssignmentDto>("الطالب قد أكمل متطلبات هذه العيادة");
            }

            // _____________ VALIDATION STEP 7: Workload Check - Student Capacity _____________ //
        // __ Count the number of active (InProgress) cases the student currently has in this clinic. __ //
            var activeCasesCount = await _uow.Repository<CaseAssignment>().CountAsync
                (
                    ca => ca.StudentId == dto.StudentId &&
                          ca.ClinicId == clinicId &&
                          ca.Status == CaseStatus.InProgress
                );

            if (activeCasesCount >= clinic.MaxCasesPerStudent)
            {
                _logger.LogWarning($"Student {dto.StudentId} at capacity in clinic {clinicId}: {activeCasesCount}/{clinic.MaxCasesPerStudent}");

                return Result.Failure<CaseAssignmentDto>($"الطالب في حده الأقصى من الحالات في هذه العيادة ({activeCasesCount}/{clinic.MaxCasesPerStudent})");
            }

            // _____________ TRANSACTION BEGIN: All-or-Nothing Operation _____________ //
        // __ Start a database transaction to ensure that both operations succeed or both fail: __ //
            await _uow.BeginTransactionAsync();

            try
            {
                // _____________ CREATE CaseAssignment _____________ //
        // __ Create a new CaseAssignment with: __ //
                var assignment = new CaseAssignment
                {
                    DiagnosisRecordId = dto.DiagnosisRecordId,
                    StudentId = dto.StudentId,
                    ClinicId = clinicId, 
                    TermId = student.ActiveTermId.Value, 
                    AssignedByInternId = intern.Id,
                    Status = CaseStatus.InProgress, 
                    Notes = dto.Notes?.Trim(),
                    AssignedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                // Add to repository
                await _uow.Repository<CaseAssignment>().AddAsync(assignment);

                // _____________ UPDATE DiagnosisRecord _____________ //
        // __ Mark the DiagnosisRecord as assigned to prevent duplicate assignments. __ //
                diagnosis.IsAssigned = true;
                _uow.Repository<DiagnosisRecord>().Update(diagnosis);

                // _____________ SAVE & COMMIT TRANSACTION _____________ //
        // __ Persist both changes in a single transaction. __ //
                await _uow.SaveChangesAsync(internDoctorId);
                await _uow.CommitTransactionAsync();

                // _____________ INVALIDATE CACHE _____________ //
                // __ Remove student cases cache to reflect new assignment. __ //
                await _cache.RemoveAsync(CacheKeys.StudentCases(dto.StudentId));
                // __ Remove available students for this clinic as workload has changed. __ //
                await _cache.RemoveAsync(CacheKeys.AvailableStudents(clinicId));
                // __ Remove student user profile cache if it exists (handles useStudentContext updates) __ //
                await _cache.RemoveAsync(CacheKeys.StudentByUserId(student.ApplicationUserId));

                _logger.LogInfo($"Case assignment successful: Diagnosis {dto.DiagnosisRecordId} -> Student {dto.StudentId} in Clinic {clinicId}");

                // _____________ PUBLISH EVENT _____________ //
        // __ Emit a CaseAssignedEvent for background processing: __ //
               // await _eventPublisher.PublishAsync(new CaseAssignedEvent
                //(
                //    assignment.Id,
                //    dto.StudentId,
                //    clinicId
                //));

                // _____________ LOAD FULL RECORD FOR RESPONSE _____________ //
        // __ Retrieve the complete CaseAssignment with all related data for the response. __ //
                var full = await _uow.Repository<CaseAssignment>()
                    .GetFirstOrDefaultAsync
                    (
                        a => a.Id == assignment.Id,
                        disabledTracking: true,
                        a => a.DiagnosisRecord,
                        a => a.DiagnosisRecord.Booking.Patient,
                        a => a.Clinic,
                        a => a.AssignedByIntern,
                        a => a.DiagnosisRecord.DiagnosisType,
                        a => a.Student,
                        a => a.Term
                    );

                return Result.Success(_mapper.Map<CaseAssignmentDto>(full));
            }

            // _____________ ERROR HANDLING _____________ //
        // __ If any error occurs during the transaction: __ //
            catch (Exception ex)
            {
                await _uow.RollbackTransactionAsync();
                
                _logger.LogError($"Error assigning student {dto.StudentId} to diagnosis {dto.DiagnosisRecordId}", ex);
                
                return Result.Failure<CaseAssignmentDto>("فشل التعيين. حاول مرة أخرى أو تواصل مع الدعم الفني");
            }
        }
    }
}

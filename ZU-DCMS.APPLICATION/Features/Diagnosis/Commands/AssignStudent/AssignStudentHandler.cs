using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Background_Jobs.Events;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.DTOs.Case;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Diagnosis.Commands.AssignStudent
{
    /// <summary>
    /// Handles the assignment of a student to a diagnosed case (DiagnosisRecord).
    /// 
    /// This handler implements strict validation based on:
    /// 1. Academic Year Constraints: Student's year must match clinic's min/max range
    /// 2. Unsatisfied Requirements: Student must have an unsatisfied TermRequirement for this clinic
    /// 3. Workload Limits: Student cannot exceed the clinic's max cases per student
    /// 4. Auto-Clinic Selection: ClinicId is retrieved from DiagnosisRecord, not the request
    /// 5. Data Integrity: TermId comes from student's ActiveTermId
    /// 
    /// Workflow:
    /// 1. Load and validate DiagnosisRecord
    /// 2. Load clinic details and constraints (MinAcademicYear, MaxAcademicYear, MaxCasesPerStudent)
    /// 3. Validate student eligibility:
    ///    - Student exists and is active
    ///    - Student's academic year within clinic range
    ///    - Student has unsatisfied requirement for this clinic
    ///    - Student has capacity for more cases
    /// 4. Create CaseAssignment within transaction (atomic operation)
    /// 5. Mark DiagnosisRecord as assigned
    /// 6. Publish event for background processing
    /// 7. Return confirmation to caller
    /// </summary>
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

        /// <summary>
        /// Main handler method for assigning a student to a case.
        /// Implements strict academic year and workload validation.
        /// </summary>
        public async Task<Result<CaseAssignmentDto>> Handle(AssignStudentCommand command, CancellationToken cancellationToken)
        {
            var internDoctorId = command.InternDoctorId;
            var dto = command.Dto;

            // _____________ VALIDATION STEP 1: Validate DiagnosisRecord _____________ //
            /// <summary>
            /// Load the DiagnosisRecord and its related data.
            /// Check that:
            /// - Record exists
            /// - Record is not already assigned
            /// - Clinic and DiagnosisType are loaded
            /// </summary>
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
            /// <summary>
            /// CRITICAL: ClinicId is NOT provided in the request.
            /// Instead, it is automatically retrieved from the DiagnosisRecord.
            /// 
            /// Reason for Auto-Population:
            /// - Ensures data consistency: Case is assigned in the same clinic where diagnosis was made
            /// - Prevents mismatches: Prevents assigning a case to a student from a different clinic
            /// - Simplifies API: Client doesn't need to know clinic ID
            /// 
            /// The ClinicId links to clinic-specific constraints (academic year range, workload limits).
            /// </summary>
            var clinicId = diagnosis.ClinicId;
            var clinic = diagnosis.Clinic;

            if (clinic is null || !clinic.IsActive)
            {
                _logger.LogWarning($"Clinic {clinicId} is null or inactive");
               
                return Result.Failure<CaseAssignmentDto>("العيادة غير صالحة");
            }

            // _____________ VALIDATION STEP 3: Validate InternDoctor _____________ //
            /// <summary>
            /// Verify that the intern doctor making the assignment exists and is valid.
            /// </summary>
            var intern = await _uow.Repository<InternDoctor>().GetFirstOrDefaultAsync(i => i.ApplicationUserId == internDoctorId);

            if (intern is null)
            {
                _logger.LogWarning($"Intern doctor {internDoctorId} not found");
               
                return Result.Failure<CaseAssignmentDto>("طبيب الامتياز غير موجود");
            }

            // _____________ VALIDATION STEP 4: Validate Student Basic Requirements _____________ //
            /// <summary>
            /// Load student and check:
            /// - Student exists in database
            /// - Student is marked as active (not suspended/deleted)
            /// - Student has an active term (required for case assignment)
            /// </summary>
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
            /// <summary>
            /// Validate that student's academic year falls within the clinic's allowed range.
            /// 
            /// Example Scenario:
            /// - Surgery clinic: MinAcademicYear = 3, MaxAcademicYear = 4
            /// - Student.AcademicYear = 2
            /// - Result: FAIL - Student is too junior for this clinic
            /// 
            /// This constraint ensures students have appropriate skills for clinic complexity.
            /// </summary>
            if (student.AcademicYear < clinic.MinAcademicYear || student.AcademicYear > clinic.MaxAcademicYear)
            {
                _logger.LogWarning($"Student {dto.StudentId} academic year {student.AcademicYear} outside clinic range [{clinic.MinAcademicYear}-{clinic.MaxAcademicYear}]");

                return Result.Failure<CaseAssignmentDto>($"الطالب في سنة دراسية غير مناسبة للعيادة (متطلب: {clinic.MinAcademicYear}-{clinic.MaxAcademicYear}, الطالب في: {student.AcademicYear})");
            }

            // _____________ VALIDATION STEP 6: Unsatisfied TermRequirement Check _____________ //
            /// <summary>
            /// Verify that the student has an unsatisfied TermRequirement for this clinic.
            /// 
            /// This check ensures:
            /// - Student is enrolled in this clinic for current term
            /// - Student still needs to complete cases (requirement not yet satisfied)
            /// - Student can receive more case assignments
            /// 
            /// Query details:
            /// - StudentId = the student being assigned
            /// - TermId = student's ActiveTermId
            /// - ClinicId = auto-selected from DiagnosisRecord
            /// - IsSatisfied = false (only unsatisfied requirements)
            /// </summary>
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
            /// <summary>
            /// Count the number of active (InProgress) cases the student currently has in this clinic.
            /// If count >= clinic's MaxCasesPerStudent, the student is at capacity and cannot accept more.
            /// 
            /// Example:
            /// - Clinic.MaxCasesPerStudent = 3
            /// - Student already has 3 active cases in this clinic
            /// - Result: FAIL - Student is at capacity
            /// 
            /// This prevents student overload and ensures case quality through focused attention.
            /// 
            /// Status Condition:
            /// Only count cases that are Active or InProgress (Completed/Transferred don't take up capacity).
            /// </summary>
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
            /// <summary>
            /// Start a database transaction to ensure that both operations succeed or both fail:
            /// 1. Create CaseAssignment record
            /// 2. Mark DiagnosisRecord as assigned
            /// 
            /// If either operation fails, BOTH are rolled back to maintain consistency.
            /// </summary>
            await _uow.BeginTransactionAsync();

            try
            {
                // _____________ CREATE CaseAssignment _____________ //
                /// <summary>
                /// Create a new CaseAssignment with:
                /// - DiagnosisRecordId: Links to the diagnosed case
                /// - StudentId: The student assigned to treat the case
                /// - ClinicId: Auto-retrieved from DiagnosisRecord (NOT from client input)
                /// - TermId: Auto-retrieved from student's ActiveTermId (ensures data consistency)
                /// - AssignedByInternId: The doctor who made the assignment
                /// - Status: Initially set to InProgress (student will begin treatment)
                /// - AssignedAt: Current timestamp for audit trail
                /// </summary>
                var assignment = new CaseAssignment
                {
                    DiagnosisRecordId = dto.DiagnosisRecordId,
                    StudentId = dto.StudentId,
                    ClinicId = clinicId,  // ✅ Auto-populated from DiagnosisRecord
                    TermId = student.ActiveTermId.Value,  // ✅ Auto-populated from Student.ActiveTermId
                    AssignedByInternId = intern.Id,
                    Status = CaseStatus.InProgress,  // Student will start working immediately
                    Notes = dto.Notes?.Trim(),
                    AssignedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                // Add to repository
                await _uow.Repository<CaseAssignment>().AddAsync(assignment);

                // _____________ UPDATE DiagnosisRecord _____________ //
                /// <summary>
                /// Mark the DiagnosisRecord as assigned to prevent duplicate assignments.
                /// This flag is critical for preventing one case being assigned to multiple students.
                /// </summary>
                diagnosis.IsAssigned = true;
                _uow.Repository<DiagnosisRecord>().Update(diagnosis);

                // _____________ SAVE & COMMIT TRANSACTION _____________ //
                /// <summary>
                /// Persist both changes in a single transaction.
                /// If any error occurs, the entire transaction is rolled back.
                /// </summary>
                await _uow.SaveChangesAsync(internDoctorId);
                await _uow.CommitTransactionAsync();

                _logger.LogInfo($"Case assignment successful: Diagnosis {dto.DiagnosisRecordId} -> Student {dto.StudentId} in Clinic {clinicId}");

                // _____________ PUBLISH EVENT _____________ //
                /// <summary>
                /// Emit a CaseAssignedEvent for background processing:
                /// - Send notification to student about new assignment
                /// - Update dashboards in real-time (via SignalR if available)
                /// - Trigger any async workflows (e.g., email notifications)
                /// </summary>
               // await _eventPublisher.PublishAsync(new CaseAssignedEvent
                //(
                //    assignment.Id,
                //    dto.StudentId,
                //    clinicId
                //));

                // _____________ LOAD FULL RECORD FOR RESPONSE _____________ //
                /// <summary>
                /// Retrieve the complete CaseAssignment with all related data for the response.
                /// This ensures the client has all information needed.
                /// </summary>
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
            /// <summary>
            /// If any error occurs during the transaction:
            /// 1. Rollback transaction to undo partial changes
            /// 2. Log the error for investigation
            /// 3. Return failure result to client
            /// </summary>
            catch (Exception ex)
            {
                await _uow.RollbackTransactionAsync();
                
                _logger.LogError($"Error assigning student {dto.StudentId} to diagnosis {dto.DiagnosisRecordId}", ex);
                
                return Result.Failure<CaseAssignmentDto>("فشل التعيين. حاول مرة أخرى أو تواصل مع الدعم الفني");
            }
        }
    }
}

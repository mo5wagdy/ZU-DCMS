
using AutoMapper;
using ZU_DCMS.APPLICATION.Background_Jobs.Events;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Case;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Diagnosis.Events;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Case;
using ZU_DCMS.APPLICATION.DTOs.Diagnosis;
using ZU_DCMS.APPLICATION.DTOs.Student;
using ZU_DCMS.APPLICATION.Services.Interfaces;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;


namespace ZU_DCMS.APPLICATION.Services.Implementations
{
    public class DiagnosisService : IDiagnosisService
    {
        private readonly IUnitOfWork _uow;
        private readonly IBookingService _bookingService;
        private readonly IAiAgentService _aiAgent;
        private readonly IEventPublisher _eventPublisher;
        private readonly IMapper _mapper;
        private readonly IAppLogger<DiagnosisService> _logger;

        public DiagnosisService(
            IUnitOfWork uow,
            IBookingService bookingService,
            IAiAgentService aiAgent,
            IEventPublisher eventPublisher,
            IMapper mapper,
            IAppLogger<DiagnosisService> logger)
        {
            _uow = uow;
            _bookingService = bookingService;
            _aiAgent = aiAgent;
            _eventPublisher = eventPublisher;
            _mapper = mapper;
            _logger = logger;
        }

        // __ This method is called by the intern doctor when they access the diagnosis page for a session. It checks if the session is valid and returns the list of patients booked for that session. __
        public async Task<Result<List<BookingForDiagnosisDto>>> GetSessionPatientsAsync(int sessionId, string internDoctorId)
        {
            // __ Validate session existence and access __ //
            var session = await _uow.Repository<Session>().GetByIdAsync(sessionId);

            // __ Log and return failure if session not found __ //
            if (session is null)
            {
                _logger.LogWarning("Session {SessionId} not found for intern {InternDoctorId}", sessionId, internDoctorId);

                return Result.Failure<List<BookingForDiagnosisDto>>("السكشن غير موجود");
            }

            // __ Log and return failure if session date is not today __ //
            if (session.Date.Date != DateTime.Today)
            {
                _logger.LogWarning("Intern {InternDoctorId} attempted to access session {SessionId} for date {SessionDate}", internDoctorId, sessionId, session.Date);

                return Result.Failure<List<BookingForDiagnosisDto>>("لا يمكن الوصول لمرضى سكشن يوم آخر");
            }

            // __ Log and return failure if session has not started yet __ //
            if (DateTime.Now.TimeOfDay < session.StartTime)
            {
                _logger.LogInfo("Intern {InternDoctorId} attempted to access session {SessionId} before start time {StartTime}", internDoctorId, sessionId, session.StartTime);

                return Result.Failure<List<BookingForDiagnosisDto>>("السكشن لم يبدأ بعد");
            }

            // __ Get confirmed bookings for the session, including patient details __ //
            var sessionBookingsResult = _bookingService.GetSessionBookingsAsync(sessionId, new PagedRequest { Page = 1, PageSize = int.MaxValue }).Result;
            
            // __ Check if fetching bookings was successful __ //
            if (sessionBookingsResult.IsFailure)
            {
                _logger.LogWarning("Failed to fetch bookings for session {SessionId} by intern {InternDoctorId}", sessionId, internDoctorId);

                return Result.Failure<List<BookingForDiagnosisDto>>(sessionBookingsResult.Error);
            }

            // __ Map bookings to DTOs and return success __ //
            var dtos = _mapper.Map<List<BookingForDiagnosisDto>>(sessionBookingsResult.Value.Items);

            return Result.Success(dtos);
        }


        // __ This method is called by the intern doctor when they submit a diagnosis for a patient. It validates the input, checks the booking and payment status, creates a diagnosis record, and emits an event for further processing. __ //
        public async Task<Result<DiagnosisRecordDto>> DiagnosePatientAsync(string internDoctorId, CreateDiagnosisDto dto)
        {
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

            // __ Log and return failure if payment is not completed __ //
            if (booking.Payment?.Status != PaymentStatus.Paid)
            {
                _logger.LogWarning("Attempt to diagnose booking {BookingId} with unpaid status by intern {InternDoctorId}", dto.BookingId, internDoctorId);

                return Result.Failure<DiagnosisRecordDto>("لم يتم الدفع بعد");
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


        // __ This method is called by the intern doctor when they want to see the list of available students for case assignment. It retrieves the term requirements for the clinic and term, calls the AI agent to get a prioritized list of student IDs, and returns the list of students with their priority and completion status. __ //
        public async Task<Result<List<StudentPriorityDto>>> GetAvailableStudentsAsync(int clinicId, int termId)
        {
            // __ Get term requirements for the clinic and term, including student details __ //
            var requirements = await _uow.Repository<TermRequirement>()
                .GetListAsync
                (
                    r => r.ClinicId == clinicId && r.TermId == termId,
                    true,
                    r => r.Student
                );

            // __ If no requirements found, return empty list __ //
            if (!requirements.Any())
                return Result.Success(new List<StudentPriorityDto>());

            // __ List to hold prioritized student IDs from AI agent __ //
            List<int> prioritizedIds;

            // __ Call AI agent to get prioritized list of student IDs based on requirements __ //
            try
            {
                // __ The AI agent will analyze the requirements and return a list of student IDs ordered by priority for case assignment __ //
                prioritizedIds = await _aiAgent.GetStudentPriorityListAsync(clinicId, termId);
            }

            // __ If AI agent fails (e.g., due to an error or timeout), log the error and fall back to a default prioritization based on the Priority field in the requirements __ //
            catch (Exception ex)
            {

                _logger.LogError("AI fallback for clinic {ClinicId}", ex,  clinicId);

                // __ Fallback: order students by the Priority field defined in the requirements __ //
                prioritizedIds = requirements
                    .OrderBy(r => r.Priority)
                    .Select(r => r.StudentId)
                    .ToList();
            }

            // __ Create a dictionary of requirements for quick lookup by student ID __ //
            var dict = requirements.ToDictionary(r => r.StudentId);

            // __ Build the list of StudentPriorityDto based on the prioritized IDs, including completion status and priority ranking __ //
            var dtos = prioritizedIds
                .Where(dict.ContainsKey)
                .Select((studentId, index) =>
                {
                    var req = dict[studentId];

                    return new StudentPriorityDto
                    {
                        StudentId = studentId,
                        FullName = req.Student.FullName,
                        StudentCode = req.Student.StudentCode,
                        CompletedCases = req.CompletedCount,
                        RequiredCases = req.RequiredCount,
                        Priority = index + 1,
                        IsComplete = req.IsSatisfied
                    };
                })
                .ToList();

            // __ Return the list of students with their priority and completion status __ //
            return Result.Success(dtos);
        }


        // __ This method is called by the intern doctor when they assign a student to a diagnosed case. It validates the input, checks the diagnosis record and student status, creates a case assignment, updates the diagnosis record, emits an event for further processing, and returns the details of the created assignment. __ //
        public async Task<Result<CaseAssignmentDto>> AssignStudentAsync(string internDoctorId, AssignStudentDto dto)
        {
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
                    Status = CaseStatus.Active,
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

                // __ Save changes to database and commit transaction __ //
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

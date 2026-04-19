using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Diagnosis;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Diagnosis.Queries.GetSessionPatients
{
    public class GetSessionPatientsHandler : IRequestHandler<GetSessionPatientsQuery, Result<PagedResult<BookingForDiagnosisDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IAppLogger<GetSessionPatientsHandler> _logger;

        public GetSessionPatientsHandler
        (
            IUnitOfWork uow,
            IMapper mapper,
            IAppLogger<GetSessionPatientsHandler> logger
        )
        {
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
        }

        // __ This method is called by the intern doctor when they access the diagnosis page for a session. It checks if the session is valid and returns the list of patients booked for that session. __
        public async Task<Result<PagedResult<BookingForDiagnosisDto>>> Handle(GetSessionPatientsQuery query, CancellationToken cancellationToken)
        {
            var sessionId = query.SessionId;
            
            var internDoctorId = query.InternDoctorId;

            // __ Validate session existence and access __ //
            var session = await _uow.Repository<Session>().GetByIdAsync(sessionId);

            // __ Log and return failure if session not found __ //
            if (session is null)
            {
                _logger.LogWarning("Session {SessionId} not found for intern {InternDoctorId}", sessionId, internDoctorId);
            
                return Result.Failure<PagedResult<BookingForDiagnosisDto>>("السكشن غير موجود");
            }

            // __ Log and return failure if session date is not today __ //
            if (session.Date.Date != DateTime.Today)
            {
                _logger.LogWarning("Intern {InternDoctorId} attempted to access session {SessionId} for date {SessionDate}", internDoctorId, sessionId, session.Date);
               
                return Result.Failure<PagedResult<BookingForDiagnosisDto>>("لا يمكن الوصول لمرضى سكشن يوم آخر");
            }

            // __ Log and return failure if session has not started yet __ //
            if (DateTime.Now.TimeOfDay < session.StartTime)
            {
                _logger.LogInfo("Intern {InternDoctorId} attempted to access session {SessionId} before start time {StartTime}", internDoctorId, sessionId, session.StartTime);
               
                return Result.Failure<PagedResult<BookingForDiagnosisDto>>("السكشن لم يبدأ بعد");
            }
            _logger.LogInfo("Fetching confirmed bookings for SessionId: {SessionId}", sessionId);

            // __ Get confirmed bookings for the session, ordered by creation time, including patient details __ //
            var request = new PagedRequest();

            var (items, total) = await _uow.Repository<Booking>().GetPagedListAsync
                (
                    (request.Page - 1) * request.PageSize,
                    request.PageSize,
                    b => b.SessionId == sessionId && b.Status == BookingStatus.Confirmed,
                    true,
                    q => q.OrderBy(b => b.CreatedAt),
                    b => b.Patient
                );

            // __ Map to DTOs __ //
            var dtos = _mapper.Map<List<BookingForDiagnosisDto>>(items);

            // __ Return paged result __ //
            var pagedResult = PagedResult<BookingForDiagnosisDto>.Create(dtos, total, request);

            _logger.LogInfo("Fetched {Count} confirmed bookings for SessionId: {SessionId}", dtos.Count, sessionId);

            return Result.Success(pagedResult);
        }
    }
}

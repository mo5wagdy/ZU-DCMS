using AutoMapper;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Diagnosis;
using ZU_DCMS.APPLICATION.Services.Interfaces;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Diagnosis.Queries.GetSessionPatients
{
    public class GetSessionPatientsHandler
    {
        private readonly IUnitOfWork _uow;
        private readonly IBookingService _bookingService;
        private readonly IMapper _mapper;
        private readonly IAppLogger<GetSessionPatientsHandler> _logger;

        public GetSessionPatientsHandler(
            IUnitOfWork uow,
            IBookingService bookingService,
            IMapper mapper,
            IAppLogger<GetSessionPatientsHandler> logger)
        {
            _uow = uow;
            _bookingService = bookingService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<BookingForDiagnosisDto>>> Handle(GetSessionPatientsQuery query)
        {
            var sessionId = query.SessionId;
            var internDoctorId = query.InternDoctorId;

            var session = await _uow.Repository<Session>().GetByIdAsync(sessionId);

            if (session is null)
            {
                _logger.LogWarning("Session {SessionId} not found for intern {InternDoctorId}", sessionId, internDoctorId);
                return Result.Failure<List<BookingForDiagnosisDto>>("السكشن غير موجود");
            }

            if (session.Date.Date != DateTime.Today)
            {
                _logger.LogWarning("Intern {InternDoctorId} attempted to access session {SessionId} for date {SessionDate}", internDoctorId, sessionId, session.Date);
                return Result.Failure<List<BookingForDiagnosisDto>>("لا يمكن الوصول لمرضى سكشن يوم آخر");
            }

            if (DateTime.Now.TimeOfDay < session.StartTime)
            {
                _logger.LogInfo("Intern {InternDoctorId} attempted to access session {SessionId} before start time {StartTime}", internDoctorId, sessionId, session.StartTime);
                return Result.Failure<List<BookingForDiagnosisDto>>("السكشن لم يبدأ بعد");
            }

            var sessionBookingsResult = await _bookingService.GetSessionBookingsAsync(sessionId, new PagedRequest { Page = 1, PageSize = int.MaxValue });
            
            if (sessionBookingsResult.IsFailure)
            {
                _logger.LogWarning("Failed to fetch bookings for session {SessionId} by intern {InternDoctorId}", sessionId, internDoctorId);
                return Result.Failure<List<BookingForDiagnosisDto>>(sessionBookingsResult.Error);
            }

            var dtos = _mapper.Map<List<BookingForDiagnosisDto>>(sessionBookingsResult.Value.Items);

            return Result.Success(dtos);
        }
    }
}

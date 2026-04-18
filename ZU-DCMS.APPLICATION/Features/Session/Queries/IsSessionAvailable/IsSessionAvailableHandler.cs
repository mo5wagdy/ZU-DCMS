using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Session.Queries.IsSessionAvailable
{
    public class IsSessionAvailableHandler
    {
        private readonly IUnitOfWork _uow;
        private readonly IAppLogger<IsSessionAvailableHandler> _logger;

        public IsSessionAvailableHandler(IUnitOfWork uow, IAppLogger<IsSessionAvailableHandler> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(IsSessionAvailableQuery query)
        {
            var sessionId = query.SessionId;
            var bookingType = query.BookingType;

            _logger.LogInfo("Checking availability for SessionId: {SessionId}, BookingType: {BookingType}", sessionId, bookingType);

            var session = await _uow.Repository<Domain.Entities.Session>().GetByIdAsync(sessionId);

            if (session is null)
            { 
                _logger.LogWarning("Session not found for SessionId: {SessionId}", sessionId);
                return Result.Failure<bool>("السكشن غير موجود");
            }

            if (!session.IsActive)
            {
                _logger.LogWarning("Session is not active for SessionId: {SessionId}", sessionId);
                return Result.Failure<bool>("السكشن غير نشط");
            }

            var isAvailable = bookingType == BookingType.New ? !session.IsNewFull : !session.IsFollowUpFull;

            _logger.LogInfo("Session availability for SessionId: {SessionId}, BookingType: {BookingType} is {IsAvailable}", sessionId, bookingType, isAvailable);

            return Result.Success(isAvailable);
        }
    }
}

using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Sessions.Queries.IsSessionAvailable
{
    public class IsSessionAvailableHandler : IRequestHandler<IsSessionAvailableQuery, Result<bool>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IAppLogger<IsSessionAvailableHandler> _logger;

        public IsSessionAvailableHandler(IUnitOfWork uow, IAppLogger<IsSessionAvailableHandler> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        // __ This method checks if a specific session is available for booking based on the booking type __ //
        public async Task<Result<bool>> Handle(IsSessionAvailableQuery query, CancellationToken cancellationToken)
        {
            var sessionId = query.SessionId;
            var bookingType = query.BookingType;

            _logger.LogInfo("Checking availability for SessionId: {SessionId}, BookingType: {BookingType}", sessionId, bookingType);

            // __ Fetch the session by Id __ //
            var session = await _uow.Repository<Session>().GetByIdAsync(sessionId);

            // __ Handle if not found or not active __ //
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

            // __ Determine availability based on booking type __ //
            var isAvailable = bookingType == BookingType.New ? !session.IsNewFull : !session.IsFollowUpFull;

            _logger.LogInfo("Session availability for SessionId: {SessionId}, BookingType: {BookingType} is {IsAvailable}", sessionId, bookingType, isAvailable);

            return Result.Success(isAvailable);
        }
    }
}

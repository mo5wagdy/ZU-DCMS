using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.DTOs.Session;
using ZU_DCMS.APPLICATION.Features.Sessions.Commands.GenerateSessions;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Sessions.Queries.GetAvailableSlots
{
    public class GetAvailableSlotsHandler : IRequestHandler<GetAvailableSlotsQuery, Result<List<AvailableSlotDto>>>
    {
        private const int MaxLookAheadDays = 14;

        private readonly IUnitOfWork _uow;
        private readonly IMediator _mediator;
        private readonly IAppLogger<GetAvailableSlotsHandler> _logger;

        public GetAvailableSlotsHandler(
            IUnitOfWork uow,
            IMediator mediator,
            IAppLogger<GetAvailableSlotsHandler> logger)
        {
            _uow = uow;
            _mediator = mediator;
            _logger = logger;
        }

        // __________ Get Available Slots __________ //
        public async Task<Result<List<AvailableSlotDto>>> Handle(GetAvailableSlotsQuery query, CancellationToken cancellationToken)
        {
            var bookingType = query.BookingType;

            // __ We will look ahead up to MaxLookAheadDays to find available slots __ //
            var slots = new List<AvailableSlotDto>();
            var currentDate = DateTime.Today;
            var daysChecked = 0;

            _logger.LogInfo("Fetching available slots for {BookingType}", bookingType);

            // __ Loop through upcoming days until we find 4 available slots or reach the look-ahead limit __ //
            while (slots.Count < 4 && daysChecked < MaxLookAheadDays)
            {
                // __ Skip Fridays __ //
                if (currentDate.DayOfWeek == DayOfWeek.Friday)
                {
                    currentDate = currentDate.AddDays(1);
                    daysChecked++;
                    continue;
                }

                // __ Get sessions for the current date __ //
                var sessions = await _uow.Repository<Session>().GetListAsync
                (s => 
                    s.Date.Date == currentDate.Date &&
                    s.IsActive &&
                   !s.IsDeleted
                );

                // __ If no sessions exist, generate them __ //
                if (!sessions.Any())
                {
                    _logger.LogInfo("No sessions found, generating for {Date}", currentDate);

                    await _mediator.Send(new GenerateSessionsCommand(currentDate), cancellationToken);

                    sessions = await _uow.Repository<Session>().GetListAsync
                    (s => 
                        s.Date.Date == currentDate.Date &&
                        s.IsActive &&
                       !s.IsDeleted
                    );
                }

                // __ Check each session for availability based on booking type __ //
                foreach (var session in sessions.OrderBy(s => s.StartTime))
                {
                    var isAvailable = bookingType == BookingType.New ? !session.IsNewFull : !session.IsFollowUpFull;

                    // __ If this session is not available for the booking type, skip it __ //
                    if (!isAvailable) continue;

                    // __ Add available slot info to the list __ //
                    slots.Add(new AvailableSlotDto
                    {
                        SessionId = session.Id,
                        Date = session.Date,
                        StartTime = session.StartTime.ToString(@"hh\:mm"),
                        EndTime = session.EndTime.ToString(@"hh\:mm"),
                        AvailableNewSlots = session.MaxNewPatients - session.CurrentNewCount,
                        AvailableFollowUpSlots = session.MaxFollowUpPatients - session.CurrentFollowUpCount,
                        IsAvailable = true
                    });

                    // __ If we have found 4 available slots, we can stop looking further __ //
                    if (slots.Count >= 4) break;
                }

                // __ Move to the next day __ //
                currentDate = currentDate.AddDays(1);
               
                daysChecked++;
            }

            _logger.LogInfo("Found {Count} available slots for {BookingType}", slots.Count, bookingType);
            
            return Result.Success(slots);
        }        
    }
}

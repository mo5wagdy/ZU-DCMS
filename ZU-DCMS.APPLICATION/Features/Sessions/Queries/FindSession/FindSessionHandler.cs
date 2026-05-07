using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Session;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Sessions.Queries.FindSession
{
    public class FindSessionHandler : IRequestHandler<FindSessionQuery, Result<SessionDto>>
    {
        private readonly IUnitOfWork _uow;

        public FindSessionHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Result<SessionDto>> Handle(FindSessionQuery query, CancellationToken cancellationToken)
        {
            var date = query.Date;
            
            var timeSlot = query.TimeSlot;

            // __ Validate time slot format __ //
            if (!TimeSpan.TryParse(timeSlot, out var time))
                return Result.Failure<SessionDto>("Invalid time format");

            // __ Fetch the session that matches the date and time slot __ //
            var session = await _uow.Repository<Session>().GetFirstOrDefaultAsync
                (s => 
                    s.Date.Date == date.Date &&
                    s.StartTime == time &&
                    s.IsActive &&
                   !s.IsDeleted
                );

            // __ If session is not found, return failure __ //
            if (session == null)
                return Result.Failure<SessionDto>("Session not found");

            // __ Map domain entity to DTO to avoid exposing domain internals __ //
            var dto = new SessionDto
            {
                Id = session.Id,
                Date = session.Date,
                StartTime = session.StartTime.ToString(@"hh\:mm"),
                EndTime = session.EndTime.ToString(@"hh\:mm"),
                MaxNewPatients = session.MaxNewPatients,
                MaxFollowUpPatients = session.MaxFollowUpPatients,
                CurrentNewCount = session.CurrentNewCount,
                CurrentFollowUpCount = session.CurrentFollowUpCount,
                IsFull = session.IsFull,
                IsNewFull = session.IsNewFull,
                IsFollowUpFull = session.IsFollowUpFull
            };

            return Result.Success(dto);
        }
    }
}

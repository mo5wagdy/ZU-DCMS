using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Sessions.Queries.FindSession
{
    public class FindSessionHandler : IRequestHandler<FindSessionQuery, Result<Session>>
    {
        private readonly IUnitOfWork _uow;

        public FindSessionHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Result<Session>> Handle(FindSessionQuery query, CancellationToken cancellationToken)
        {
            var date = query.Date;
            
            var timeSlot = query.TimeSlot;

            // __ Validate time slot format __ //
            if (!TimeSpan.TryParse(timeSlot, out var time))
                return Result.Failure<Session>("صيغة الوقت غير صحيحة");

            // __ Fetch the session that matches the date and time slot __ //
            var session = await _uow.Repository<Session>().GetFirstOrDefaultAsync
                (s => 
                    s.Date.Date == date.Date &&
                    s.StartTime == time &&
                    s.IsActive &&
                   !s.IsDeleted
                );

            // __ If session is not found, return failure __ //
            return session == null ? Result.Failure<Session>("السكشن غير موجود") : Result.Success(session);
        }
    }
}

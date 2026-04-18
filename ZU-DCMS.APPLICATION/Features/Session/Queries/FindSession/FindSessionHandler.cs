using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Session.Queries.FindSession
{
    public class FindSessionHandler
    {
        private readonly IUnitOfWork _uow;

        public FindSessionHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Result<Domain.Entities.Session>> Handle(FindSessionQuery query)
        {
            var date = query.Date;
            var timeSlot = query.TimeSlot;

            if (!TimeSpan.TryParse(timeSlot, out var time))
                return Result.Failure<Domain.Entities.Session>("صيغة الوقت غير صحيحة");

            var session = await _uow.Repository<Domain.Entities.Session>().GetFirstOrDefaultAsync
                (s => s.Date.Date == date.Date &&
                      s.StartTime == time &&
                      s.IsActive &&
                     !s.IsDeleted
                );

            return session == null ? Result.Failure<Domain.Entities.Session>("السكشن غير موجود") : Result.Success(session);
        }
    }
}

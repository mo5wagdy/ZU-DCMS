using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Session.Queries.GetSessionEntityById
{
    public class GetSessionEntityByIdHandler
    {
        private readonly IUnitOfWork _uow;

        public GetSessionEntityByIdHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Result<Domain.Entities.Session>> Handle(GetSessionEntityByIdQuery query)
        {
            var session = await _uow.Repository<Domain.Entities.Session>().GetByIdAsync(query.SessionId);

            return session is null ? Result.Failure<Domain.Entities.Session>("السكشن غير موجود") : Result.Success(session);
        }
    }
}

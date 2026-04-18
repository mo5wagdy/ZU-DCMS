using AutoMapper;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Session;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Session.Queries.GetSessionById
{
    public class GetSessionByIdHandler
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IAppLogger<GetSessionByIdHandler> _logger;

        public GetSessionByIdHandler(IUnitOfWork uow, IMapper mapper, IAppLogger<GetSessionByIdHandler> logger)
        {
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<SessionDto>> Handle(GetSessionByIdQuery query)
        {
            var sessionId = query.SessionId;

            _logger.LogInfo("Fetching session by Id: {SessionId}", sessionId);

            var session = await _uow.Repository<Domain.Entities.Session>().GetByIdAsync(sessionId);

            if (session is null)
            {
                _logger.LogWarning("Session not found for Id: {SessionId}", sessionId);
                return Result.Failure<SessionDto>("السكشن غير موجود");
            }

            _logger.LogInfo("Session found for Id: {SessionId}", sessionId);

            return Result.Success(_mapper.Map<SessionDto>(session));
        }
    }
}

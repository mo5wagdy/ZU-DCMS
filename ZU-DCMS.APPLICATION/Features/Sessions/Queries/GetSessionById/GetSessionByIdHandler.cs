using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.DTOs.Session;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Sessions.Queries.GetSessionById
{
    public class GetSessionByIdHandler : IRequestHandler<GetSessionByIdQuery, Result<SessionDto>>
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

        // __________ Get By Id __________ //
        public async Task<Result<SessionDto>> Handle(GetSessionByIdQuery query, CancellationToken cancellationToken)
        {
            var sessionId = query.SessionId;

            _logger.LogInfo("Fetching session by Id: {SessionId}", sessionId);

            // __ Fetch the session by Id __ //
            var session = await _uow.Repository<Session>().GetByIdAsync(sessionId);

            // __ Handle if not found __ //
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

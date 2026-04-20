using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.DTOs.Session;
using ZU_DCMS.APPLICATION.Features.Sessions.Commands.GenerateSessions;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Sessions.Queries.GetTodaySessions
{
    public class GetTodaySessionsHandler : IRequestHandler<GetTodaySessionsQuery, Result<List<SessionDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IAppLogger<GetTodaySessionsHandler> _logger;

        public GetTodaySessionsHandler
        (
            IUnitOfWork uow,
            IMediator mediator,
            IMapper mapper,
            IAppLogger<GetTodaySessionsHandler> logger
        )
        {
            _uow = uow;
            _mediator = mediator;
            _mapper = mapper;
            _logger = logger;
        }

        // __________ Get Today Sessions __________ //
        public async Task<Result<List<SessionDto>>> Handle(GetTodaySessionsQuery query, CancellationToken cancellationToken)
        {
            // __ We will fetch sessions for today, and if they don't exist, we will generate them __ //
            var today = DateTime.Today;

            _logger.LogInfo("Fetching today's sessions");

            // __ Get sessions for today __ //
            var sessions = await _uow.Repository<Session>().GetListAsync(
                s => s.Date.Date == today &&
                     s.IsActive &&
                    !s.IsDeleted);

            // __ If no sessions exist for today, generate them __ //
            if (!sessions.Any())
            {
                _logger.LogInfo("No sessions today, generating...");

                var generateResult = await _mediator.Send(new GenerateSessionsCommand(today), cancellationToken);

                if (generateResult.IsFailure)
                {
                    _logger.LogError("Failed to generate today's sessions.");
                    
                    return Result.Failure<List<SessionDto>>(generateResult.Error);
                }

                // __ Fetch the newly generated sessions __ //
                sessions = await _uow.Repository<Session>().GetListAsync
                (s =>
                    s.Date.Date == today &&
                    s.IsActive &&
                   !s.IsDeleted
                );
            }

            _logger.LogInfo("Found {Count} sessions for today", sessions.Count);

            return Result.Success(_mapper.Map<List<SessionDto>>(sessions.OrderBy(s => s.StartTime)));
        }
    }
}

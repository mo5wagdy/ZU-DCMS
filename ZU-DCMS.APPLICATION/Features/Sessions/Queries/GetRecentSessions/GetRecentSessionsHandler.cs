using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.DTOs.Session;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Sessions.Queries.GetRecentSessions
{
    public class GetRecentSessionsHandler : IRequestHandler<GetRecentSessionsQuery, Result<List<SessionDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IAppLogger<GetRecentSessionsHandler> _logger;

        public GetRecentSessionsHandler(IUnitOfWork uow, IMapper mapper, IAppLogger<GetRecentSessionsHandler> logger)
        {
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<SessionDto>>> Handle(GetRecentSessionsQuery query, CancellationToken cancellationToken)
        {
            var startDate = DateTime.Today.AddDays(-query.DaysCount);
            var endDate = DateTime.Today; // Only past sessions up to today

            _logger.LogInfo("Fetching recent sessions from {StartDate} to {EndDate}", startDate, endDate);

            var sessions = await _uow.Repository<Session>().GetListAsync
            (
                s => s.Date.Date >= startDate && s.Date.Date <= endDate && s.IsActive && !s.IsDeleted
            );

            _logger.LogInfo("Found {Count} recent sessions", sessions.Count);

            return Result.Success(_mapper.Map<List<SessionDto>>(sessions.OrderByDescending(s => s.Date).ThenByDescending(s => s.StartTime)));
        }
    }
}

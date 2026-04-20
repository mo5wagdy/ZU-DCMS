using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Cache;
using ZU_DCMS.APPLICATION.Contracts.Cache;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.DTOs.Session;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Sessions.Commands.GenerateSessions
{
    public class GenerateSessionsHandler : IRequestHandler<GenerateSessionsCommand, Result<List<SessionDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICacheService _cache;
        private readonly IMapper _mapper;
        private readonly IAppLogger<GenerateSessionsHandler> _logger;

        public GenerateSessionsHandler(
            IUnitOfWork uow,
            ICacheService cache,
            IMapper mapper,
            IAppLogger<GenerateSessionsHandler> logger)
        {
            _uow = uow;
            _cache = cache;
            _mapper = mapper;
            _logger = logger;
        }

        // __________ Generate Sessions __________ //
        public async Task<Result<List<SessionDto>>> Handle(GenerateSessionsCommand command, CancellationToken cancellationToken)
        {
            var date = command.Date;

            _logger.LogInfo("Generating sessions for {Date}", date);

            // __ Prevent generating sessions on Fridays __ //
            if (date.DayOfWeek == DayOfWeek.Friday)
            {
                _logger.LogWarning("Attempt to generate sessions on Friday {Date}", date);
                
                return Result.Failure<List<SessionDto>>("الجمعة إجازة"); 
            }

            // __ Check if sessions already exist for the date __ //
            var exists = await _uow.Repository<Session>().ExistsAsync(s => s.Date.Date == date.Date);

            if (exists)
            {
                _logger.LogWarning("Sessions already exist for {Date}", date);
              
                return Result.Failure<List<SessionDto>>("السكاشن موجودة بالفعل");
            }

            // __ Get session configuration __ //
            var config = await GetSessionConfigAsync();
            
            if (config is null)
            {
                _logger.LogError("Session configuration is missing");
               
                return Result.Failure<List<SessionDto>>("إعدادات السكاشن غير موجودة");
            }

            // __ Create sessions based on configuration __ //
            var sessions = config.SessionTimes.Select(time => new Session
            {
                Date = date.Date,
                StartTime = time,
                EndTime = time.Add(TimeSpan.FromHours(2)),
                MaxNewPatients = config.MaxNewPerSession,
                MaxFollowUpPatients = config.MaxFollowUpPerSession,
                CurrentNewCount = 0,
                CurrentFollowUpCount = 0,
                IsActive = true
            }).ToList();

            // __ Save sessions to database __ //
            await _uow.Repository<Session>().AddRangeAsync(sessions);
            
            await _uow.SaveChangesAsync(cancellationToken: cancellationToken);

            _logger.LogInfo("Sessions created successfully for {Date} Count: {Count}", date, sessions.Count);
            
            return Result.Success(_mapper.Map<List<SessionDto>>(sessions));
        }

        // __________ Private Helpers __________ //
        private async Task<SessionConfig?> GetSessionConfigAsync()
        {
            // __ Try to get session configuration from cache first __ //
            var cached = await _cache.GetAsync<SessionConfig>(CacheKeys.SessionConfig);
           
            if (cached != null) return cached;

            // __ If not in cache, fetch from database __ //
            var configs = await _uow.Repository<SystemConfig>().GetListAsync(c => c.Key == ConfigKeys.SessionTimes || c.Key == ConfigKeys.MaxNewPerSession || c.Key == ConfigKeys.MaxFollowUpPerSession);

            // __ We need all three configurations to proceed __ //
            if (configs.Count < 3) return null;

            // __ Convert list of configs to a dictionary for easier access __ //
            var dict = configs.ToDictionary(c => c.Key, c => c.Value);

            // __ Parse session times from comma-separated string __ //
            var times = dict[ConfigKeys.SessionTimes]
                .Split(',')
                .Select(t => TimeSpan.Parse(t.Trim()))
                .ToList();

            // __ Create session configuration object __ //
            var config = new SessionConfig
            {
                SessionTimes = times,
                MaxNewPerSession = int.Parse(dict[ConfigKeys.MaxNewPerSession]),
                MaxFollowUpPerSession = int.Parse(dict[ConfigKeys.MaxFollowUpPerSession])
            };

            // __ Cache the session configuration for future use __ //
            await _cache.SetAsync(CacheKeys.SessionConfig, config, CacheDuration.Medium);

            return config;
        }

        // __ This class is used to hold session configuration values after parsing them from the database __ //
        private sealed class SessionConfig
        {
            public List<TimeSpan> SessionTimes { get; init; } = new();
            public int MaxNewPerSession { get; init; }
            public int MaxFollowUpPerSession { get; init; }
        }
    }
}

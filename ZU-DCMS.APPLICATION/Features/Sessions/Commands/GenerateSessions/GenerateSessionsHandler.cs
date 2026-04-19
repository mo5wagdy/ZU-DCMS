using AutoMapper;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Session;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Session.Commands.GenerateSessions
{
    public class GenerateSessionsHandler
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

        public async Task<Result<List<SessionDto>>> Handle(GenerateSessionsCommand command)
        {
            var date = command.Date;

            _logger.LogInfo("Generating sessions for {Date}", date);

            if (date.DayOfWeek == DayOfWeek.Friday)
            {
                _logger.LogWarning("Attempt to generate sessions on Friday {Date}", date);
                return Result.Failure<List<SessionDto>>("الجمعة إجازة"); 
            }

            var exists = await _uow.Repository<Domain.Entities.Session>().ExistsAsync(s => s.Date.Date == date.Date);

            if (exists)
            {
                _logger.LogWarning("Sessions already exist for {Date}", date);
                return Result.Failure<List<SessionDto>>("السكاشن موجودة بالفعل");
            }

            var config = await GetSessionConfigAsync();
            
            if (config is null)
            {
                _logger.LogError("Session configuration is missing");
                return Result.Failure<List<SessionDto>>("إعدادات السكاشن غير موجودة");
            }

            var sessions = config.SessionTimes.Select(time => new Domain.Entities.Session
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

            await _uow.Repository<Domain.Entities.Session>().AddRangeAsync(sessions);
            await _uow.SaveChangesAsync();

            _logger.LogInfo("Sessions created successfully for {Date} Count: {Count}", date, sessions.Count);
            
            return Result.Success(_mapper.Map<List<SessionDto>>(sessions));
        }

        private async Task<SessionConfig?> GetSessionConfigAsync()
        {
            var cached = await _cache.GetAsync<SessionConfig>(CacheKeys.SessionConfig);
            if (cached != null) return cached;

            var configs = await _uow.Repository<SystemConfig>().GetListAsync(c => c.Key == ConfigKeys.SessionTimes || c.Key == ConfigKeys.MaxNewPerSession || c.Key == ConfigKeys.MaxFollowUpPerSession);

            if (configs.Count < 3) return null;

            var dict = configs.ToDictionary(c => c.Key, c => c.Value);

            var times = dict[ConfigKeys.SessionTimes]
                .Split(',')
                .Select(t => TimeSpan.Parse(t.Trim()))
                .ToList();

            var config = new SessionConfig
            {
                SessionTimes = times,
                MaxNewPerSession = int.Parse(dict[ConfigKeys.MaxNewPerSession]),
                MaxFollowUpPerSession = int.Parse(dict[ConfigKeys.MaxFollowUpPerSession])
            };

            await _cache.SetAsync(CacheKeys.SessionConfig, config, CacheDuration.Medium);

            return config;
        }

        private sealed class SessionConfig
        {
            public List<TimeSpan> SessionTimes { get; init; } = new();
            public int MaxNewPerSession { get; init; }
            public int MaxFollowUpPerSession { get; init; }
        }
    }
}

using AutoMapper;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Session;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Session.Queries.GetTodaySessions
{
    public class GetTodaySessionsHandler
    {
        private readonly IUnitOfWork _uow;
        private readonly ICacheService _cache;
        private readonly IMapper _mapper;
        private readonly IAppLogger<GetTodaySessionsHandler> _logger;

        public GetTodaySessionsHandler(
            IUnitOfWork uow,
            ICacheService cache,
            IMapper mapper,
            IAppLogger<GetTodaySessionsHandler> logger)
        {
            _uow = uow;
            _cache = cache;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<SessionDto>>> Handle(GetTodaySessionsQuery query)
        {
            var today = DateTime.Today;

            _logger.LogInfo("Fetching today's sessions");

            var sessions = await _uow.Repository<Domain.Entities.Session>().GetListAsync(
                s => s.Date.Date == today &&
                     s.IsActive &&
                    !s.IsDeleted);

            if (!sessions.Any())
            {
                _logger.LogInfo("No sessions today, generating...");

                var generateResult = await GenerateSessionsAsync(today);

                if (generateResult.IsFailure)
                {
                    _logger.LogError("Failed to generate today's sessions.");
                    return Result.Failure<List<SessionDto>>(generateResult.Error);
                }

                sessions = await _uow.Repository<Domain.Entities.Session>().GetListAsync(
                    s => s.Date.Date == today &&
                         s.IsActive &&
                        !s.IsDeleted);
            }

            _logger.LogInfo("Found {Count} sessions for today", sessions.Count);

            return Result.Success(_mapper.Map<List<SessionDto>>(sessions.OrderBy(s => s.StartTime)));
        }

        private async Task<Result<List<SessionDto>>> GenerateSessionsAsync(DateTime date)
        {
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

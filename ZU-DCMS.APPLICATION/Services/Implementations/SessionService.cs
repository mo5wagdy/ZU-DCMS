using AutoMapper;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Booking;
using ZU_DCMS.APPLICATION.DTOs.Session;
using ZU_DCMS.APPLICATION.Services.Interfaces;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Services.Implementations;

public class SessionService : ISessionService
{
    private const int MaxLookAheadDays = 14;

    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;

    public SessionService(
        IUnitOfWork uow,
        ICacheService cache,
        IMapper mapper)
    {
        _uow = uow;
        _cache = cache;
        _mapper = mapper;
    }

    // __________ Generate Sessions __________ //
    public async Task<Result<List<SessionDto>>> GenerateSessionsAsync(DateTime date)
    {
        if (date.DayOfWeek == DayOfWeek.Friday)
            return Result.Failure<List<SessionDto>>("الجمعة إجازة");

        var exists = await _uow.Repository<Session>().ExistsAsync(s => s.Date.Date == date.Date);

        if (exists)
            return Result.Failure<List<SessionDto>>("السكاشن موجودة بالفعل");

        var config = await GetSessionConfigAsync();
        
        if (config is null)
            return Result.Failure<List<SessionDto>>("إعدادات السكاشن غير موجودة");
        
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

        await _uow.Repository<Session>().AddRangeAsync(sessions);
        await _uow.SaveChangesAsync();

        return Result.Success(_mapper.Map<List<SessionDto>>(sessions));
    }

    // __________ Get Available Slots __________ //
    public async Task<Result<List<AvailableSlotDto>>> GetAvailableSlotsAsync(BookingType bookingType)
    {
        var slots = new List<AvailableSlotDto>();
        var currentDate = DateTime.Today;
        var daysChecked = 0;

        while (slots.Count < 4 && daysChecked < MaxLookAheadDays)
        {
            if (currentDate.DayOfWeek == DayOfWeek.Friday)
            {
                currentDate = currentDate.AddDays(1);
                daysChecked++;
                continue;
            }

            var sessions = await _uow.Repository<Session>().GetListAsync(s =>
                    s.Date.Date == currentDate.Date &&
                    s.IsActive &&
                    !s.IsDeleted);

            if (!sessions.Any())
            {
                await GenerateSessionsAsync(currentDate);

                sessions = await _uow.Repository<Session>().GetListAsync(s =>
                        s.Date.Date == currentDate.Date &&
                        s.IsActive &&
                        !s.IsDeleted);
            }

            foreach (var session in sessions.OrderBy(s => s.StartTime))
            {
                var isAvailable = bookingType == BookingType.New ? !session.IsNewFull : !session.IsFollowUpFull;

                if (!isAvailable) continue;

                slots.Add(new AvailableSlotDto
                {
                    SessionId = session.Id,
                    Date = session.Date,
                    StartTime = session.StartTime.ToString(@"hh\:mm"),
                    EndTime = session.EndTime.ToString(@"hh\:mm"),
                    AvailableNewSlots = session.MaxNewPatients - session.CurrentNewCount,
                    AvailableFollowUpSlots = session.MaxFollowUpPatients - session.CurrentFollowUpCount,
                    IsAvailable = true
                });

                if (slots.Count >= 4) break;
            }

            currentDate = currentDate.AddDays(1);
            daysChecked++;
        }

        return Result.Success(slots);
    }

    // __________ Get Today Sessions __________ //
    public async Task<Result<List<SessionDto>>> GetTodaySessionsAsync()
    {
        var today = DateTime.Today;

        var sessions = await _uow.Repository<Session>().GetListAsync(s =>
                s.Date.Date == today &&
                s.IsActive &&
                !s.IsDeleted);

        if (!sessions.Any())
        {
            var generateResult = await GenerateSessionsAsync(today);
            if (generateResult.IsFailure)
                return Result.Failure<List<SessionDto>>(generateResult.Error);

            sessions = await _uow.Repository<Session>().GetListAsync(s =>
                    s.Date.Date == today &&
                    s.IsActive &&
                    !s.IsDeleted);
        }

        return Result.Success(_mapper.Map<List<SessionDto>>(sessions.OrderBy(s => s.StartTime)));
    }

    // __________ Is Session Available __________ //
    public async Task<Result<bool>> IsSessionAvailableAsync(int sessionId, BookingType bookingType)
    {
        var session = await _uow.Repository<Session>().GetByIdAsync(sessionId);

        if (session is null)
            return Result.Failure<bool>("السكشن غير موجود");

        if (!session.IsActive)
            return Result.Failure<bool>("السكشن غير نشط");

        var isAvailable = bookingType == BookingType.New ? !session.IsNewFull : !session.IsFollowUpFull;

        return Result.Success(isAvailable);
    }

    // __________ Get By Id __________ //
    public async Task<Result<SessionDto>> GetByIdAsync(int sessionId)
    {
        var session = await _uow.Repository<Session>().GetByIdAsync(sessionId);

        if (session is null)
            return Result.Failure<SessionDto>("السكشن غير موجود");

        return Result.Success(_mapper.Map<SessionDto>(session));
    }

    // __________ Private Helpers __________ //
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
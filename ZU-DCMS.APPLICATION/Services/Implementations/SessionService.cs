using AutoMapper;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
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
    private readonly IRawSqlExecutor _sql;
    private readonly IAppLogger<SessionService> _logger;

    public SessionService(
        IUnitOfWork uow,
        ICacheService cache,
        IMapper mapper,
        IRawSqlExecutor sql,
        IAppLogger<SessionService> logger)
    {
        _uow = uow;
        _cache = cache;
        _mapper = mapper;
        _sql = sql;
        _logger = logger;
    }

    // __________ Generate Sessions __________ //
    public async Task<Result<List<SessionDto>>> GenerateSessionsAsync(DateTime date)
    {
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
        await _uow.SaveChangesAsync();

        _logger.LogInfo("Sessions created successfully for {Date} Count: {Count}", date, sessions.Count);
        
        return Result.Success(_mapper.Map<List<SessionDto>>(sessions));
    }

    // __________ Get Available Slots __________ //
    public async Task<Result<List<AvailableSlotDto>>> GetAvailableSlotsAsync(BookingType bookingType)
    {
        // __ We will look ahead up to MaxLookAheadDays to find available slots __ //
        var slots = new List<AvailableSlotDto>();
        var currentDate = DateTime.Today;
        var daysChecked = 0;

        _logger.LogInfo("Fetching available slots for {BookingType}", bookingType);

        // __ Loop through upcoming days until we find 4 available slots or reach the look-ahead limit __ //
        while (slots.Count < 4 && daysChecked < MaxLookAheadDays)
        {
            // __ Skip Fridays __ //
            if (currentDate.DayOfWeek == DayOfWeek.Friday)
            {
                currentDate = currentDate.AddDays(1);
                daysChecked++;
                continue;
            }

            // __ Get sessions for the current date __ //
            var sessions = await _uow.Repository<Session>().GetListAsync(
                s => s.Date.Date == currentDate.Date &&
                     s.IsActive &&
                    !s.IsDeleted);

            // __ If no sessions exist, generate them __ //
            if (!sessions.Any())
            {
                _logger.LogInfo("No sessions found, generating for {Date}", currentDate);

                await GenerateSessionsAsync(currentDate);

                sessions = await _uow.Repository<Session>().GetListAsync(
                    s => s.Date.Date == currentDate.Date &&
                         s.IsActive &&
                        !s.IsDeleted);
            }

            // __ Check each session for availability based on booking type __ //
            foreach (var session in sessions.OrderBy(s => s.StartTime))
            {
                var isAvailable = bookingType == BookingType.New ? !session.IsNewFull : !session.IsFollowUpFull;

                // __ If this session is not available for the booking type, skip it __ //
                if (!isAvailable) continue;

                // __ Add available slot info to the list __ //
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

                // __ If we have found 4 available slots, we can stop looking further __ //
                if (slots.Count >= 4) break;
            }

            // __ Move to the next day __ //
            currentDate = currentDate.AddDays(1);
            daysChecked++;
        }

        _logger.LogInfo("Found {Count} available slots for {BookingType}", slots.Count, bookingType);
        
        return Result.Success(slots);
    }

    // __________ Get Today Sessions __________ //
    public async Task<Result<List<SessionDto>>> GetTodaySessionsAsync()
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

            var generateResult = await GenerateSessionsAsync(today);

            if (generateResult.IsFailure)
            {
                _logger.LogError("Failed to generate today's sessions.");
               
                return Result.Failure<List<SessionDto>>(generateResult.Error);
            }

            // __ Fetch the newly generated sessions __ //
            sessions = await _uow.Repository<Session>().GetListAsync(
                s => s.Date.Date == today &&
                     s.IsActive &&
                    !s.IsDeleted);
        }

        _logger.LogInfo("Found {Count} sessions for today", sessions.Count);

        return Result.Success(_mapper.Map<List<SessionDto>>(sessions.OrderBy(s => s.StartTime)));
    }

    // __________ Is Session Available __________ //

    // __ This method checks if a specific session is available for booking based on the booking type __ //
    public async Task<Result<bool>> IsSessionAvailableAsync(int sessionId, BookingType bookingType)
    {
        _logger.LogInfo("Checking availability for SessionId: {SessionId}, BookingType: {BookingType}", sessionId, bookingType);

        // __ Fetch the session by Id __ //
        var session = await _uow.Repository<Session>().GetByIdAsync(sessionId);

        if (session is null)
        { 
            _logger.LogWarning("Session not found for SessionId: {SessionId}", sessionId);

            return Result.Failure<bool>("السكشن غير موجود");
        }

        if (!session.IsActive)
        {
            _logger.LogWarning("Session is not active for SessionId: {SessionId}", sessionId);
            
            return Result.Failure<bool>("السكشن غير نشط");
        }

        // __ Determine availability based on booking type __ //
        var isAvailable = bookingType == BookingType.New ? !session.IsNewFull : !session.IsFollowUpFull;

        _logger.LogInfo("Session availability for SessionId: {SessionId}, BookingType: {BookingType} is {IsAvailable}", sessionId, bookingType, isAvailable);

        return Result.Success(isAvailable);
    }

    // __________ Get By Id __________ //
    public async Task<Result<SessionDto>> GetByIdAsync(int sessionId)
    {
        _logger.LogInfo("Fetching session by Id: {SessionId}", sessionId);

        // __ Fetch the session by Id __ //
        var session = await _uow.Repository<Session>().GetByIdAsync(sessionId);

        if (session is null)
        {
            _logger.LogWarning("Session not found for Id: {SessionId}", sessionId);

            return Result.Failure<SessionDto>("السكشن غير موجود");
        }

        _logger.LogInfo("Session found for Id: {SessionId}", sessionId);

        return Result.Success(_mapper.Map<SessionDto>(session));
    }

    /*
     * This method is used internally to fetch the session entity by Id without mapping it to a DTO,
     * mainly for use in booking operations where we need to update the session counts
     */
    public async Task<Result<Session>> GetEntityByIdAsync(int sessionId)
    {
        // __ Fetch the session entity by Id __ //
        var session = await _uow.Repository<Session>().GetByIdAsync(sessionId);

        return session is null ? Result.Failure<Session>("السكشن غير موجود") : Result.Success(session);
    }

    // __________ Reserve Slot __________ //
    // __ This method is called when a booking is made to reserve a slot in the session by incrementing the appropriate count __ //
    public async Task<Result> ReserveSlotAsync(int sessionId, BookingType type)
    {
        _logger.LogInfo("Reserving slot for SessionId: {SessionId}, Type: {Type}", sessionId, type);

        // __ We will use a raw SQL update to increment the count atomically and ensure we do not exceed the max limits __ //

        // __ Determine which column to update based on booking type __ //
        var column = type == BookingType.New ? nameof(Session.CurrentNewCount) : nameof(Session.CurrentFollowUpCount);

        // __ Determine the max column for the check __ //
        var maxColumn = type == BookingType.New ? nameof(Session.MaxNewPatients) : nameof(Session.MaxFollowUpPatients);

        // __ Construct the SQL query to increment the count if it does not exceed the max and the session is active and not deleted __ //
        var sql = $@"
        UPDATE Sessions
        SET {column} = {column} + 1
        WHERE Id = @id
        AND {column} < {maxColumn}
        AND IsActive = 1
        AND IsDeleted = 0";

        // __ Execute the SQL query and check how many rows were affected __ //
        var affected = await _sql.ExecuteAsync(sql, new { id = sessionId });

        // __ If no rows were affected, it means the session was either full, inactive, deleted, or does not exist __ //
        if (affected == 0)
        {
            _logger.LogWarning("Failed to reserve slot for SessionId: {SessionId}, BookingType: {Type}", sessionId, type);

            return Result.Failure("السكشن غير متاح أو تم حجزه للتو، حاول اختيار موعد آخر");
        }

        // __ If we reach here, it means the slot was successfully reserved __ //
        return Result.Success();
    }

    // __________ Release Slot __________ //
    // __ This method is called when a booking is cancelled to release a slot in the session by decrementing the appropriate count __ //
    public async Task<Result> ReleaseSlotAsync(int sessionId, BookingType type)
    {
        _logger.LogInfo("Releasing slot for SessionId: {SessionId}, Type: {Type}", sessionId, type);

        // __ We will use a raw SQL update to decrement the count atomically and ensure we do not go below zero __ //

        // __ Determine which column to update based on booking type __ //
        var column = type == BookingType.New ? nameof(Session.CurrentNewCount) : nameof(Session.CurrentFollowUpCount);

        // __ Construct the SQL query to decrement the count if it is greater than zero and the session is active and not deleted __ //
        var sql = $@"
        UPDATE Sessions
        SET {column} = {column} - 1
        WHERE Id = @id
        AND {column} > 0
        AND IsActive = 1
        AND IsDeleted = 0";

        // __ Execute the SQL query and check how many rows were affected __ //
        var affected = await _sql.ExecuteAsync(sql, new { id = sessionId });

        // __ If no rows were affected, it means the session was either inactive, deleted, does not exist, or the count was already at zero __ //
        if (affected == 0)
        {
            _logger.LogWarning("Failed to release slot for SessionId: {SessionId}, Type: {Type}", sessionId, type);

            return Result.Failure("فشل تحرير مكان الحجز");
        }

        // __ If we reach here, it means the slot was successfully released __ //
        return Result.Success();
    }

    // __________ Find Session by Date and Time Slot __________ //
    public async Task<Result<Session>> FindSessionAsync(DateTime date, string timeSlot)
    {
        // __ Validate time slot format __ //
        if (!TimeSpan.TryParse(timeSlot, out var time))
            return Result.Failure<Session>("صيغة الوقت غير صحيحة");

        // __ Fetch the session that matches the date and time slot __ //
        var session = await _uow.Repository<Session>().GetFirstOrDefaultAsync
            (s => s.Date.Date == date.Date &&
                  s.StartTime == time &&
                  s.IsActive &&
                 !s.IsDeleted
            );

        // __ If session is not found, return failure __ //
        return session == null ? Result.Failure<Session>("السكشن غير موجود") : Result.Success(session);
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
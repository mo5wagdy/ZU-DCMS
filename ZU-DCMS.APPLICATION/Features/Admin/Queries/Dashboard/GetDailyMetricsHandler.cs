using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Admin;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;
using ZiggyCreatures.Caching.Fusion;
using ZU_DCMS.APPLICATION.Common.Cache;

namespace ZU_DCMS.APPLICATION.Features.Admin.Queries.Dashboard
{
    // __ Handler for aggregating daily clinical metrics __ //
    public class GetDailyMetricsHandler : IRequestHandler<GetDailyMetricsQuery, Result<DailyMetricsDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IFusionCache _cache;

        public GetDailyMetricsHandler(IUnitOfWork uow, IFusionCache cache)
        {
            _uow = uow;
            _cache = cache;
        }

        public async Task<Result<DailyMetricsDto>> Handle(GetDailyMetricsQuery request, CancellationToken cancellationToken)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var metrics = await _cache.GetOrSetAsync(
                CacheKeys.DailyMetrics,
                async _ =>
                {
                    // __ Count patients registered today __ //
                    var todayNewPatients = await _uow.Repository<Patient>().CountAsync(
                        p => p.CreatedAt >= today && p.CreatedAt < tomorrow && !p.IsDeleted);

                    // __ Count total bookings for today's sessions __ //
                    var todayBookings = await _uow.Repository<Booking>().CountAsync(
                        b => b.Session.Date == today && !b.IsDeleted);

                    // __ Count new patient bookings today __ //
                    var todayNewBookings = await _uow.Repository<Booking>().CountAsync(
                        b => b.Session.Date == today && b.BookingType == BookingType.New && !b.IsDeleted);

                    // __ Count follow-up bookings today __ //
                    var todayFollowUpBookings = await _uow.Repository<Booking>().CountAsync(
                        b => b.Session.Date == today && b.BookingType == BookingType.FollowUp && !b.IsDeleted);

                    // __ Count pending (unattended) bookings today __ //
                    var pendingBookings = await _uow.Repository<Booking>().CountAsync(
                        b => b.Session.Date == today &&
                             (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed) &&
                             !b.IsDeleted);

                    // __ Count bookings cancelled today __ //
                    var cancelledBookings = await _uow.Repository<Booking>().CountAsync(
                        b => b.Session.Date == today && b.Status == BookingStatus.Cancelled && !b.IsDeleted);

                    // __ Count cases currently in progress (ongoing treatment) __ //
                    var inProgressCases = await _uow.Repository<CaseAssignment>().CountAsync(
                        c => c.Status == CaseStatus.InProgress);

                    // __ Count cases completed today __ //
                    var completedToday = await _uow.Repository<CaseAssignment>().CountAsync(
                        c => c.Status == CaseStatus.Completed &&
                             c.CompletedAt.HasValue &&
                             c.CompletedAt.Value >= today &&
                             c.CompletedAt.Value < tomorrow);

                    // __ Count active sessions scheduled for today __ //
                    var activeSessions = await _uow.Repository<Session>().CountAsync(
                        s => s.Date == today && s.IsActive && !s.IsDeleted);

                    // __ Count all active students in the system __ //
                    var activeStudents = await _uow.Repository<Student>().CountAsync(
                        s => s.IsActive && !s.IsDeleted);

                    return new DailyMetricsDto
                    {
                        TodayNewPatientsCount = todayNewPatients,
                        TodayBookingsCount = todayBookings,
                        TodayNewBookingsCount = todayNewBookings,
                        TodayFollowUpBookingsCount = todayFollowUpBookings,
                        PendingBookingsCount = pendingBookings,
                        CancelledBookingsCount = cancelledBookings,
                        InProgressCasesCount = inProgressCases,
                        CompletedCasesCount = completedToday,
                        ActiveSessionsCount = activeSessions,
                        TotalActiveStudents = activeStudents,
                    };
                },
                CacheDuration.Short,
                cancellationToken
            );

            return Result.Success(metrics!);
        }
    }
}

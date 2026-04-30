using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Admin;

namespace ZU_DCMS.APPLICATION.Features.Admin.Queries.Dashboard
{
    // __ Query to retrieve daily clinical metrics for management roles __ //
    public record GetDailyMetricsQuery : IRequest<Result<DailyMetricsDto>>;
}

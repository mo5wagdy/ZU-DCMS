using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Admin;

namespace ZU_DCMS.APPLICATION.Features.Admin.Queries.GetAllConfigs
{
    public record GetAllConfigsQuery : IRequest<Result<List<SystemConfigDto>>>;
}

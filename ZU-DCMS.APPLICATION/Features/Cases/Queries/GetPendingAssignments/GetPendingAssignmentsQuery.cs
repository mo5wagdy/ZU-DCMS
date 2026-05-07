using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;

using ZiggyCreatures.Caching.Fusion;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Cache;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.DTOs.Case;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Cases.Queries.GetPendingAssignments
{
    public class GetPendingAssignmentsQuery : IRequest<Result<List<CaseAssignmentDto>>> { }

}

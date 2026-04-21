using MediatR;
using ZiggyCreatures.Caching.Fusion;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Cache;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.Contracts.Auth;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.DTOs.Case;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Cases.Commands.ReviewCase
{
    public class ReviewCaseHandler : IRequestHandler<ReviewCaseCommand, Result>
    {
        private readonly IUnitOfWork _uow;
        private readonly IFusionCache _cache;
        private readonly IRawSqlExecutor _sql;
        private readonly IIdentityService _identity;
        private readonly IAppLogger<Result> _logger;

        public ReviewCaseHandler
        (
            IUnitOfWork uow,
            IFusionCache cache,
            IRawSqlExecutor sql,
            IIdentityService identity,
            IAppLogger<Result> logger
        )
        {
            _uow = uow;
            _cache = cache;
            _sql = sql;
            _identity = identity;
            _logger = logger;
        }

        public async Task<Result> Handle(ReviewCaseCommand command, CancellationToken cancellationToken)
        {
            var dto = command.dto;

            // 1. Validate TA role
            var roles = await _identity.GetRolesAsync(command.TeachingAssistantId);

            if (!roles.Contains("TeachingAssistant"))
                return Result.Failure("المعيد فقط هو المسؤول عن مراجعة الحاله");

            // 2. Fetch case
            var caseAssignment = await _uow.Repository<CaseAssignment>().GetByIdAsync(dto.CaseAssignmentId);

            if (caseAssignment is null) 
                return Result.Failure("الحاله غير موجوده");

            // 3. Ensure case is pending review
            if (caseAssignment.Status != CaseStatus.PendingReview)
                return Result.Failure("الحاله لم تكتمل بعد");

            // 4. Create review entity
            var review = new CaseReview
            {
                CaseAssignmentId = dto.CaseAssignmentId,
                TeachingAssistantId = command.TeachingAssistantId,
                Status = dto.IsApproved ? ReviewStatus.Approved : ReviewStatus.Rejected,
                Notes = dto.Notes,
                ReviewedAt = DateTime.UtcNow
            };

            await _uow.Repository<CaseReview>().AddAsync(review);

            // 5. Update case status
            if (dto.IsApproved)
            {
                caseAssignment.Status = CaseStatus.Approved;

                _logger.LogInfo("Incrementing requirement for student {StudentId}", caseAssignment.StudentId);

                // __ Atomically Increamenting student requirement for the assigned case __ //
                var sql = @"
                UPDATE TermRequirements
                SET CompletedCount = CompletedCount + 1
                WHERE StudentId = @StudentId
                AND ClinicId = @ClinicId
                AND TermId = @TermId";

                var rows = await _sql.ExecuteAsync(sql, new { caseAssignment.StudentId, caseAssignment.ClinicId, caseAssignment.TermId });

                // __ If no changes rollback and return failure __ //
                if (rows == 0)
                {
                    _logger.LogWarning("No Rows Affected");

                    await _uow.RollbackTransactionAsync();

                    return Result.Failure<CaseSessionDto>("حدث خطأ أثناء إتمام الحاله");
                }
            }
            else
            {
                caseAssignment.Status = CaseStatus.InProgress;
            }

            _uow.Repository<CaseAssignment>().Update(caseAssignment);

            // 6. Save all changes in one transaction
            await _uow.SaveChangesAsync(cancellationToken: cancellationToken);

            await _cache.RemoveAsync(CacheKeys.StudentProgress(caseAssignment.StudentId, caseAssignment.TermId));
            await _cache.RemoveAsync(CacheKeys.StudentRequirements(caseAssignment.StudentId, caseAssignment.TermId));

            return Result.Success();
        }
    }
}

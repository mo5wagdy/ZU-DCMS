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
            _logger.LogInfo("Creating a Review");

            var dto = command.dto;

            // Load Teaching Assistant
            var ta = await _uow.Repository<TeachingAssistant>().GetFirstOrDefaultAsync
                (
                    t => t.ApplicationUserId == command.TeachingAssistantId
                );

            if (ta is null)
            {
                _logger.LogWarning("TA Not Found");

                return Result.Failure("المعيد غير موجود");
            }

            // __ Validate TA role __ //
            var roles = await _identity.GetRolesAsync(command.TeachingAssistantId);

            if (!roles.Contains("TeachingAssistant"))
            {
                _logger.LogWarning("Cannot Review a Case By Anyone Except The TA");

                return Result.Failure("المعيد فقط هو المسؤول عن مراجعة الحاله");
            }

            // __ Fetch case __ //
            var caseAssignment = await _uow.Repository<CaseAssignment>().GetByIdAsync(dto.CaseAssignmentId);

            if (caseAssignment is null) 
                return Result.Failure("الحاله غير موجوده");

            // __ Ensure case is pending review __ //
            if (caseAssignment.Status != CaseStatus.PendingReview)
                return Result.Failure("الحاله لم تكتمل بعد");

            await _uow.BeginTransactionAsync();

            try
            {

                // __ Create review entity __ //
                var review = new CaseReview
                {
                    CaseAssignmentId = dto.CaseAssignmentId,
                    TeachingAssistantId = ta.Id,
                    Status = dto.IsApproved ? ReviewStatus.Approved : ReviewStatus.Rejected,
                    Notes = dto.Notes,
                    ReviewedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await _uow.Repository<CaseReview>().AddAsync(review);

                // __ Update case status __ //
                if (dto.IsApproved)
                {
                    caseAssignment.Status = CaseStatus.Approved;
                    caseAssignment.CompletedAt = DateTime.UtcNow;

                    _uow.Repository<CaseAssignment>().Update(caseAssignment);

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

                    _uow.Repository<CaseAssignment>().Update(caseAssignment);

                    _logger.LogInfo("Student's Case Not Approved And He Will Start Again");
                }

                // __ Save all changes in one transaction __ //
                await _uow.CommitTransactionAsync(command.TeachingAssistantId);

                // __ Invalidating The Cache __ //
                await _cache.RemoveAsync(CacheKeys.StudentProgress(caseAssignment.StudentId, caseAssignment.TermId));
                await _cache.RemoveAsync(CacheKeys.StudentRequirements(caseAssignment.StudentId, caseAssignment.TermId));

                return Result.Success();
            }

            // __ If Failed
            catch (Exception ex)
            {
                _logger.LogError("Error reviewing case {CaseId}", ex, dto.CaseAssignmentId);
                
                await _uow.RollbackTransactionAsync();
                
                return Result.Failure("حدث خطأ أثناء المراجعة");
            }
        }
    }
}

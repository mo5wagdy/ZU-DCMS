using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;
using ZiggyCreatures.Caching.Fusion;
using ZU_DCMS.APPLICATION.Common.Cache;

namespace ZU_DCMS.APPLICATION.Features.Cases.Commands.ReviewAssignment
{
    public class ReviewAssignmentHandler : IRequestHandler<ReviewAssignmentCommand, Result<string>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IFusionCache _cache;
        private readonly IAppLogger<ReviewAssignmentHandler> _logger;

        public ReviewAssignmentHandler(IUnitOfWork uow, IFusionCache cache, IAppLogger<ReviewAssignmentHandler> logger)
        {
            _uow = uow;
            _cache = cache;
            _logger = logger;
        }

        public async Task<Result<string>> Handle(ReviewAssignmentCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;
            
            // __ 1. Get TA data __ //
            var ta = await _uow.Repository<TeachingAssistant>().GetFirstOrDefaultAsync(t => t.ApplicationUserId == request.TaUserId);
            if (ta == null)
            {
                return Result.Failure<string>("Teaching assistant not found.");
            }

            // __ 2. Get the preliminary assignment with Diagnosis and Booking __ //
            var assignment = await _uow.Repository<CaseAssignment>().GetFirstOrDefaultAsync(
                a => a.Id == dto.CaseAssignmentId,
                false, // Need tracking
                a => a.DiagnosisRecord,
                a => a.DiagnosisRecord.Booking
            );

            if (assignment == null)
            {
                return Result.Failure<string>("Assignment not found.");
            }

            if (assignment.Status != CaseStatus.PendingAssignmentApproval)
            {
                return Result.Failure<string>("Cannot review this case, it is not in (Pending Review) status.");
            }

            // __ 3. Update review data __ //
            assignment.ReviewedByTAId = ta.Id;
            assignment.AssignmentReviewedAt = DateTime.UtcNow;
            assignment.ReviewNotes = dto.Notes?.Trim();

            // __ 4. Take action based on TA's decision __ //
            switch (dto.Action.ToUpper())
            {
                case "APPROVE":
                    assignment.Status = CaseStatus.InProgress; // __ Student starts working on it __ //
                    break;
                    
                case "ESCALATE":
                    assignment.Status = CaseStatus.EscalatedToSpecialist; // __ Escalate to specialist __ //
                    assignment.DiagnosisRecord.IsAssigned = false; // __ Free the diagnosis for reassignment in specialized clinic if needed __ //
                    break;
                    
                case "TRANSFER":
                    assignment.Status = CaseStatus.TransferredToIntern; // __ Return to intern clinic __ //
                    assignment.DiagnosisRecord.IsAssigned = false;
                    break;
                    
                default:
                    return Result.Failure<string>("Unknown action.");
            }

            _uow.Repository<CaseAssignment>().Update(assignment);
            _uow.Repository<DiagnosisRecord>().Update(assignment.DiagnosisRecord);
            
            await _uow.SaveChangesAsync(cancellationToken: cancellationToken);

            // __ 5. Invalidate cache for the concerned student and clinic __ //
            await _cache.RemoveAsync(CacheKeys.StudentCases(assignment.StudentId));
            await _cache.RemoveAsync(CacheKeys.AvailableStudents(assignment.ClinicId));

            _logger.LogInfo($"TA {ta.Id} reviewed Assignment {assignment.Id} with action {dto.Action}");

            return Result.Success("Assignment reviewed successfully.");
        }
    }
}

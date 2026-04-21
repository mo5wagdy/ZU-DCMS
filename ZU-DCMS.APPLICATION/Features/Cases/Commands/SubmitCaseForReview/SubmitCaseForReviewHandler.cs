using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Cases.Commands.SubmitCaseForReview
{
    public class SubmitCaseForReviewHandler : IRequestHandler<SubmitCaseForReviewCommand, Result>
    {
        private readonly IUnitOfWork _uow;

        public SubmitCaseForReviewHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Result> Handle(SubmitCaseForReviewCommand command, CancellationToken cancellationToken)
        {
            // 1. Fetch case assignment
            var caseAssignment = await _uow.Repository<CaseAssignment>().GetByIdAsync(command.CaseAssignmentId);

            // 2. Validate existence
            if (caseAssignment is null)
                return Result.Failure("الحاله غير موجوده");

            // 3. Ensure this student owns the case
            if (caseAssignment.StudentId != command.StudentId)
                return Result.Failure("غير مصرح");

            // 4. Ensure case is in progress
            if (caseAssignment.Status != CaseStatus.InProgress)
                return Result.Failure("الحاله ليست قيد العمل");

            // 5. Move case to PendingReview
            caseAssignment.Status = CaseStatus.PendingReview;

            // 6. Save changes
            _uow.Repository<CaseAssignment>().Update(caseAssignment);
            await _uow.SaveChangesAsync(cancellationToken: cancellationToken);

            return Result.Success();
        }
    }
}
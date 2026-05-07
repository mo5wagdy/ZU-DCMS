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
            // __ Fetch case assignment __ //
            var caseAssignment = await _uow.Repository<CaseAssignment>().GetByIdAsync(command.CaseAssignmentId);

            // __ Validate existence __ //
            if (caseAssignment is null)
                return Result.Failure("Case not found");

            // __ Ensure this student owns the case __ //
            if (caseAssignment.StudentId != command.StudentId)
                return Result.Failure("You do not have permission");

            // __ Ensure case is in progress __ //
            if (caseAssignment.Status != CaseStatus.InProgress)
                return Result.Failure("Case is not in progress");

            // __ Move case to PendingReview __ //
            caseAssignment.Status = CaseStatus.PendingReview;

            // __ Save changes __ //
            _uow.Repository<CaseAssignment>().Update(caseAssignment);
            
            await _uow.SaveChangesAsync(cancellationToken: cancellationToken);

            return Result.Success();
        }
    }
}
using Microsoft.Extensions.Logging;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Admin.Commands.TransferRequirements
{
    public class TransferRequirementsHandler
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<TransferRequirementsHandler> _logger;

        public TransferRequirementsHandler(IUnitOfWork uow, ILogger<TransferRequirementsHandler> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<Result> Handle(TransferRequirementsCommand command)
        {
            var fromRequirements = await _uow.Repository<TermRequirement>()
            .GetListAsync(r =>
                r.StudentId == command.StudentId &&
                r.TermId == command.FromTermId &&
                !r.IsSatisfied);

            if (!fromRequirements.Any())
                return Result.Failure("لا توجد متطلبات غير مكتملة للترحيل");

            var toTerm = await _uow.Repository<Term>().GetByIdAsync(command.ToTermId);

            if (toTerm == null)
                return Result.Failure("الترم المحول إليه غير موجود");

            await _uow.BeginTransactionAsync();
            
            try
            {
                foreach (var req in fromRequirements)
                {
                    // Check if requirement already exists in target term
                    var exists = await _uow.Repository<TermRequirement>()
                        .ExistsAsync(r =>
                            r.StudentId == command.StudentId &&
                            r.TermId == command.ToTermId &&
                            r.ClinicId == req.ClinicId);

                    if (exists) continue;

                    // Transfer remaining count to new term
                    var remaining = req.RequiredCount
                        - req.CompletedCount
                        - req.TransferredCount;

                    var transferred = new TermRequirement
                    {
                        StudentId = command.StudentId,
                        TermId = command.ToTermId,
                        ClinicId = req.ClinicId,
                        RequiredCount = remaining,
                        TransferredCount = remaining,
                        CompletedCount = 0,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _uow.Repository<TermRequirement>()
                        .AddAsync(transferred);
                }

                await _uow.CommitTransactionAsync(command.AdminId);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transferring requirements for student {StudentId}", command.StudentId);

                await _uow.RollbackTransactionAsync();
               
                return Result.Failure("حدث خطأ أثناء ترحيل المتطلبات");
            }
        }
    }
}

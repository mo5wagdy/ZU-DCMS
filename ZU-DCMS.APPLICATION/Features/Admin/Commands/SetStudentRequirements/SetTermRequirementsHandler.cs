using MediatR;
using Microsoft.Extensions.Logging;
using ZiggyCreatures.Caching.Fusion;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Cache;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Admin.Commands.SetStudentRequirements
{
    public class SetTermRequirementsHandler : IRequestHandler<SetTermRequirementsCommand, Result>
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<SetTermRequirementsHandler> _logger;

        public SetTermRequirementsHandler
        (
            IUnitOfWork uow,
            ILogger<SetTermRequirementsHandler> logger
        )
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<Result> Handle(SetTermRequirementsCommand command, CancellationToken cancellationToken)
        {
            // __ Verify term exists __ //
            var term = await _uow.Repository<Term>().GetByIdAsync(command.TermId);
            if (term is null)
                return Result.Failure("Term not found");

            // __ Verify all clinics exist __ //
            var clinicIds = command.Requirements.Select(r => r.ClinicId).ToList();
            var clinics = await _uow.Repository<Clinic>().GetListAsync(c => clinicIds.Contains(c.Id) && c.IsActive);

            if (clinics.Count != clinicIds.Distinct().Count())
                return Result.Failure("Some clinics are not found or inactive");

            await _uow.BeginTransactionAsync();

            try
            {
                // __ Load existing GLOBAL requirements for this Year + Term __ //
                var existing = await _uow.Repository<TermRequirement>().GetListAsync
                    (
                        r =>
                        r.StudentId == null &&
                        r.AcademicYear == command.AcademicYear &&
                        r.TermId == command.TermId
                    );

                // __ Soft delete existing __ //
                foreach (var req in existing)
                    _uow.Repository<TermRequirement>().SoftDelete(req);

                // __ Create new GLOBAL requirements (One per clinic) __ //
                var newRequirements = command.Requirements.Select(r =>
                    new TermRequirement
                    {
                        StudentId = null, // Global Template
                        AcademicYear = command.AcademicYear,
                        TermId = command.TermId,
                        ClinicId = r.ClinicId,
                        RequiredCount = r.RequiredCount,
                        CompletedCount = 0,
                        CreatedAt = DateTime.UtcNow
                    });

                await _uow.Repository<TermRequirement>().AddRangeAsync(newRequirements);

                await _uow.CommitTransactionAsync(command.AdminId);

                // __ NOTE: Cache invalidation might need a broader approach here __ //
                // For now, we rely on individual progress queries to check for global templates if no specific req exists.
                
                return Result.Success();
            }
            catch (Exception ex)
            {
                await _uow.RollbackTransactionAsync();
                _logger.LogError(ex, "Error setting term requirements for year {Year}", command.AcademicYear);
                return Result.Failure("An error occurred while updating term requirements");
            }
        }
    }
}

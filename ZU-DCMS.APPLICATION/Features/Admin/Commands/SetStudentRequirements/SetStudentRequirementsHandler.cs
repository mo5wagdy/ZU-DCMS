using Microsoft.Extensions.Logging;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Admin.Commands.SetStudentRequirements
{
    public class SetStudentRequirementsHandler
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<SetStudentRequirementsHandler> _logger;

        public SetStudentRequirementsHandler(IUnitOfWork uow, ILogger<SetStudentRequirementsHandler> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<Result> Handle(SetStudentRequirementsCommand command)
        {
            // Verify student exists
            var student = await _uow.Repository<Student>().GetByIdAsync(command.StudentId);

            if (student is null)
                return Result.Failure("الطالب غير موجود");

            // Verify term exists
            var term = await _uow.Repository<Term>().GetByIdAsync(command.TermId);

            if (term is null)
                return Result.Failure("الترم غير موجود");

            // Verify all clinics exist
            var clinicIds = command.Requirements.Select(r => r.ClinicId).ToList();
            
            var clinics = await _uow.Repository<Clinic>().GetListAsync(c => clinicIds.Contains(c.Id) && c.IsActive);

            if (clinics.Count != clinicIds.Distinct().Count())
                return Result.Failure("بعض العيادات غير موجودة أو غير نشطة");

            await _uow.BeginTransactionAsync();

            try
            {
                // Load existing requirements for this student + term
                var existing = await _uow.Repository<TermRequirement>().GetListAsync
                    (
                        r =>
                        r.StudentId == command.StudentId &&
                        r.TermId == command.TermId
                    );

                // Soft delete existing requirements
                foreach (var req in existing)
                    _uow.Repository<TermRequirement>().SoftDelete(req);

                // Create new requirements
                var newRequirements = command.Requirements.Select(r =>
                    new TermRequirement
                    {
                        StudentId = command.StudentId,
                        TermId = command.TermId,
                        ClinicId = r.ClinicId,
                        RequiredCount = r.RequiredCount,
                        CompletedCount = 0,
                        CreatedAt = DateTime.UtcNow
                    });

                await _uow.Repository<TermRequirement>().AddRangeAsync(newRequirements);

                await _uow.CommitTransactionAsync(command.AdminId);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting requirements for student {StudentId}",
                    command.StudentId);

                await _uow.RollbackTransactionAsync();
                return Result.Failure("حدث خطأ أثناء تحديث المتطلبات");
            }
        }
    }
}

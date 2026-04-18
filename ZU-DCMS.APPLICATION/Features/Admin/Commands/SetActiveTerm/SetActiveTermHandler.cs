using Microsoft.Extensions.Logging;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Admin.Commands.SetActiveTerm
{
    public class SetActiveTermHandler
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<SetActiveTermHandler> _logger;

        public SetActiveTermHandler(IUnitOfWork uow, ILogger<SetActiveTermHandler> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<Result> Handle(SetActiveTermCommand command)
        {
            var term = await _uow.Repository<Term>().GetByIdAsync(command.TermId);

            if (term is null)
                return Result.Failure("الترم غير موجود");

            await _uow.BeginTransactionAsync();

            try
            {
                // Deactivate current active term
                var currentActive = await _uow.Repository<Term>().GetFirstOrDefaultAsync(t => t.IsActive);

                if (currentActive != null)
                {
                    currentActive.IsActive = false;

                    currentActive.UpdatedAt = DateTime.UtcNow;

                    _uow.Repository<Term>().Update(currentActive);
                }

                // Activate new term
                term.IsActive = true;

                term.UpdatedAt = DateTime.UtcNow;

                _uow.Repository<Term>().Update(term);

                // Update all active students to new term
                var students = await _uow.Repository<Student>().GetListAsync(s => s.IsActive);

                foreach (var student in students)
                {
                    student.ActiveTermId = command.TermId;

                    student.UpdatedAt = DateTime.UtcNow;

                    _uow.Repository<Student>().Update(student);
                }

                await _uow.CommitTransactionAsync(command.AdminId);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting active term {TermId}", command.TermId);

                await _uow.RollbackTransactionAsync();
               
                return Result.Failure("حدث خطأ أثناء تفعيل الترم");
            }
        }
    }
}

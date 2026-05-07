using MediatR;
using Microsoft.Extensions.Logging;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Admin.Commands.SetActiveTerm
{
    public class SetActiveTermHandler : IRequestHandler<SetActiveTermCommand, Result>
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<SetActiveTermHandler> _logger;

        public SetActiveTermHandler(IUnitOfWork uow, ILogger<SetActiveTermHandler> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<Result> Handle(SetActiveTermCommand command, CancellationToken cancellationToken)
        {
            var term = await _uow.Repository<Term>().GetByIdAsync(command.TermId);

            if (term is null)
                return Result.Failure("Term not found");

            await _uow.BeginTransactionAsync();

            try
            {
                // __ Deactivate current active term __ //
                var currentActive = await _uow.Repository<Term>().GetFirstOrDefaultAsync(t => t.IsActive);

                if (currentActive != null)
                {
                    currentActive.IsActive = false;

                    currentActive.UpdatedAt = DateTime.UtcNow;

                    _uow.Repository<Term>().Update(currentActive);
                }

                // __ Activate new term __ //
                term.IsActive = true;

                term.UpdatedAt = DateTime.UtcNow;

                _uow.Repository<Term>().Update(term);

                // __ Update all active students to new term __ //
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
               
                return Result.Failure("An error occurred while activating the term");
            }
        }
    }
}

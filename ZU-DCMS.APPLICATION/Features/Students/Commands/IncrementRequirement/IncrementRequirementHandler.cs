using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;

namespace ZU_DCMS.APPLICATION.Features.Student.Commands.IncrementRequirement
{
    public class IncrementRequirementHandler
    {
        private readonly IRawSqlExecutor _sql;
        private readonly IAppLogger<IncrementRequirementHandler> _logger;

        public IncrementRequirementHandler(IRawSqlExecutor sql, IAppLogger<IncrementRequirementHandler> logger)
        {
            _sql = sql;
            _logger = logger;
        }

        public async Task<Result> Handle(IncrementRequirementCommand command)
        {
            var studentId = command.StudentId;
            var clinicId = command.ClinicId;
            var termId = command.TermId;

            _logger.LogInfo("Incrementing requirement for student {StudentId}", studentId);

            var sql = @"
            UPDATE TermRequirements
            SET CompletedCount = CompletedCount + 1
            WHERE StudentId = @studentId
            AND ClinicId = @clinicId
            AND TermId = @termId";

            var rows = await _sql.ExecuteAsync(sql, new { studentId, clinicId, termId });

            if (rows == 0)
            {
                _logger.LogWarning("No Rows Affected");

                return Result.Failure("لم تتم زيادة المتطلبات");
            }

            _logger.LogInfo("Incremented requirement for student {StudentId}", studentId);

            return Result.Success();
        }
    }
}

using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.APPLICATION.Contracts.Engine
{
    // __ واجهة محرك التوزيع الآلي لاختيار أفضل طالب لحالة معينة __ //
    public interface IAutoAssignmentEngine
    {
        /// <summary>
        /// Selects the best student for a given diagnosis record based on clinic constraints and performance metrics.
        /// </summary>
        /// <param name="diagnosis">The diagnosis record that needs to be assigned.</param>
        /// <returns>A Result containing the selected StudentId, or a failure message if no student is found.</returns>
        Task<Result<int>> GetBestStudentForCaseAsync(DiagnosisRecord diagnosis);
    }
}

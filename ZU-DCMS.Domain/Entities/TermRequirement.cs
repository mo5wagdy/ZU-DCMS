
using System.ComponentModel.DataAnnotations.Schema;
using ZU_DCMS.Domain.Common;

namespace ZU_DCMS.Domain.Entities
{
    /// <summary>
    /// TermRequirement represents the required number of cases a student must complete 
    /// in a specific clinic during a specific academic term.
    /// 
    /// This entity is crucial for:
    /// - Tracking student progress toward graduation requirements
    /// - Determining assignment eligibility (unsatisfied requirements get priority)
    /// - Academic year and clinic-specific validation
    /// </summary>
    public class TermRequirement : BaseEntity
    {
        // _____________ Main Properties _____________ //
        /// <summary>
        /// Total number of cases required to be completed in this clinic for this term.
        /// </summary>
        public int RequiredCount { get; set; }

        /// <summary>
        /// Number of cases the student has completed in this clinic for this term.
        /// Incremented when a CaseAssignment is marked as Completed.
        /// </summary>
        public int CompletedCount { get; set; }

        /// <summary>
        /// Number of cases transferred/accepted from other clinics toward this requirement.
        /// Allows flexibility in case distribution across clinics.
        /// </summary>
        public int TransferredCount { get; set; }

        // _____________ Calculated Properties (Not Mapped to Database) _____________ //

        /// <summary>
        /// Determines if the requirement has been satisfied.
        /// Satisfied when: CompletedCount + TransferredCount >= RequiredCount
        /// 
        /// CRITICAL: This calculation is used to filter eligible students during case assignment.
        /// Students with unsatisfied requirements have priority for new cases.
        /// </summary>

        [NotMapped]
        public bool IsSatisfied =>
            CompletedCount + TransferredCount >= RequiredCount;

        /// <summary>
        /// Calculates the priority level for this requirement (1 = highest priority).
        /// 
        /// Priority Logic:
        /// - Priority 1 (0 cases completed): Highest priority - student needs this requirement
        /// - Priority 2 (1 case completed): Medium-high priority
        /// - Priority 3 (2 cases completed): Medium-low priority
        /// - Priority 4 (3+ cases completed): Lowest priority
        /// 
        /// Used by GetAvailableStudentsHandler to sort students by need.
        /// </summary>

        [NotMapped]
        public int Priority =>
            CompletedCount + TransferredCount switch
            {
                0 => 1,      // Highest priority: no progress
                1 => 2,      // Medium-high priority: 1 case done
                2 => 3,      // Medium-low priority: 2 cases done
                _ => 4       // Lowest priority: 3+ cases done
            };

        // => measure by time to finish
        // => filter by all requirements progress in all clinics

        // _____________ Foreign Keys _____________ //
        /// <summary>
        /// Reference to the Student who must fulfill this requirement.
        /// </summary>
        public int StudentId { get; set; }

        /// <summary>
        /// Reference to the academic Term when this requirement applies.
        /// </summary>
        public int TermId { get; set; }

        /// <summary>
        /// Reference to the Clinic where cases must be completed.
        /// Combined with StudentId and TermId forms the unique requirement key.
        /// </summary>
        public int ClinicId { get; set; }

        // _____________ Navigation _____________ //
        public Student Student { get; set; } = null!;
        public Term Term { get; set; } = null!;
        public Clinic Clinic { get; set; } = null!;
    }
}

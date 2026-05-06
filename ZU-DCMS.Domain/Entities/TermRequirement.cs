
using System.ComponentModel.DataAnnotations.Schema;
using ZU_DCMS.Domain.Common;

namespace ZU_DCMS.Domain.Entities
{
        // __ TermRequirement represents the required number of cases a student must complete __ //
    public class TermRequirement : BaseEntity
    {
        // _____________ Main Properties _____________ //
        // __ Total number of cases required to be completed in this clinic for this term. __ //
        public int RequiredCount { get; set; }

        // __ Number of cases the student has completed in this clinic for this term. __ //
        public int CompletedCount { get; set; }

        // __ Number of cases transferred/accepted from other clinics toward this requirement. __ //
        public int TransferredCount { get; set; }

        // _____________ Calculated Properties (Not Mapped to Database) _____________ //

        // __ Determines if the requirement has been satisfied. __ //

        [NotMapped]
        public bool IsSatisfied => CompletedCount + TransferredCount >= RequiredCount;

        // __ Calculates the priority level for this requirement (1 = highest priority). __ //

        [NotMapped]
        public int Priority =>
            CompletedCount + TransferredCount switch
            {
                0 => 1,      // Highest priority: no progress
                1 => 2,      // Medium-high priority: 1 case done
                2 => 3,      // Medium-low priority: 2 cases done
                _ => 4       // Lowest priority: 3+ cases done
            };

        // _____________ Foreign Keys _____________ //
        // __ Reference to the Student who must fulfill this requirement. __ //
        public int? StudentId { get; set; }

        // __ Reference to the academic Term when this requirement applies. __ //
        public int TermId { get; set; }

        // __ Reference to the Clinic where cases must be completed. __ //
        public int ClinicId { get; set; }

        // __ The academic year this requirement applies to. __ //
        public int AcademicYear { get; set; }

        // _____________ Navigation _____________ //
        public Student Student { get; set; } = null!;
        public Term Term { get; set; } = null!;
        public Clinic Clinic { get; set; } = null!;
    }
}

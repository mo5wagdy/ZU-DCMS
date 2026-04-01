using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Common;

namespace ZU_DCMS.Domain.Entities
{
    public class TermRequirement : BaseEntity
    {
        public int StudentId { get; set; }
        public int TermId { get; set; }
        public int ClinicId { get; set; }
        public int RequiredCount { get; set; }
        public int CompletedCount { get; set; }
        public int TransferredCount { get; set; }

        // Calculated
        public bool IsSatisfied =>
            CompletedCount + TransferredCount >= RequiredCount;

        public int Priority =>
            CompletedCount + TransferredCount switch
            {
                0 => 1,
                1 => 2,
                2 => 3,
                _ => 4
            };

        // Navigation
        public Student Student { get; set; } = null!;
        public Term Term { get; set; } = null!;
        public Clinic Clinic { get; set; } = null!;
    }
}

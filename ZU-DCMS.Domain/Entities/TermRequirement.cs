using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Common;

namespace ZU_DCMS.Domain.Entities
{
    public class TermRequirement : BaseEntity
    {
        // Properties
        public int StudentId { get; set; }
        public int TermId { get; set; }
        public int ClinicId { get; set; }
        public int RequiredCount { get; set; }
        public int CompletedCount { get; set; }
        public int TransferredCount { get; set; }

        // Calculated properties
        // These properties are not mapped to the database but calculated at runtime
        // They can be used to easily check if the requirement is satisfied and to determine the priority of completing it
        public bool IsSatisfied =>
            CompletedCount + TransferredCount >= RequiredCount;

        // Priority is determined by how many cases are completed or transferred
        // 0 completed/transferred = priority 1 (highest)
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

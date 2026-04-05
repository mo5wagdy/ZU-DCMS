using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Common;

namespace ZU_DCMS.Domain.Entities
{
    public class DiagnosisRecord : BaseEntity
    {
        // _____________ Main Properties _____________ //
        public string Complaint { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime DiagnosedAt { get; set; } = DateTime.UtcNow;
        public bool IsAssigned { get; set; } = false;

        // _____________ Foreign Keys _____________ //
        public int BookingId { get; set; }
        public int InternDoctorId { get; set; }
        public int ClinicId { get; set; }

        // _____________ Navigation _____________ //
        public InternDoctor InternDoctor { get; set; } = null!;
        public Booking Booking { get; set; } = null!;
        public Clinic Clinic { get; set; } = null!;
        public CaseAssignment? CaseAssignment { get; set; }
    }
}

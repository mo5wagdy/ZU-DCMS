using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Admin
{
    // __ DTO for setting the requirement of a clinic __ //
    public class SetRequirementDto
    {
        public int ClinicId { get; set; }
        public int RequiredCount { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Admin
{
    // __ DTO for updating an existing Term __ //
    public class UpdateTermDto
    {
        public string? Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? RequiredCasesCount { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Admin
{
    // __ DTO for creating a new Term __ //
    public class CreateTermDto
    {
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int RequiredCasesCount { get; set; }
    }
}

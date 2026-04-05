using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Admin
{
    // __ DTO for Term information __ //
    public class TermDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int RequiredCasesCount { get; set; }
    }
}

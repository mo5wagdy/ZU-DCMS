using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Common;

namespace ZU_DCMS.Domain.Entities
{
    // This entity represents a key-value pair for system configuration settings.
    // It can be used to store various settings that can be updated by an admin without changing the code.
    public class SystemConfig : BaseEntity
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string UpdatedByAdminId { get; set; } = string.Empty;
    }
}

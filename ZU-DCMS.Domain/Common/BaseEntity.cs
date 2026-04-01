using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.Domain.Common
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string? CreatedByUserId { get; set; }
        public string? UpdatedByUserId { get; set; }
    }
}

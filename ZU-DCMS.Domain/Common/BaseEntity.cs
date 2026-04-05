using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.Domain.Common
{
    /* __ BaseEntity class to be inherited by all entities in the application, 
     * providing common properties for tracking creation, updates, and soft deletion.
     * This class can be extended in the future to include additional common properties or methods as needed __ */
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

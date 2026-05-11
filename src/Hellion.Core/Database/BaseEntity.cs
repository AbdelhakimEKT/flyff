using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hellion.Core.Database
{
    /// <summary>
    /// Base type for entities tracked with creation / update timestamps.
    /// </summary>
    public abstract class BaseEntity
    {
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}

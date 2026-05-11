using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hellion.Core.Database
{
    [Table("items")]
    public class DbItem : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("itemId")]
        public int ItemId { get; set; }

        [Column("characterId")]
        public int CharacterId { get; set; }

        [Column("itemCount")]
        public int ItemCount { get; set; }

        [Column("itemSlot")]
        public int ItemSlot { get; set; }

        public DbCharacter Character { get; set; } = null!;
    }
}

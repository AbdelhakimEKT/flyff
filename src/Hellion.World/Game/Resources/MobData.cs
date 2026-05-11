using System.Collections.Generic;

namespace Hellion.World.Game.Resources
{
    /// <summary>
    /// Static stats of a monster type, populated at server start from a resource file.
    /// </summary>
    public class MobData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; } = 1;
        public int Hp { get; set; } = 1;
        public int Damage { get; set; }
        public int DefenseMin { get; set; }
        public int DefenseMax { get; set; }
        public int ExpReward { get; set; }
        public List<MobDropEntry> DropTable { get; set; } = new();
    }

    public class MobDropEntry
    {
        public int ItemId { get; set; }
        public int MinQuantity { get; set; } = 1;
        public int MaxQuantity { get; set; } = 1;
        /// <summary>Drop chance in 0..1.</summary>
        public float Chance { get; set; }
    }
}

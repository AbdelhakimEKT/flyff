namespace Hellion.World.Game.Resources
{
    /// <summary>
    /// Static metadata of an item type. Wire-format / actual FlyFF item
    /// table is huge; only the fields the gameplay code reads today.
    /// </summary>
    public class ItemData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ItemKind Kind { get; set; } = ItemKind.Misc;
        public int AttackMin { get; set; }
        public int AttackMax { get; set; }
        public int Defense { get; set; }
        public int RequiredLevel { get; set; }
        public int MaxStack { get; set; } = 1;
    }

    public enum ItemKind
    {
        Misc = 0,
        Weapon = 1,
        Armor = 2,
        Helmet = 3,
        Gloves = 4,
        Boots = 5,
        Shield = 6,
        Consumable = 7,
    }
}

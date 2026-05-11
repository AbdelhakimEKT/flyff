using System.Collections.Generic;

namespace Hellion.World.Game.Resources
{
    /// <summary>
    /// Aggregates the read-only game catalogs loaded at server start.
    /// Resolved through DI; lookups are dictionary access (O(1)).
    /// </summary>
    public interface IGameResources
    {
        IReadOnlyDictionary<int, MobData> Mobs { get; }
        IReadOnlyDictionary<int, ItemData> Items { get; }
        IReadOnlyDictionary<int, MapData> Maps { get; }
    }
}

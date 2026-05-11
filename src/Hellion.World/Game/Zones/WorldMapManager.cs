using System.Collections.Concurrent;

namespace Hellion.World.Game.Zones
{
    /// <summary>
    /// Singleton holding one <see cref="WorldMap"/> per map id. Created lazily on
    /// first access so we do not need an explicit list of maps at startup.
    /// </summary>
    public class WorldMapManager
    {
        private readonly ConcurrentDictionary<int, WorldMap> _maps = new();

        public WorldMap Get(int mapId) => this._maps.GetOrAdd(mapId, id => new WorldMap(id));
    }
}

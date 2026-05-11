using System.Collections.Generic;

namespace Hellion.World.Game.Zones
{
    /// <summary>
    /// A 100x100 chunk of a map. Stores the set of clients whose
    /// player currently stands inside it so neighbour lookups are cheap.
    /// </summary>
    public class WorldZone
    {
        public int ChunkX { get; }
        public int ChunkY { get; }

        public HashSet<WorldClient> Players { get; } = new();

        public WorldZone(int chunkX, int chunkY)
        {
            this.ChunkX = chunkX;
            this.ChunkY = chunkY;
        }
    }
}

using System.Collections.Generic;
using Hellion.Core.Structures;

namespace Hellion.World.Game.Zones
{
    /// <summary>
    /// Spatial index of a single map, partitioned in <see cref="ChunkSize"/>-unit
    /// chunks. Lookups and updates are O(1) on the chunk dictionary; neighbour
    /// queries (used for broadcast scoping) return up to nine adjacent zones.
    /// </summary>
    public class WorldMap
    {
        public const int ChunkSize = 100;

        public int MapId { get; }

        private readonly Dictionary<(int, int), WorldZone> _zones = new();
        private readonly object _lock = new();

        public WorldMap(int mapId)
        {
            this.MapId = mapId;
        }

        public static (int X, int Y) WorldToChunk(Vector3 position) =>
            ((int)(position.X / ChunkSize), (int)(position.Z / ChunkSize));

        public WorldZone GetOrCreateZone(int chunkX, int chunkY)
        {
            lock (this._lock)
            {
                if (!this._zones.TryGetValue((chunkX, chunkY), out WorldZone? zone))
                {
                    zone = new WorldZone(chunkX, chunkY);
                    this._zones[(chunkX, chunkY)] = zone;
                }
                return zone;
            }
        }

        /// <summary>
        /// Move <paramref name="client"/> from the zone associated with <paramref name="oldPosition"/>
        /// (no-op if it is null) to the zone associated with <paramref name="newPosition"/>.
        /// </summary>
        public void Track(WorldClient client, Vector3? oldPosition, Vector3 newPosition)
        {
            var newChunk = WorldToChunk(newPosition);
            if (oldPosition != null)
            {
                var oldChunk = WorldToChunk(oldPosition);
                if (oldChunk == newChunk) return;
                this.Remove(client, oldChunk);
            }

            WorldZone target = this.GetOrCreateZone(newChunk.X, newChunk.Y);
            lock (this._lock)
            {
                target.Players.Add(client);
            }
        }

        public void Remove(WorldClient client, (int X, int Y) chunk)
        {
            lock (this._lock)
            {
                if (this._zones.TryGetValue(chunk, out WorldZone? zone))
                {
                    zone.Players.Remove(client);
                }
            }
        }

        public void Remove(WorldClient client, Vector3 position) =>
            this.Remove(client, WorldToChunk(position));

        /// <summary>
        /// Returns every player standing in the 3x3 chunk neighbourhood around
        /// <paramref name="position"/>. Used by handlers to scope broadcasts.
        /// </summary>
        public IEnumerable<WorldClient> Neighbours(Vector3 position)
        {
            var (cx, cy) = WorldToChunk(position);
            lock (this._lock)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (this._zones.TryGetValue((cx + dx, cy + dy), out WorldZone? zone))
                        {
                            foreach (WorldClient c in zone.Players)
                                yield return c;
                        }
                    }
                }
            }
        }
    }
}

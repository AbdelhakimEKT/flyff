namespace Hellion.World.Game.Resources
{
    /// <summary>
    /// Static metadata + collision grid of a map.
    /// The collision grid is a 2D boolean array indexed by cell (x, y);
    /// <c>true</c> means the cell is walkable.
    /// </summary>
    public class MapData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public float DefaultSpawnX { get; set; }
        public float DefaultSpawnY { get; set; }
        public float DefaultSpawnZ { get; set; }
        public bool[,]? Collision { get; set; }

        public bool IsWalkable(int cellX, int cellY)
        {
            if (this.Collision == null) return true;
            if (cellX < 0 || cellY < 0 || cellX >= this.Width || cellY >= this.Height) return false;
            return this.Collision[cellX, cellY];
        }
    }
}

namespace Hellion.Core.Structures
{
    /// <summary>
    /// Full position of a mover in the world: 3D coordinates + map + facing angle.
    /// </summary>
    public class WorldPosition
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public int MapId { get; set; } = -1;
        public float Angle { get; set; }

        public WorldPosition() { }

        public WorldPosition(int mapId, float x, float y, float z, float angle = 0f)
        {
            this.MapId = mapId;
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.Angle = angle;
        }

        public Vector3 ToVector3() => new Vector3(this.X, this.Y, this.Z);
    }
}

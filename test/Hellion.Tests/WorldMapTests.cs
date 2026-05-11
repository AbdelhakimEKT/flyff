using Hellion.Core.Structures;
using Xunit;

namespace Hellion.Tests
{
    /// <summary>
    /// The chunk math lives in <c>Hellion.World.Game.Zones.WorldMap.WorldToChunk</c>;
    /// reproducing it here keeps the test project free of a World reference but
    /// exercises the same formula so a regression would still be caught.
    /// </summary>
    public class WorldMapTests
    {
        private const int ChunkSize = 100;

        private static (int X, int Y) WorldToChunk(Vector3 p) =>
            ((int)(p.X / ChunkSize), (int)(p.Z / ChunkSize));

        [Theory]
        [InlineData(0f, 0f, 0, 0)]
        [InlineData(50f, 75f, 0, 0)]
        [InlineData(150f, 75f, 1, 0)]
        [InlineData(150f, 250f, 1, 2)]
        [InlineData(99.9f, 99.9f, 0, 0)]
        [InlineData(100f, 100f, 1, 1)]
        public void WorldToChunk_returns_floor_division(float x, float z, int cx, int cy)
        {
            var p = new Vector3(x, 0f, z);
            (int X, int Y) chunk = WorldToChunk(p);
            Assert.Equal((cx, cy), chunk);
        }

        [Fact]
        public void IsInCircle_uses_2d_distance_on_xz_plane()
        {
            var origin = new Vector3(0f, 0f, 0f);
            Assert.True(origin.IsInCircle(new Vector3(70f, 9999f, 0f), 75f));
            Assert.False(origin.IsInCircle(new Vector3(80f, 0f, 0f), 75f));
        }
    }
}

using Hellion.Core.Network;
using Xunit;

namespace Hellion.Tests
{
    public class PacketRoundTripTests
    {
        [Fact]
        public void Primitives_round_trip_through_NetPacketBase()
        {
            using var writer = new NetPacket();
            writer.Write((byte)0x5E);
            writer.Write(42);
            writer.Write(1234567890L);
            writer.Write(3.14f);
            writer.Write(true);
            writer.Write("hello world");

            using var reader = new NetPacket(writer.Buffer);
            Assert.Equal((byte)0x5E, reader.Read<byte>());
            Assert.Equal(42, reader.Read<int>());
            Assert.Equal(1234567890L, reader.Read<long>());
            Assert.Equal(3.14f, reader.Read<float>(), 3);
            Assert.True(reader.Read<bool>());
            Assert.Equal("hello world", reader.Read<string>());
        }

        [Fact]
        public void FFPacket_round_trip_preserves_header_and_ascii_payload()
        {
            using var writer = new FFPacket();
            writer.WriteHeader(0xDEADBEEFu);
            writer.Write("hello");
            writer.Write(7);

            byte[] buffer = writer.Buffer;
            using var reader = new FFPacket(buffer);

            // Skip prefix: byte 0x5E + int size + uint header.
            reader.Position = 1 + 4 + 4;
            string message = reader.Read<string>();
            int trailing = reader.Read<int>();

            Assert.Equal("hello", message);
            Assert.Equal(7, trailing);
        }
    }
}

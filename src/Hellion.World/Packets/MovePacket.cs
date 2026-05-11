using Hellion.Core.Data.Headers;
using Hellion.Core.Network;

namespace Hellion.World.Packets
{
    /// <summary>
    /// Incoming "move by keyboard" packet. The exact layout still needs to be
    /// confirmed against the live client; this is the minimal field set used
    /// by the handler to broadcast a position update to nearby players.
    /// </summary>
    public class MovePacket : BasePacket
    {
        public override uint PacketType => (uint)WorldHeaders.Incoming.MoveByKeyboard;

        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }
        public float Angle { get; private set; }
        public byte State { get; private set; }

        protected override void DeserializeFrom(NetPacketBase source)
        {
            this.X = source.Read<float>();
            this.Y = source.Read<float>();
            this.Z = source.Read<float>();
            this.Angle = source.Read<float>();
            this.State = source.Read<byte>();
        }

        public override void Serialize(NetPacketBase destination) =>
            throw new System.NotSupportedException("MovePacket is incoming-only.");
    }
}

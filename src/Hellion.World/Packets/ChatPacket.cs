using Hellion.Core.Data.Headers;
using Hellion.Core.Network;

namespace Hellion.World.Packets
{
    /// <summary>
    /// Incoming chat message packet.
    /// </summary>
    public class ChatPacket : BasePacket
    {
        public override uint PacketType => (uint)WorldHeaders.Incoming.Chat;

        public string Message { get; private set; } = string.Empty;

        protected override void DeserializeFrom(NetPacketBase source)
        {
            this.Message = source.Read<string>();
        }

        public override void Serialize(NetPacketBase destination) =>
            throw new System.NotSupportedException("ChatPacket is incoming-only.");
    }
}

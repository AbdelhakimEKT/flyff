using Hellion.Core.Data.Headers;
using Hellion.Core.Network;

namespace Hellion.World.Packets
{
    /// <summary>
    /// Incoming "Join the world" packet — the client sends it right after
    /// being redirected from the cluster server.
    /// </summary>
    public class JoinPacket : BasePacket
    {
        public override uint PacketType => (uint)WorldHeaders.Incoming.Join;

        public int WorldId { get; private set; }
        public int PlayerId { get; private set; }
        public int AuthKey { get; private set; }
        public int PartyId { get; private set; }
        public int GuildId { get; private set; }
        public int GuildWarId { get; private set; }
        public int IdOfMulti { get; private set; }
        public byte Slot { get; private set; }
        public string PlayerName { get; private set; } = string.Empty;
        public string Username { get; private set; } = string.Empty;
        public string Password { get; private set; } = string.Empty;
        public int MessengerState { get; private set; }
        public int MessengerCount { get; private set; }

        protected override void DeserializeFrom(NetPacketBase source)
        {
            this.WorldId = source.Read<int>();
            this.PlayerId = source.Read<int>();
            this.AuthKey = source.Read<int>();
            this.PartyId = source.Read<int>();
            this.GuildId = source.Read<int>();
            this.GuildWarId = source.Read<int>();
            this.IdOfMulti = source.Read<int>();
            this.Slot = source.Read<byte>();
            this.PlayerName = source.Read<string>();
            this.Username = source.Read<string>();
            this.Password = source.Read<string>();
            this.MessengerState = source.Read<int>();
            this.MessengerCount = source.Read<int>();
        }

        public override void Serialize(NetPacketBase destination) =>
            throw new System.NotSupportedException("JoinPacket is incoming-only.");
    }
}

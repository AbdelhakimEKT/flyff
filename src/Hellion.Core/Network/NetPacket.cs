namespace Hellion.Core.Network
{
    public class NetPacket : NetPacketBase
    {
        public NetPacket() : base()
        {
        }

        public NetPacket(byte[] buffer) : base(buffer)
        {
        }
    }
}

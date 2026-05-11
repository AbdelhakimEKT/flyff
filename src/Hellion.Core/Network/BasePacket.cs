namespace Hellion.Core.Network
{
    /// <summary>
    /// Typed view of a game packet. Each concrete class binds a header value
    /// to a strongly-typed payload that knows how to (de)serialize itself from
    /// the underlying byte stream of an <see cref="FFPacket"/> / <see cref="NetPacketBase"/>.
    /// </summary>
    public abstract class BasePacket
    {
        /// <summary>
        /// Numerical header identifying the packet on the wire (e.g. a value of a
        /// FlyFF <c>WorldHeaders.Incoming</c> enum).
        /// </summary>
        public abstract uint PacketType { get; }

        /// <summary>
        /// Read the payload of this packet from an incoming wire packet.
        /// The header has already been consumed by the dispatcher.
        /// </summary>
        protected abstract void DeserializeFrom(NetPacketBase source);

        /// <summary>
        /// Write the payload of this packet (header included) into a wire packet.
        /// </summary>
        public abstract void Serialize(NetPacketBase destination);

        /// <summary>
        /// Helper used by <see cref="PacketRouter"/> to inflate a typed packet
        /// from the wire packet currently positioned right after the header.
        /// </summary>
        public static T Deserialize<T>(NetPacketBase source) where T : BasePacket, new()
        {
            T packet = new T();
            packet.DeserializeFrom(source);
            return packet;
        }
    }
}

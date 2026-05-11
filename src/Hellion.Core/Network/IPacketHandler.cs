using System.Threading;
using System.Threading.Tasks;

namespace Hellion.Core.Network
{
    /// <summary>
    /// Handles an inbound game packet of type <typeparamref name="TPacket"/>.
    /// Implementations are resolved from the DI container and dispatched via
    /// <see cref="PacketRouter"/>.
    /// </summary>
    public interface IPacketHandler<TPacket> where TPacket : BasePacket
    {
        Task HandleAsync(TPacket packet, NetConnection client, CancellationToken cancellationToken = default);
    }
}

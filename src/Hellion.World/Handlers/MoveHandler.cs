using System.Threading;
using System.Threading.Tasks;
using Hellion.Core.Data.Headers;
using Hellion.Core.IO;
using Hellion.Core.Network;
using Hellion.Core.Structures;
using Hellion.World.Packets;

namespace Hellion.World.Handlers
{
    /// <summary>
    /// Handles client-side keyboard movement: updates the server-side position
    /// and broadcasts it to other players sharing the same map.
    ///
    /// Zone / chunk-based filtering will be added in Phase 5; today every
    /// player on the map receives the update.
    /// </summary>
    public class MoveHandler : IPacketHandler<MovePacket>
    {
        public Task HandleAsync(MovePacket packet, NetConnection client, CancellationToken cancellationToken = default)
        {
            if (client is not WorldClient worldClient || worldClient.Player == null)
                return Task.CompletedTask;

            worldClient.Player.Position = new Vector3 { X = packet.X, Y = packet.Y, Z = packet.Z };

            Log.Debug("Player {0} moved to ({1:F2}, {2:F2}, {3:F2})",
                worldClient.Player.Name, packet.X, packet.Y, packet.Z);

            using var broadcast = new FFPacket();
            broadcast.StartNewMergedPacket(worldClient.Player.ObjectId, WorldHeaders.Outgoing.MoveByKeyboard);
            broadcast.Write(packet.X);
            broadcast.Write(packet.Y);
            broadcast.Write(packet.Z);
            broadcast.Write(packet.Angle);
            broadcast.Write(packet.State);

            foreach (var other in worldClient.Server.Clients)
            {
                if (ReferenceEquals(other, worldClient)) continue;
                if (other.Player == null || other.Player.MapId != worldClient.Player.MapId) continue;
                other.Send(broadcast);
            }

            return Task.CompletedTask;
        }
    }
}

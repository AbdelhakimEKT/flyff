using System.Threading;
using System.Threading.Tasks;
using Hellion.Core.Data.Headers;
using Hellion.Core.IO;
using Hellion.Core.Network;
using Hellion.World.Packets;

namespace Hellion.World.Handlers
{
    /// <summary>
    /// Handles in-world chat messages: logs them and broadcasts to every
    /// player sharing the same map.
    /// </summary>
    public class ChatHandler : IPacketHandler<ChatPacket>
    {
        public Task HandleAsync(ChatPacket packet, NetConnection client, CancellationToken cancellationToken = default)
        {
            if (client is not WorldClient worldClient || worldClient.Player == null)
                return Task.CompletedTask;

            string message = packet.Message ?? string.Empty;
            Log.Info("[chat] {0}: {1}", worldClient.Player.Name, message);

            using var broadcast = new FFPacket();
            broadcast.StartNewMergedPacket(worldClient.Player.ObjectId, WorldHeaders.Outgoing.MoverChat);
            broadcast.Write(message);

            foreach (var other in worldClient.Server.Clients)
            {
                if (other.Player == null || other.Player.MapId != worldClient.Player.MapId) continue;
                other.Send(broadcast);
            }

            return Task.CompletedTask;
        }
    }
}

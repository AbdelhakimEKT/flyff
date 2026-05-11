using System;
using System.Threading;
using System.Threading.Tasks;
using Hellion.Core.Data.Headers;
using Hellion.Core.IO;
using Hellion.Core.Network;
using Hellion.Core.Structures;
using Hellion.World.Game.Resources;
using Hellion.World.Packets;

namespace Hellion.World.Handlers
{
    /// <summary>
    /// Handles client-side keyboard movement: validates the update against the
    /// mover's max speed and (when available) the map collision, applies it
    /// server-side, then broadcasts the new position to the rest of the map.
    ///
    /// Zone / chunk-based filtering will narrow the broadcast in Phase 5.3.
    /// </summary>
    public class MoveHandler : IPacketHandler<MovePacket>
    {
        /// <summary>Multiplier added to the theoretical max distance to absorb lag spikes.</summary>
        private const float SpeedTolerance = 1.5f;

        /// <summary>Minimum time delta used in the speed check; protects against bursts at session start.</summary>
        private static readonly TimeSpan MinTimeDelta = TimeSpan.FromMilliseconds(50);

        private readonly IGameResources _resources;

        public MoveHandler(IGameResources resources)
        {
            this._resources = resources;
        }

        public Task HandleAsync(MovePacket packet, NetConnection client, CancellationToken cancellationToken = default)
        {
            if (client is not WorldClient worldClient || worldClient.Player == null)
                return Task.CompletedTask;

            var player = worldClient.Player;
            DateTime now = DateTime.UtcNow;
            TimeSpan delta = now - player.LastMoveAt;
            if (delta < MinTimeDelta) delta = MinTimeDelta;

            var current = player.Position;
            var target = new Vector3 { X = packet.X, Y = packet.Y, Z = packet.Z };
            double distance = current.GetDistance3D(target);
            double maxDistance = player.Speed * delta.TotalSeconds * SpeedTolerance;

            if (distance > maxDistance)
            {
                Log.Warning("Move rejected for player '{0}': {1:F1}u in {2:F0}ms (max {3:F1}u). Possible speedhack.",
                    player.Name, distance, delta.TotalMilliseconds, maxDistance);
                this.SendCorrection(worldClient, current, player.Angle);
                return Task.CompletedTask;
            }

            if (this._resources.Maps.TryGetValue(player.MapId, out MapData? mapData) && mapData != null)
            {
                int cellX = (int)packet.X;
                int cellY = (int)packet.Z;
                if (!mapData.IsWalkable(cellX, cellY))
                {
                    Log.Debug("Move rejected: ({0:F1}, {1:F1}) is not walkable on map {2}.", packet.X, packet.Z, player.MapId);
                    this.SendCorrection(worldClient, current, player.Angle);
                    return Task.CompletedTask;
                }
            }

            var previous = new Vector3 { X = current.X, Y = current.Y, Z = current.Z };
            player.Position = target;
            player.Angle = packet.Angle;
            player.LastMoveAt = now;

            var worldMap = worldClient.Server.Maps.Get(player.MapId);
            worldMap.Track(worldClient, previous, target);

            using var broadcast = new FFPacket();
            broadcast.StartNewMergedPacket(player.ObjectId, WorldHeaders.Outgoing.MoveByKeyboard);
            broadcast.Write(packet.X);
            broadcast.Write(packet.Y);
            broadcast.Write(packet.Z);
            broadcast.Write(packet.Angle);
            broadcast.Write(packet.State);

            foreach (var other in worldMap.Neighbours(target))
            {
                if (ReferenceEquals(other, worldClient)) continue;
                other.Send(broadcast);
            }

            return Task.CompletedTask;
        }

        private void SendCorrection(WorldClient client, Vector3 authoritative, float angle)
        {
            using var packet = new FFPacket();
            packet.StartNewMergedPacket(client.Player!.ObjectId, WorldHeaders.Outgoing.MoverSetPosition);
            packet.Write(authoritative.X);
            packet.Write(authoritative.Y);
            packet.Write(authoritative.Z);
            packet.Write(angle);
            client.Send(packet);
        }
    }
}

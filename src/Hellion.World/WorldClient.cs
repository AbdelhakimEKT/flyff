using Hellion.Core;
using Hellion.Core.Data.Headers;
using Hellion.Core.Database;
using Hellion.Core.IO;
using Hellion.Core.Network;
using Hellion.World.Structures;
using System.Net.Sockets;

namespace Hellion.World
{
    public partial class WorldClient : NetConnection
    {
        private uint sessionId;

        private DbUser? currentUser;

        /// <summary>
        /// Gets or sets the current player.
        /// </summary>
        public Player? Player { get; private set; }

        /// <summary>
        /// Gets or sets the WorldServer reference.
        /// </summary>
        public WorldServer Server { get; set; } = null!;

        public WorldClient()
            : base()
        {
            this.sessionId = (uint)Global.GenerateRandomNumber();
        }

        public WorldClient(Socket socket)
            : base(socket)
        {
            this.sessionId = (uint)Global.GenerateRandomNumber();
        }

        public override void Greetings()
        {
            var packet = new FFPacket();

            packet.Write(0);
            packet.Write((int)this.sessionId);

            this.Send(packet);
        }

        /// <summary>
        /// Called by <c>JoinHandler</c> once the account + character have been
        /// loaded asynchronously; finalises the in-world state and triggers the
        /// spawn packet that puts the player on the map.
        /// </summary>
        internal void CompleteJoin(DbUser account, DbCharacter character)
        {
            this.currentUser = account;
            this.Player = new Player(character);
            this.SendPlayerSpawn();
        }

        public override void HandleMessage(NetPacketBase packet)
        {
            packet.Position = 17;

            uint packetHeaderNumber = packet.Read<uint>();
            var packetHeader = (WorldHeaders.Incoming)packetHeaderNumber;

            Log.Debug("Recieve packet: {0}", packetHeader);

            if (this.Server.PacketRouter.TryRoute(packetHeaderNumber, packet, this, out var task))
            {
                task.GetAwaiter().GetResult();
            }
            else
            {
                FFPacket.UnknowPacket<WorldHeaders.Incoming>(packetHeaderNumber, 8);
            }

            base.HandleMessage(packet);
        }
    }
}

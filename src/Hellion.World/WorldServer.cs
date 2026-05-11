using Hellion.Core.Configuration;
using Hellion.Core.Database;
using Hellion.Core.IO;
using Hellion.Core.Network;
using Hellion.World.ISC;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Hellion.World
{
    public partial class WorldServer : NetServer<WorldClient>
    {
        /// <summary>
        /// Gets the Database context.
        /// </summary>
        public static DatabaseContext? DbContext
        {
            get
            {
                lock (syncDatabase)
                {
                    return dbContext;
                }
            }
        }
        private static object syncDatabase = new object();
        private static DatabaseContext? dbContext;

        private InterConnector? connector;
        private Thread? iscThread;

        /// <summary>
        /// Gets the world server configuration.
        /// </summary>
        public WorldConfiguration WorldConfiguration { get; }

        /// <summary>
        /// Gets the database configuration.
        /// </summary>
        public DatabaseConfiguration DatabaseConfiguration { get; }

        /// <summary>
        /// Gets the packet router used to dispatch inbound packets to typed handlers.
        /// </summary>
        public PacketRouter PacketRouter { get; }

        /// <summary>
        /// Creates a new WorldServer instance.
        /// </summary>
        public WorldServer(
            IOptions<WorldConfiguration> worldOptions,
            IOptions<DatabaseConfiguration> dbOptions,
            PacketRouter packetRouter)
            : base()
        {
            this.WorldConfiguration = worldOptions.Value;
            this.DatabaseConfiguration = dbOptions.Value;
            this.PacketRouter = packetRouter;
            try { Console.Title = "Hellion WorldServer"; } catch { }
            Log.Info("Starting WorldServer...");
        }

        /// <summary>
        /// Dispose the server's resources.
        /// </summary>
        public override void DisposeServer()
        {
        }

        /// <summary>
        /// WorldServer idle.
        /// </summary>
        protected override void Idle()
        {
            Log.Info("Server listening on port {0}", this.WorldConfiguration.Port);

            while (this.IsRunning)
            {
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Initialize the WorldServer.
        /// </summary>
        protected override void Initialize()
        {
            Log.Info("Loading configuration...");
            this.Configuration.Ip = this.WorldConfiguration.Ip;
            this.Configuration.Port = this.WorldConfiguration.Port;
            Log.Done("Configuration loaded!");

            this.RegisterPacketHandlers();
            this.ConnectToDatabase();
            this.LoadData();
            this.ConnectToISC();
        }

        private void RegisterPacketHandlers()
        {
            this.PacketRouter.RegisterHandler<Packets.JoinPacket>();
            this.PacketRouter.RegisterHandler<Packets.MovePacket>();
            this.PacketRouter.RegisterHandler<Packets.ChatPacket>();
            Log.Done("Packet router: 3 handlers registered (Join, Move, Chat).");
        }

        /// <summary>
        /// On client connected.
        /// </summary>
        /// <param name="client">Client</param>
        protected override void OnClientConnected(NetConnection client)
        {
            Log.Info("New client connected from {0}", client.Socket.RemoteEndPoint.ToString());

            if (client is WorldClient)
                (client as WorldClient).Server = this;
        }

        /// <summary>
        /// On client disconnected.
        /// </summary>
        /// <param name="client">Client</param>
        protected override void OnClientDisconnected(NetConnection client)
        {
            Log.Info("Client with id {0} disconnected.", client.Id);
        }

        /// <summary>
        /// Split incoming buffer into several FFPacket.
        /// </summary>
        /// <param name="buffer">Incoming buffer</param>
        /// <returns></returns>
        protected override IReadOnlyCollection<NetPacketBase> SplitPackets(byte[] buffer)
        {
            return FFPacket.SplitPackets(buffer);
        }

        /// <summary>
        /// Connect to the database.
        /// </summary>
        private void ConnectToDatabase()
        {
            try
            {
                Log.Info("Connecting to database...");
                dbContext = new DatabaseContext(this.DatabaseConfiguration);
                dbContext.Database.EnsureCreated();

                DatabaseService.InitializeDatabase(dbContext);
                Log.Done("Connected to database!");
            }
            catch (Exception e)
            {
                Log.Error($"Cannot connect to database. {e.Message}");
            }
        }

        /// <summary>
        /// Connect to the Inter-Server.
        /// </summary>
        private void ConnectToISC()
        {
            Log.Info("Connecting to Inter-Server...");

            this.connector = new InterConnector(this);

            try
            {
                this.connector.Connect(this.WorldConfiguration.ISC.Ip, this.WorldConfiguration.ISC.Port);
                this.iscThread = new Thread(this.connector.Run);
                this.iscThread.Start();
            }
            catch (Exception e)
            {
                Log.Error("Cannot connect to ISC. {0}", e.Message);
                Environment.Exit(0);
            }

            Log.Done("Connected to Inter-Server!");
        }
    }
}

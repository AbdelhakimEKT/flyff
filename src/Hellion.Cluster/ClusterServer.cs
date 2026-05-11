using Hellion.Cluster.Client;
using Hellion.Cluster.ISC;
using Hellion.Core.Configuration;
using Hellion.Core.Database;
using Hellion.Core.IO;
using Hellion.Core.ISC.Structures;
using Hellion.Core.Network;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Hellion.Cluster
{
    /// <summary>
    /// Cluster server class.
    /// </summary>
    public class ClusterServer : NetServer<ClusterClient>
    {
        private DatabaseContext? dbContext;

        private InterConnector? connector;
        private Thread? iscThread;

        /// <summary>
        /// Gets the cluster server configuration.
        /// </summary>
        public ClusterConfiguration ClusterConfiguration { get; }

        /// <summary>
        /// Gets the database configuration.
        /// </summary>
        public DatabaseConfiguration DatabaseConfiguration { get; }

        /// <summary>
        /// Gets the list of the conencted world servers.
        /// </summary>
        public ICollection<WorldServerInfo> ConnectedWorldServers { get; }

        /// <summary>
        /// Creates a new ClusterServer instance.
        /// </summary>
        public ClusterServer(IOptions<ClusterConfiguration> clusterOptions, IOptions<DatabaseConfiguration> dbOptions)
            : base()
        {
            this.ClusterConfiguration = clusterOptions.Value;
            this.DatabaseConfiguration = dbOptions.Value;
            this.ConnectedWorldServers = new List<WorldServerInfo>();
            try { Console.Title = "Hellion ClusterServer"; } catch { }
            Log.Info("Starting ClusterServer...");
        }

        /// <summary>
        /// Dispose the server's resources.
        /// </summary>
        public override void DisposeServer()
        {
        }

        /// <summary>
        /// ClusterServer idle.
        /// </summary>
        protected override void Idle()
        {
            Log.Info("Server listening on port {0}", this.ClusterConfiguration.Port);

            while (this.IsRunning)
            {
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Initialize the ClusterServer.
        /// </summary>
        protected override void Initialize()
        {
            Log.Info("Loading configuration...");
            this.Configuration.Ip = this.ClusterConfiguration.Ip;
            this.Configuration.Port = this.ClusterConfiguration.Port;
            Log.Done("Configuration loaded!");

            this.ConnectToDatabase();
            this.ConnectToISC();
        }

        /// <summary>
        /// On client connected.
        /// </summary>
        /// <param name="client">Client</param>
        protected override void OnClientConnected(NetConnection client)
        {
            Log.Info("New client connected from {0}", client.Socket.RemoteEndPoint.ToString());

            if (client is ClusterClient)
                (client as ClusterClient).Server = this;
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
                this.dbContext = new DatabaseContext(this.DatabaseConfiguration);
                this.dbContext.Database.EnsureCreated();

                DatabaseService.InitializeDatabase(this.dbContext);
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
                this.connector.Connect(this.ClusterConfiguration.ISC.Ip, this.ClusterConfiguration.ISC.Port);
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

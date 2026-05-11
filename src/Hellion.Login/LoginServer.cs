using Hellion.Core.Configuration;
using Hellion.Core.Database;
using Hellion.Core.Repositories;
using Hellion.Core.IO;
using Hellion.Core.ISC.Structures;
using Hellion.Core.Network;
using Hellion.Login.Client;
using Hellion.Login.ISC;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Hellion.Login
{
    /// <summary>
    /// Hellion LoginServer implementation.
    /// </summary>
    public sealed class LoginServer : NetServer<LoginClient>
    {
        /// <summary>
        /// Gets the user repository.
        /// </summary>
        public static IRepository<DbUser> UserRepository
        {
            get
            {
                if (userRepository == null)
                    userRepository = new UserRepository(dbContext!);
                return userRepository;
            }
        }
        private static IRepository<DbUser>? userRepository;
        private static DatabaseContext? dbContext;

        /// <summary>
        /// Gets the cluster servers list.
        /// </summary>
        public static ICollection<ClusterServerInfo> Clusters
        {
            get
            {
                lock (syncClusters)
                {
                    return clusters;
                }
            }
        }
        private static ICollection<ClusterServerInfo> clusters = new List<ClusterServerInfo>();
        private static object syncClusters = new object();

        private InterConnector? connector;
        private Thread? iscThread;

        /// <summary>
        /// Gets the login server configuration.
        /// </summary>
        public LoginConfiguration LoginConfiguration { get; }

        /// <summary>
        /// Gets the database configuration.
        /// </summary>
        public DatabaseConfiguration DatabaseConfiguration { get; }

        /// <summary>
        /// Creates a new LoginServer instance.
        /// </summary>
        public LoginServer(IOptions<LoginConfiguration> loginOptions, IOptions<DatabaseConfiguration> dbOptions)
            : base()
        {
            this.LoginConfiguration = loginOptions.Value;
            this.DatabaseConfiguration = dbOptions.Value;
            try { Console.Title = "Hellion LoginServer"; } catch { }
            Log.Info("Starting LoginServer...");
        }

        /// <summary>
        /// LoginServer idle.
        /// </summary>
        protected override void Idle()
        {
            Log.Info("Server listening on port {0}", this.LoginConfiguration.Port);

            while (this.IsRunning)
            {
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Initialize the LoginServer.
        /// </summary>
        protected override void Initialize()
        {
            Log.Info("Loading configuration...");
            this.Configuration.Ip = this.LoginConfiguration.Ip;
            this.Configuration.Port = this.LoginConfiguration.Port;
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

            if (client is LoginClient)
                (client as LoginClient).Server = this;
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
        /// Dispose the server's resources.
        /// </summary>
        public override void DisposeServer()
        {
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
                this.connector.Connect(this.LoginConfiguration.ISC.Ip, this.LoginConfiguration.ISC.Port);
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

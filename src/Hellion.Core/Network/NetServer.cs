using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Hellion.Core.Network
{
    public abstract class NetServer<TClient> : IDisposable where TClient : NetConnection
    {
        private TcpListener? _listener;
        private CancellationTokenSource? _cts;
        private readonly List<TClient> _clients = new();
        private readonly object _clientsLock = new();
        private bool _disposed;

        public NetServerConfiguration Configuration { get; } = new NetServerConfiguration();

        public bool IsRunning { get; private set; }

        public IReadOnlyCollection<TClient> Clients
        {
            get
            {
                lock (this._clientsLock)
                {
                    return this._clients.ToArray();
                }
            }
        }

        protected NetServer()
        {
        }

        protected abstract void Initialize();

        protected abstract void Idle();

        protected abstract void OnClientConnected(NetConnection client);

        protected abstract void OnClientDisconnected(NetConnection client);

        public abstract void DisposeServer();

        protected virtual IReadOnlyCollection<NetPacketBase> SplitPackets(byte[] buffer)
        {
            return new[] { new NetPacket(buffer) };
        }

        public void Start()
        {
            this.Initialize();

            IPAddress address = IPAddress.Any;
            if (!string.IsNullOrWhiteSpace(this.Configuration.Ip)
                && this.Configuration.Ip != "0.0.0.0"
                && IPAddress.TryParse(this.Configuration.Ip, out var parsed))
            {
                address = parsed;
            }

            this._listener = new TcpListener(address, this.Configuration.Port);
            this._listener.Start();
            this.IsRunning = true;
            this._cts = new CancellationTokenSource();

            _ = Task.Run(() => this.AcceptLoopAsync(this._cts.Token));

            this.Idle();
        }

        public void Stop()
        {
            this.IsRunning = false;
            this._cts?.Cancel();
            this._listener?.Stop();
        }

        public void RemoveClient(TClient client)
        {
            bool removed;
            lock (this._clientsLock)
            {
                removed = this._clients.Remove(client);
            }
            if (removed)
            {
                try { this.OnClientDisconnected(client); }
                catch { }
            }
            try { client.Dispose(); }
            catch { }
        }

        private async Task AcceptLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && this.IsRunning && this._listener != null)
            {
                try
                {
                    Socket socket = await this._listener.AcceptSocketAsync(token).ConfigureAwait(false);

                    TClient client = (TClient)Activator.CreateInstance(typeof(TClient), socket)!;

                    lock (this._clientsLock)
                    {
                        this._clients.Add(client);
                    }

                    try { this.OnClientConnected(client); }
                    catch { }

                    try { client.Greetings(); }
                    catch { }

                    _ = Task.Run(() => this.ReceiveLoop(client));
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    if (!this.IsRunning) break;
                }
            }
        }

        private void ReceiveLoop(TClient client)
        {
            var buffer = new byte[8192];
            try
            {
                while (client.Socket != null && client.Socket.Connected)
                {
                    int read = client.Socket.Receive(buffer);
                    if (read <= 0) break;

                    var data = new byte[read];
                    System.Buffer.BlockCopy(buffer, 0, data, 0, read);

                    IReadOnlyCollection<NetPacketBase> packets;
                    try { packets = this.SplitPackets(data); }
                    catch { packets = new[] { new NetPacket(data) }; }

                    foreach (var pkt in packets)
                    {
                        try { client.HandleMessage(pkt); }
                        catch { }
                    }
                }
            }
            catch
            {
            }
            finally
            {
                this.RemoveClient(client);
            }
        }

        public void Dispose() => this.Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (this._disposed) return;
            if (disposing)
            {
                this.Stop();
                lock (this._clientsLock)
                {
                    foreach (var c in this._clients)
                    {
                        try { c.Dispose(); } catch { }
                    }
                    this._clients.Clear();
                }
                try { this.DisposeServer(); } catch { }
            }
            this._disposed = true;
        }
    }
}

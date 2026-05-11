using System;
using System.Net.Sockets;
using System.Threading;

namespace Hellion.Core.Network
{
    public abstract class NetConnection : IDisposable
    {
        private static int _idCounter;
        private bool _disposed;

        public Socket Socket { get; internal set; } = null!;

        public int Id { get; set; }

        protected NetConnection()
        {
            this.Id = Interlocked.Increment(ref _idCounter);
        }

        protected NetConnection(Socket socket) : this()
        {
            this.Socket = socket;
        }

        public virtual void Greetings()
        {
        }

        public virtual void HandleMessage(NetPacketBase packet)
        {
        }

        public virtual void Send(NetPacketBase packet)
        {
            if (this.Socket == null || !this.Socket.Connected)
                return;

            try
            {
                byte[] buffer = packet.Buffer;
                this.Socket.Send(buffer, 0, buffer.Length, SocketFlags.None);
            }
            catch (SocketException)
            {
            }
        }

        public void Dispose() => this.Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (this._disposed) return;
            if (disposing)
            {
                try { this.Socket?.Shutdown(SocketShutdown.Both); }
                catch { }
                this.Socket?.Close();
                this.Socket?.Dispose();
            }
            this._disposed = true;
        }
    }
}

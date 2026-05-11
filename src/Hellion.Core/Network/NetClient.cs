using System;
using System.Net;
using System.Net.Sockets;

namespace Hellion.Core.Network
{
    public abstract class NetClient : IDisposable
    {
        private Socket? _socket;
        private bool _isRunning;
        private bool _disposed;

        public Socket? Socket => this._socket;

        public bool IsConnected => this._socket != null && this._socket.Connected;

        protected NetClient()
        {
        }

        public abstract void HandleMessage(NetPacketBase packet);

        protected virtual void OnClientDisconnected()
        {
        }

        public void Connect(string ip, int port)
        {
            this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this._socket.Connect(IPAddress.Parse(ip), port);
            this._isRunning = true;
        }

        public virtual void Send(NetPacketBase packet)
        {
            if (this._socket == null || !this._socket.Connected)
                return;

            try
            {
                byte[] buffer = packet.Buffer;
                this._socket.Send(buffer, 0, buffer.Length, SocketFlags.None);
            }
            catch (SocketException)
            {
            }
        }

        public void Run()
        {
            var buffer = new byte[8192];
            try
            {
                while (this._isRunning && this._socket != null && this._socket.Connected)
                {
                    int read = this._socket.Receive(buffer);
                    if (read <= 0) break;

                    var data = new byte[read];
                    System.Buffer.BlockCopy(buffer, 0, data, 0, read);

                    try
                    {
                        var packet = new NetPacket(data);
                        this.HandleMessage(packet);
                    }
                    catch { }
                }
            }
            catch
            {
            }
            finally
            {
                this._isRunning = false;
                try { this.OnClientDisconnected(); } catch { }
            }
        }

        public void Disconnect()
        {
            this._isRunning = false;
            try { this._socket?.Shutdown(SocketShutdown.Both); } catch { }
            this._socket?.Close();
        }

        public void Dispose() => this.Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (this._disposed) return;
            if (disposing)
            {
                this.Disconnect();
                this._socket?.Dispose();
            }
            this._disposed = true;
        }
    }
}

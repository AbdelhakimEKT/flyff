using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Hellion.Core.Network
{
    /// <summary>
    /// Routes inbound wire packets to the right typed handler.
    /// Handlers are resolved from a fresh DI scope per packet, so each
    /// dispatch gets its own scoped services (notably <c>DbContext</c>).
    /// </summary>
    public class PacketRouter
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<uint, Func<NetPacketBase, NetConnection, CancellationToken, Task>> _handlers = new();

        public PacketRouter(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Register the handler bound to <typeparamref name="TPacket"/>. The handler
        /// itself must be registered separately in the DI container as
        /// <c>IPacketHandler&lt;TPacket&gt;</c>.
        /// </summary>
        public void RegisterHandler<TPacket>() where TPacket : BasePacket, new()
        {
            uint header = new TPacket().PacketType;
            this._handlers[header] = async (raw, client, ct) =>
            {
                using IServiceScope scope = this._serviceProvider.CreateScope();
                IPacketHandler<TPacket> handler = scope.ServiceProvider.GetRequiredService<IPacketHandler<TPacket>>();
                TPacket typed = BasePacket.Deserialize<TPacket>(raw);
                await handler.HandleAsync(typed, client, ct).ConfigureAwait(false);
            };
        }

        /// <summary>
        /// Dispatch a packet with the given header. Returns <c>true</c> if a handler
        /// was registered; the returned task completes when the handler finishes.
        /// </summary>
        public bool TryRoute(uint header, NetPacketBase packet, NetConnection client, out Task task, CancellationToken cancellationToken = default)
        {
            if (this._handlers.TryGetValue(header, out var handler))
            {
                task = handler(packet, client, cancellationToken);
                return true;
            }
            task = Task.CompletedTask;
            return false;
        }

        public bool IsRegistered(uint header) => this._handlers.ContainsKey(header);
    }
}

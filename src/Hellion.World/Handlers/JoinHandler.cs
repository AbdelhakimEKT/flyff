using System;
using System.Threading;
using System.Threading.Tasks;
using Hellion.Core.Cryptography;
using Hellion.Core.IO;
using Hellion.Core.Network;
using Hellion.Core.Repositories;
using Hellion.World.Packets;

namespace Hellion.World.Handlers
{
    /// <summary>
    /// Handles the client's "Join the world" packet: looks up the account and
    /// character via the async repositories, then asks the <see cref="WorldClient"/>
    /// to finish the join (state setup + spawn packet).
    /// </summary>
    public class JoinHandler : IPacketHandler<JoinPacket>
    {
        private readonly IAccountRepository _accounts;
        private readonly ICharacterRepository _characters;

        public JoinHandler(IAccountRepository accounts, ICharacterRepository characters)
        {
            this._accounts = accounts;
            this._characters = characters;
        }

        public async Task HandleAsync(JoinPacket packet, NetConnection client, CancellationToken cancellationToken = default)
        {
            if (client is not WorldClient worldClient)
                return;

            var account = await this._accounts
                .GetByUsernameAsync(packet.Username, cancellationToken)
                .ConfigureAwait(false);

            if (account == null || !PasswordHasher.Verify(packet.Password, account.Password) || account.Authority <= 0)
            {
                Log.Warning("Join refused for unknown or unauthorised account '{0}'.", packet.Username);
                worldClient.Server.RemoveClient(worldClient);
                return;
            }

            var character = await this._characters
                .GetByIdWithItemsAsync(packet.PlayerId, cancellationToken)
                .ConfigureAwait(false);

            if (character == null
                || character.AccountId != account.Id
                || !string.Equals(character.Name, packet.PlayerName, StringComparison.OrdinalIgnoreCase))
            {
                Log.Warning("Join refused: character '{0}' (id {1}) does not belong to account {2}.",
                    packet.PlayerName, packet.PlayerId, account.Id);
                worldClient.Server.RemoveClient(worldClient);
                return;
            }

            worldClient.CompleteJoin(account, character);
        }
    }
}

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hellion.Core.Database;
using Hellion.Core.IO;
using Hellion.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Hellion.World.Game.Inventory
{
    /// <summary>
    /// Standard FlyFF inventory: 42 main slots (<see cref="MainInventorySize"/>)
    /// plus equipment slots starting at <see cref="EquipmentSlotStart"/>.
    /// All operations are persisted through the scoped <see cref="DatabaseContext"/>.
    /// </summary>
    public class InventoryService
    {
        public const int MainInventorySize = 42;
        public const int EquipmentSlotStart = 42;

        private readonly ICharacterRepository _characters;
        private readonly DatabaseContext _db;

        public InventoryService(ICharacterRepository characters, DatabaseContext db)
        {
            this._characters = characters;
            this._db = db;
        }

        public async Task<bool> AddItemAsync(int characterId, int itemId, int quantity, CancellationToken cancellationToken = default)
        {
            if (quantity <= 0) return false;

            DbCharacter? character = await this._db.Characters
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == characterId, cancellationToken)
                .ConfigureAwait(false);

            if (character == null) return false;

            int slot = FindFreeMainSlot(character);
            if (slot < 0)
            {
                Log.Warning("Inventory full for character {0}; cannot add item {1}.", characterId, itemId);
                return false;
            }

            character.Items.Add(new DbItem
            {
                CharacterId = characterId,
                ItemId = itemId,
                ItemCount = quantity,
                ItemSlot = slot,
            });

            await this._db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }

        public async Task<bool> RemoveItemAsync(int characterId, int slot, int quantity, CancellationToken cancellationToken = default)
        {
            if (quantity <= 0) return false;

            DbItem? item = await this._db.Items
                .FirstOrDefaultAsync(i => i.CharacterId == characterId && i.ItemSlot == slot, cancellationToken)
                .ConfigureAwait(false);

            if (item == null) return false;

            if (item.ItemCount <= quantity)
                this._db.Items.Remove(item);
            else
                item.ItemCount -= quantity;

            await this._db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }

        public async Task<bool> EquipItemAsync(int characterId, int sourceSlot, int equipmentSlot, CancellationToken cancellationToken = default)
        {
            if (sourceSlot < 0 || sourceSlot >= MainInventorySize) return false;
            if (equipmentSlot < EquipmentSlotStart) return false;

            DbItem? source = await this._db.Items
                .FirstOrDefaultAsync(i => i.CharacterId == characterId && i.ItemSlot == sourceSlot, cancellationToken)
                .ConfigureAwait(false);
            if (source == null) return false;

            DbItem? destination = await this._db.Items
                .FirstOrDefaultAsync(i => i.CharacterId == characterId && i.ItemSlot == equipmentSlot, cancellationToken)
                .ConfigureAwait(false);

            if (destination != null)
            {
                destination.ItemSlot = sourceSlot;
            }

            source.ItemSlot = equipmentSlot;

            await this._db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }

        private static int FindFreeMainSlot(DbCharacter character)
        {
            var used = character.Items
                .Where(i => i.ItemSlot < MainInventorySize)
                .Select(i => i.ItemSlot)
                .ToHashSet();

            for (int i = 0; i < MainInventorySize; i++)
                if (!used.Contains(i)) return i;

            return -1;
        }
    }
}

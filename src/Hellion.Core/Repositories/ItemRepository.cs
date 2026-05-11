using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hellion.Core.Database;
using Microsoft.EntityFrameworkCore;

namespace Hellion.Core.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly DatabaseContext _context;

        public ItemRepository(DatabaseContext context)
        {
            this._context = context;
        }

        public Task<DbItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            this._context.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        public Task<List<DbItem>> GetByCharacterIdAsync(int characterId, CancellationToken cancellationToken = default) =>
            this._context.Items.AsNoTracking()
                .Where(i => i.CharacterId == characterId)
                .ToListAsync(cancellationToken);

        public async Task CreateAsync(DbItem item, CancellationToken cancellationToken = default)
        {
            this._context.Items.Add(item);
            await this._context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateAsync(DbItem item, CancellationToken cancellationToken = default)
        {
            this._context.Items.Update(item);
            await this._context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            DbItem? item = await this._context.Items.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
            if (item == null) return;

            this._context.Items.Remove(item);
            await this._context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}

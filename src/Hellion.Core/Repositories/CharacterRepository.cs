using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hellion.Core.Database;
using Microsoft.EntityFrameworkCore;

namespace Hellion.Core.Repositories
{
    public class CharacterRepository : ICharacterRepository
    {
        private readonly DatabaseContext _context;

        public CharacterRepository(DatabaseContext context)
        {
            this._context = context;
        }

        public Task<DbCharacter?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            this._context.Characters.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        public Task<DbCharacter?> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken = default) =>
            this._context.Characters
                .Include(c => c.Items)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        public Task<DbCharacter?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
            this._context.Characters.AsNoTracking().FirstOrDefaultAsync(c => c.Name == name, cancellationToken);

        public Task<List<DbCharacter>> GetByAccountIdAsync(int accountId, CancellationToken cancellationToken = default) =>
            this._context.Characters
                .Include(c => c.Items)
                .AsNoTracking()
                .Where(c => c.AccountId == accountId)
                .ToListAsync(cancellationToken);

        public async Task CreateAsync(DbCharacter character, CancellationToken cancellationToken = default)
        {
            this._context.Characters.Add(character);
            await this._context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateAsync(DbCharacter character, CancellationToken cancellationToken = default)
        {
            this._context.Characters.Update(character);
            await this._context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            DbCharacter? character = await this._context.Characters.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
            if (character == null) return;

            this._context.Characters.Remove(character);
            await this._context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}

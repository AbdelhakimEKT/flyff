using System.Threading;
using System.Threading.Tasks;
using Hellion.Core.Database;
using Microsoft.EntityFrameworkCore;

namespace Hellion.Core.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly DatabaseContext _context;

        public AccountRepository(DatabaseContext context)
        {
            this._context = context;
        }

        public Task<DbUser?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            this._context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        public Task<DbUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
            this._context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

        public Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
            this._context.Users.AsNoTracking().AnyAsync(u => u.Username == username, cancellationToken);

        public async Task CreateAsync(DbUser account, CancellationToken cancellationToken = default)
        {
            this._context.Users.Add(account);
            await this._context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}

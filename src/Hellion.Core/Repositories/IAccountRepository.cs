using System.Threading;
using System.Threading.Tasks;
using Hellion.Core.Database;

namespace Hellion.Core.Repositories
{
    public interface IAccountRepository
    {
        Task<DbUser?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<DbUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

        Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default);

        Task CreateAsync(DbUser account, CancellationToken cancellationToken = default);
    }
}

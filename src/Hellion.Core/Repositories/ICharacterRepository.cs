using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hellion.Core.Database;

namespace Hellion.Core.Repositories
{
    public interface ICharacterRepository
    {
        Task<DbCharacter?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<DbCharacter?> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken = default);

        Task<DbCharacter?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

        Task<List<DbCharacter>> GetByAccountIdAsync(int accountId, CancellationToken cancellationToken = default);

        Task CreateAsync(DbCharacter character, CancellationToken cancellationToken = default);

        Task UpdateAsync(DbCharacter character, CancellationToken cancellationToken = default);

        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hellion.Core.Database;

namespace Hellion.Core.Repositories
{
    public interface IItemRepository
    {
        Task<DbItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<List<DbItem>> GetByCharacterIdAsync(int characterId, CancellationToken cancellationToken = default);

        Task CreateAsync(DbItem item, CancellationToken cancellationToken = default);

        Task UpdateAsync(DbItem item, CancellationToken cancellationToken = default);

        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}

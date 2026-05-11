using Hellion.Core.Database;
using Microsoft.EntityFrameworkCore;

namespace Hellion.Core.Repositories
{
    /// <summary>
    /// Synchronous repository kept for the static <see cref="DatabaseService"/> shim used
    /// by legacy packet handlers. New code should use <see cref="IItemRepository"/>.
    /// </summary>
    public class LegacyItemRepository : RepositoryBase<DbItem>
    {
        public LegacyItemRepository(DbContext dbContext)
            : base(dbContext)
        {
        }
    }
}

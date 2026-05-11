using Hellion.Core.Database;
using Microsoft.EntityFrameworkCore;

namespace Hellion.Core.Repositories
{
    /// <summary>
    /// Synchronous repository kept for the static <see cref="DatabaseService"/> shim used
    /// by legacy packet handlers. New code should use <see cref="ICharacterRepository"/>.
    /// </summary>
    public class LegacyCharacterRepository : RepositoryBase<DbCharacter>
    {
        public LegacyCharacterRepository(DbContext dbContext)
            : base(dbContext)
        {
        }
    }
}

using Hellion.Core.Database;
using Microsoft.EntityFrameworkCore;

namespace Hellion.Core.Repositories
{
    public class CharacterRepository : RepositoryBase<DbCharacter>
    {
        public CharacterRepository(DbContext dbContext)
            : base(dbContext)
        {
        }

        
    }
}

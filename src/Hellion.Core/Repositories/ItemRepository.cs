using Hellion.Core.Database;
using Microsoft.EntityFrameworkCore;

namespace Hellion.Core.Repositories
{
    public class ItemRepository : RepositoryBase<DbItem>
    {
        public ItemRepository(DbContext dbContext)
            : base(dbContext)
        {
        }
    }
}

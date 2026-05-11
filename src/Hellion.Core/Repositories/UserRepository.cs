using Hellion.Core.Database;
using Microsoft.EntityFrameworkCore;

namespace Hellion.Core.Repositories
{
    public class UserRepository : RepositoryBase<DbUser>
    {
        public UserRepository(DbContext dbContext)
            : base(dbContext)
        {
        }
    }
}

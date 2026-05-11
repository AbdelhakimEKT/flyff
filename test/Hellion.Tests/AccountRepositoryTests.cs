using Hellion.Core.Configuration;
using Hellion.Core.Database;
using Hellion.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hellion.Tests
{
    public class AccountRepositoryTests
    {
        private static DatabaseContext NewContext()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase($"hellion-{System.Guid.NewGuid()}")
                .Options;
            return new DatabaseContext(options);
        }

        [Fact]
        public async Task GetByUsernameAsync_returns_matching_user()
        {
            await using DatabaseContext db = NewContext();
            db.Users.Add(new DbUser { Username = "alice", Password = "h", Authority = 1 });
            db.Users.Add(new DbUser { Username = "bob", Password = "h", Authority = 1 });
            await db.SaveChangesAsync();

            IAccountRepository repo = new AccountRepository(db);
            DbUser? result = await repo.GetByUsernameAsync("alice");

            Assert.NotNull(result);
            Assert.Equal("alice", result!.Username);
        }

        [Fact]
        public async Task GetByUsernameAsync_returns_null_for_missing_user()
        {
            await using DatabaseContext db = NewContext();
            IAccountRepository repo = new AccountRepository(db);

            DbUser? result = await repo.GetByUsernameAsync("ghost");

            Assert.Null(result);
        }

        [Fact]
        public async Task ExistsByUsernameAsync_is_case_sensitive_in_memory_provider()
        {
            await using DatabaseContext db = NewContext();
            db.Users.Add(new DbUser { Username = "Alice", Password = "h", Authority = 1 });
            await db.SaveChangesAsync();

            IAccountRepository repo = new AccountRepository(db);

            Assert.True(await repo.ExistsByUsernameAsync("Alice"));
        }

        [Fact]
        public async Task CreateAsync_persists_a_new_user_and_stamps_timestamps()
        {
            await using DatabaseContext db = NewContext();
            IAccountRepository repo = new AccountRepository(db);

            await repo.CreateAsync(new DbUser { Username = "charlie", Password = "h", Authority = 1 });

            DbUser? saved = await db.Users.AsNoTracking().FirstAsync(u => u.Username == "charlie");
            Assert.NotEqual(default, saved.CreatedAt);
            Assert.NotEqual(default, saved.UpdatedAt);
        }
    }
}

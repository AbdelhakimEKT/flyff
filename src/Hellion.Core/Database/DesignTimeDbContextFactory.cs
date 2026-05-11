using Hellion.Core.Configuration;
using Microsoft.EntityFrameworkCore.Design;

namespace Hellion.Core.Database
{
    /// <summary>
    /// Used by EF Core CLI tooling (<c>dotnet ef migrations</c>, <c>dotnet ef database update</c>)
    /// to instantiate a <see cref="DatabaseContext"/> at design time without a real DI host.
    /// Reads its connection settings from <c>HELLION_DB_*</c> environment variables when set,
    /// falling back to safe local defaults.
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args)
        {
            var config = new DatabaseConfiguration
            {
                Ip = System.Environment.GetEnvironmentVariable("HELLION_DB_HOST") ?? "127.0.0.1",
                User = System.Environment.GetEnvironmentVariable("HELLION_DB_USER") ?? "root",
                Password = System.Environment.GetEnvironmentVariable("HELLION_DB_PASSWORD") ?? string.Empty,
                DatabaseName = System.Environment.GetEnvironmentVariable("HELLION_DB_NAME") ?? "hellion",
            };

            return new DatabaseContext(config);
        }
    }
}

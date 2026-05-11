using Hellion.Core.Configuration;
using Hellion.Core.Database;
using Hellion.Core.IO;
using Hellion.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Hellion.Cluster
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Initialize("Cluster", "logs/cluster-.log");
            try
            {
                var builder = Host.CreateApplicationBuilder(args);

                builder.Services.Configure<ClusterConfiguration>(builder.Configuration.GetSection("Cluster"));
                builder.Services.Configure<DatabaseConfiguration>(builder.Configuration.GetSection("Database"));

                builder.Services.AddDbContext<DatabaseContext>((sp, options) =>
                {
                    var dbConfig = sp.GetRequiredService<IOptions<DatabaseConfiguration>>().Value;
                    var connStr = $"server={dbConfig.Ip};userid={dbConfig.User};pwd={dbConfig.Password};port=3306;database={dbConfig.DatabaseName};sslmode=none;CharSet=utf8mb4;";
                    options.UseMySql(connStr, new MySqlServerVersion(new System.Version(8, 0, 21)));
                });

                builder.Services.AddScoped<IAccountRepository, AccountRepository>();
                builder.Services.AddScoped<ICharacterRepository, CharacterRepository>();
                builder.Services.AddScoped<IItemRepository, ItemRepository>();

                builder.Services.AddSingleton<ClusterServer>();

                using var host = builder.Build();
                var server = host.Services.GetRequiredService<ClusterServer>();
                server.Start();
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}

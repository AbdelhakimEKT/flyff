using Hellion.Core.Configuration;
using Hellion.Core.Database;
using Hellion.Core.IO;
using Hellion.Core.Network;
using Hellion.Core.Repositories;
using Hellion.World.Handlers;
using Hellion.World.Packets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Hellion.World
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Initialize("World", "logs/world-.log");
            try
            {
                var builder = Host.CreateApplicationBuilder(args);

                builder.Services.Configure<WorldConfiguration>(builder.Configuration.GetSection("World"));
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

                builder.Services.AddSingleton<PacketRouter>();
                builder.Services.AddScoped<IPacketHandler<JoinPacket>, JoinHandler>();
                builder.Services.AddScoped<IPacketHandler<MovePacket>, MoveHandler>();
                builder.Services.AddScoped<IPacketHandler<ChatPacket>, ChatHandler>();

                builder.Services.AddSingleton<WorldServer>();

                using var host = builder.Build();
                var server = host.Services.GetRequiredService<WorldServer>();
                server.Start();
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}

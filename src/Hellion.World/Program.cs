using Hellion.Core.Configuration;
using Hellion.Core.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

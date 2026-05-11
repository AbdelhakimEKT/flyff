using Hellion.Core.Configuration;
using Hellion.Core.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

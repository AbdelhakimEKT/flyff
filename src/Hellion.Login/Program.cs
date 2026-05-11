using Hellion.Core.Configuration;
using Hellion.Core.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Hellion.Login
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Initialize("Login", "logs/login-.log");
            try
            {
                var builder = Host.CreateApplicationBuilder(args);

                builder.Services.Configure<LoginConfiguration>(builder.Configuration.GetSection("Login"));
                builder.Services.Configure<DatabaseConfiguration>(builder.Configuration.GetSection("Database"));
                builder.Services.AddSingleton<LoginServer>();

                using var host = builder.Build();
                var server = host.Services.GetRequiredService<LoginServer>();
                server.Start();
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}

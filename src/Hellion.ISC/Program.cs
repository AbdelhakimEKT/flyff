using Hellion.Core.Configuration;
using Hellion.Core.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Hellion.ISC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Initialize("ISC", "logs/isc-.log");
            try
            {
                var builder = Host.CreateApplicationBuilder(args);

                builder.Services.Configure<ISCConfiguration>(builder.Configuration.GetSection("Isc"));
                builder.Services.AddSingleton<InterServer>();

                using var host = builder.Build();
                var server = host.Services.GetRequiredService<InterServer>();
                server.Start();
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}

using System;
using System.Text;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Hellion.Core.IO
{
    public static class Log
    {
        private static ILogger _logger = Serilog.Log.Logger;
        private static bool _initialized;

        static Log()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        /// <summary>
        /// Initialize the Serilog logger with the given server name.
        /// The chosen format is: <c>HH:mm:ss.fff | LEVEL | Server | message</c>.
        /// </summary>
        public static void Initialize(string serverName, string? logFilePath = null)
        {
            var template = "{Timestamp:HH:mm:ss.fff} | {Level:u3} | {Server} | {Message:lj}{NewLine}{Exception}";

            var config = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithProperty("Server", serverName)
                .WriteTo.Console(outputTemplate: template);

            if (!string.IsNullOrWhiteSpace(logFilePath))
            {
                config = config.WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day, outputTemplate: template);
            }

            Serilog.Log.Logger = config.CreateLogger();
            _logger = Serilog.Log.Logger;
            _initialized = true;
        }

        public static void Info(string format, params object[] args) =>
            Write(LogEventLevel.Information, format, args);

        public static void Done(string format, params object[] args) =>
            Write(LogEventLevel.Information, format, args);

        public static void Warning(string format, params object[] args) =>
            Write(LogEventLevel.Warning, format, args);

        public static void Error(string format, params object[] args) =>
            Write(LogEventLevel.Error, format, args);

        public static void Debug(string format, params object[] args)
        {
#if DEBUG
            Write(LogEventLevel.Debug, format, args);
#endif
        }

        public static void CloseAndFlush()
        {
            if (_initialized)
            {
                Serilog.Log.CloseAndFlush();
            }
        }

        private static void Write(LogEventLevel level, string format, params object[] args)
        {
            string message;
            try
            {
                message = args.Length == 0 ? format : string.Format(format, args);
            }
            catch (FormatException)
            {
                message = format;
            }
            _logger.Write(level, "{Text}", message);
        }
    }
}

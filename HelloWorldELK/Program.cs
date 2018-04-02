using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace HelloWorldElk
{
    public interface ILoggingService
    {
        void Configure();
    }

    public class LoggingService : ILoggingService
    {
        public LoggingService()
        {
        }

        public void Configure()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("TimeStampUtc", DateTime.UtcNow)
                .Enrich.WithProperty("TimeStampLocal", DateTime.Now)
                .Enrich.WithProperty("AppServer", Environment.MachineName.ToUpper())
                .Enrich.WithProperty("AppService", Assembly.GetExecutingAssembly().GetName().Name)
                .Enrich.WithProperty("AppVersion", Assembly.GetExecutingAssembly().GetName().Version.ToString())
                .Enrich.WithProperty("AppUserName", $"{Environment.UserDomainName}\\{Environment.UserName.ToUpper()}")
                .Destructure.ToMaximumDepth(10)
                .MinimumLevel.Verbose()
                .ReadFrom.AppSettings()

                //log to console when its available
                .WriteTo.Console(LogEventLevel.Information)

                .CreateLogger();

            var logger = Log.ForContext<LoggingService>();
            logger.Information("Logging service started and sending to preferred sinks {console, elasticsearch}.");
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var loggingService = new LoggingService();
            loggingService.Configure();

            while (true)
            {
                Log.Debug("Hello world from ELK + Serilog");
                Log.Information("Hello world from ELK + Serilog");
                Log.Warning("Hello world from ELK + Serilog");
                Log.Error(new Exception("test"), "Hello world from ELK + Serilog");

                var context = new { stringField = "Main", integerField = 150, decimalField = 257.25946, booleanField = true, compositeField = new { x = 125.287, y = 254.345 }};
                Log.Information($"Logging with more context. Data: {@context}", context);

                Thread.Sleep(TimeSpan.FromSeconds(5));
            }

        }
    }
}

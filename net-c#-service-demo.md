#### Serilog/ElasticSearch Strucutured Logging

The fastest way to the reap full benefits of ES in your .NET service is starting logging responsibly and use Serilog. Serilog is a structured logging framework where log carries structured contextual information or payload. This is not tutorial on Serilog, instead I advise you start reading [Serilog Wiki](https://github.com/serilog/serilog).

#### HellWorldELK

1. Create a console app C# project

2. Add the following packages

```csharp
Install-Package Serilog
Install-Package Serilog.Sinks.Console
Install-Package Serilog.Settings.AppSettings
Install-Package Serilog.Sinks.Elasticsearch
```

3. Add AppSettings section

```csharp
  <appSettings>
    <add key="serilog:using" value="Serilog.Sinks.Elasticsearch" />
    <add key="serilog:write-to:Elasticsearch.nodeUris" value="http://ardilabsserverelk.eastus.cloudapp.azure.com:9200/" />
    <add key="serilog:write-to:Elasticsearch.indexFormat" value="hello-world-elk-dev-{0:yyyy.MM}" />
    <add key="serilog:write-to:Elasticsearch.templateName" value="serilog-events-template" />
  </appSettings>
```

4. Create logging configurator

```csharp
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
```
	5. Write some test logs

```csharp
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
```

6. Run the service for at least 1 minute

![](https://github.com/rdagumampan/elasticsearch-windows-server-cookbook/blob/master/screenshot-demo-csharp-project.PNG "")

7. Check our indices and logs

![](https://github.com/rdagumampan/elasticsearch-windows-server-cookbook/blob/master/screenshot-demo-csharp-project-head.PNG "")

8. Check with Elastic HQ on status of our clusters

![](https://github.com/rdagumampan/elasticsearch-windows-server-cookbook/blob/master/screenshot-demo-csharp-project-hq.PNG "")

9. Visualize in Kibana

![](https://github.com/rdagumampan/elasticsearch-windows-server-cookbook/blob/master/screenshot-demo-csharp-project-kibana-01.PNG "")

![](https://github.com/rdagumampan/elasticsearch-windows-server-cookbook/blob/master/screenshot-demo-csharp-project-kibana-02.PNG "")

![](https://github.com/rdagumampan/elasticsearch-windows-server-cookbook/blob/master/screenshot-demo-csharp-project-kibana-03.PNG "")

10. Go to Management > Advanced Settings
MM-DD-YYYY HH:mm:ss

![](https://github.com/rdagumampan/elasticsearch-windows-server-cookbook/blob/master/screenshot-demo-csharp-project-kibana-04.PNG "")

#### References:

v0.10

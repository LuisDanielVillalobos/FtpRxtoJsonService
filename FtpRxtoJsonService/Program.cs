using FtpRxtoJsonService;
using FtpRxtoJsonService.css;
using Microsoft.Extensions.Logging.EventLog;
using static System.Runtime.InteropServices.JavaScript.JSType;

IHost host = Host.CreateDefaultBuilder(args)
.UseWindowsService()
.ConfigureLogging((context, logging) =>
{
    logging.ClearProviders();
    logging.AddConsole();

    logging.AddEventLog(settings =>
    {
        settings.SourceName = "FtpRxtoJsonService";
        settings.Filter = (source, level) => level >= LogLevel.Information; 
    });
    logging.SetMinimumLevel(LogLevel.Information);
})
.ConfigureServices((hostContext, services) =>
{
    services.AddHostedService<Worker>();
})
.Build();

host.Run();
/*    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .ConfigureLogging((context, logging) =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.AddEventLog(context.Configuration.GetSection("Logging:EventLog").Get<EventLogSettings>());
            logging.SetMinimumLevel(LogLevel.Information);
        });
    var builder = Host.CreateApplicationBuilder(args);
    builder.Services.AddWindowsService();
    builder.Services.AddHostedService<Worker>();
    var host = builder.Build();

    host.Run();*/

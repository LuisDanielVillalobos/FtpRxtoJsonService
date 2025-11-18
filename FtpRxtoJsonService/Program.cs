using FtpRxtoJsonService;
using FtpRxtoJsonService.css;
using Microsoft.Extensions.Logging.EventLog;
using static System.Runtime.InteropServices.JavaScript.JSType;
using CliWrap;

const string ServiceName = "RxtoJsonConverter";

if (args is { Length: 1 })
{
    try
    {
        string executablePath = "C:\\Users\\luis3\\OneDrive\\Documentos\\Visual Studio 2022\\PVC\\FtpRxtoJsonService\\Precompiled\\FtpRxtoJsonService.exe";
            Path.Combine(AppContext.BaseDirectory, "App.WindowsService.exe");

        if (args[0] is "/Install")
        {
            await Cli.Wrap("sc")
                .WithArguments(new[] { "create", ServiceName, $"binPath={executablePath}", "start=auto" })
                .ExecuteAsync();
        }
        else if (args[0] is "/Uninstall")
        {
            await Cli.Wrap("sc")
                .WithArguments(new[] { "stop", ServiceName })
                .ExecuteAsync();

            await Cli.Wrap("sc")
                .WithArguments(new[] { "delete", ServiceName })
                .ExecuteAsync();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }

    return;
}
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

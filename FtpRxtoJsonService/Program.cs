using System.Net;
using System.Net.Mail;
using System.Text;
using CliWrap;
using FtpRxtoJsonService;
using FtpRxtoJsonService.css;
using Microsoft.Extensions.Logging.EventLog;
using static System.Runtime.InteropServices.JavaScript.JSType;

const string ServiceName = "RxtoJsonConverter";


if (args is { Length: 1 })
{
    try
    {
        string ruta = Directory.GetCurrentDirectory();
        string parent = Directory.GetParent(ruta).FullName;
        string executablePath = Path.Combine(parent, "Precompiled", "FtpRxtoJsonService.exe");
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

public class WorkerLogger
{

    public ILogger _logger { get; set; }
    public IConfiguration _configuration { get; set; }
    public string EmailFrom { get; set; }
    public string EmailTo { get; set; }
    public string EmailPass { get; set; }


    public void LogInformation(string message)
    {
        _logger.LogInformation(message);
    }
    public void LogWarning(string message)
    {
        _logger.LogWarning(message);
    }
    public void LogError(Exception error, string filename, string processing = "processing")
    {
        _logger.LogError(error.Message);
        using (var client = new SmtpClient())
        {
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(EmailFrom, EmailPass);
            using (var message = new MailMessage(
                from: new MailAddress(EmailFrom, "OCUCO Service"),
                to: new MailAddress(EmailTo, "Usuario")
                ))
            {
                message.Subject = $"Error {processing} file: {filename}";
                var Body = new StringBuilder();
                Body.Append("An error has occurred when the service tried to process the file.\n");
                Body.Append($"The error is: {error.Message}\n\n");
                Body.Append("The The job won't be processed until fixing the error. Please contact IT");
                message.Body = Body.ToString();
                client.Send(message);
            }
        }
    }
    public void LogOtherError(Exception error, string messages, string processing = "processing")
    {
        _logger.LogError(error.Message);
        using (var client = new SmtpClient())
        {
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(EmailFrom, EmailPass);
            using (var message = new MailMessage(
                from: new MailAddress(EmailFrom, "OCUCO Service"),
                to: new MailAddress(EmailTo, "Usuario")
                ))
            {
                message.Subject = $"Error {processing} {messages}";
                var Body = new StringBuilder();
                Body.Append("An error has occurred when the service was running.\n");
                Body.Append($"The error is: {error.Message}\n\n");
                Body.Append("The The job won't be processed until fixing the error. Please contact IT");
                message.Body = Body.ToString();
                client.Send(message);
            }
        }
    }
}

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

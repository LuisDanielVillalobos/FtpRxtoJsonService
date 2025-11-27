using System.Data;
using System.Net;
using System.Net.Mail;
using System.Text;
using FtpRxtoJsonService.css;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FtpRxtoJsonService
{
    public class Worker : BackgroundService
    {
        public readonly ILogger<Worker> _logger;
        public readonly IConfiguration _configuration;
        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            WorkerLogger WorkerLogger = new WorkerLogger
            {
                _logger = _logger,
                _configuration = _configuration,
                EmailFrom = _configuration["Email:EmailFrom"],
                EmailTo = _configuration["Email:EmailTo"],
                EmailPass = _configuration["Email:EmailPass"]
            };
            //parametros
            FtpServers ftpServers = new FtpServers { connectionString = WorkerLogger._configuration["WebOrders:connectionString"] };
            WebRequestGet ftpDetails = new WebRequestGet
            {
                ftpServer = WorkerLogger._configuration["ftpdata:ftpServer"],
                ftpUsername = WorkerLogger._configuration["ftpdata:ftpUsername"],
                ftpPassword = WorkerLogger._configuration["ftpdata:ftpPassword"],
                localpath = WorkerLogger._configuration["localpath"]
            };
            string processpath = WorkerLogger._configuration["ftpdata:processpath"];
            string errorpath = WorkerLogger._configuration["ftpdata:errorpath"];
            string conectionstring = WorkerLogger._configuration["WebOrders:connectionString"];
            string localpath = ftpDetails.localpath;

            while (!stoppingToken.IsCancellationRequested)
            {
                if (WorkerLogger._logger.IsEnabled(LogLevel.Information))
                {
                    WorkerLogger.LogInformation($"Running at: {DateTimeOffset.Now}");
                    try
                    {
                        //servidores a los que se mandará la informacion y jobfolder de donde se sacará
                        var servers = ftpServers.GetServers();
                        foreach (var server in servers)
                        {
                            bool isJF = false;
                            bool isRx = false;
                            //verificamos si existe el jobfolder para trabjar con el
                            if (ftpDetails.IsFileOnDir(server.jobfolder.Trim()))
                            {
                                isJF = true;
                            }

                            if (isJF)
                            {
                                //verificamos si existen archivos rx
                                string[] fileslist = ftpDetails.GetRxFileName(server.jobfolder);
                                if (fileslist.Count() > 0)
                                    isRx = true;
                                if (isRx)
                                {
                                    //logica
                                    int i = 0;
                                    while (i <= fileslist.Count() - 1)
                                    {
                                        string filepath = server.jobfolder + "/" + fileslist[i];
                                        //inciamos la clase para el manejo del arcihvo
                                        ProcessFile processFile = new ProcessFile
                                        {
                                            conectionstring = conectionstring,
                                            arcName = fileslist[i],
                                            ftpDetails = ftpDetails,
                                            _logger = WorkerLogger,
                                            clientserver = new WebRequestGet
                                            {
                                                ftpServer = server.name,
                                                ftpUsername = server.username,
                                                ftpPassword = server.password,
                                            }
                                        };
                                        try
                                        {
                                            if (processFile.ProcessRx(filepath))
                                                if (processFile.BuildOrder(localpath))
                                                    if (processFile.InsertOrder())
                                                        processFile.ClientOrder(filepath, server.jobfolder + "/" + processpath, localpath);
                                        }
                                        catch (Exception ex)
                                        {
                                            WorkerLogger.LogError(ex, processFile.arcName, "unexpected, processing order");
                                        }
                                        i++;
                                        //borrar archivos locales
                                        string[] filePaths = Directory.GetFiles(localpath);
                                        foreach (string filePath in filePaths)
                                            File.Delete(filePath);
                                    }
                                    //Errores en archivos
                                    var errorfiles = new List<string>();
                                    foreach (string files in fileslist)
                                    {
                                        string[] temp = ftpDetails.GetRxFileName(server.jobfolder);
                                        if (temp == null)
                                            break;
                                        foreach (string file in temp)
                                        {
                                            //si los archivos no se han movido, tuvieron un error
                                            if (file == files)
                                            {
                                                errorfiles.Add(files);
                                            }
                                        }
                                    }
                                    if (errorfiles != null)
                                    {
                                        foreach (string file in errorfiles)
                                        {
                                            try
                                            {
                                                WorkerLogger.LogWarning("Problemas con archivo: " + file);
                                                // mover a la carpeta para revisar despues
                                                ftpDetails.MoveFtpFile(server.jobfolder + "/" + file, server.jobfolder + "/" + errorpath + file);
                                            } catch (Exception ex) {WorkerLogger.LogError(ex.InnerException, file, ex.Message); }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e) { WorkerLogger.LogOtherError(e, "in the overall process", "unexpected"); }
                    WorkerLogger.LogInformation("Fin del procesamiento");
                }
                await Task.Delay(20000, stoppingToken);
            }
        }
    }
}

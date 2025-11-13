using FtpRxtoJsonService.css;

namespace FtpRxtoJsonService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //parametros
            FtpServers ftpServers = new FtpServers { connectionString = _configuration["WebOrders:connectionString"] };
            WebRequestGet ftpDetails = new WebRequestGet
            {
                ftpServer = _configuration["ftpdata:ftpServer"],
                ftpUsername = _configuration["ftpdata:ftpUsername"],
                ftpPassword = _configuration["ftpdata:ftpPassword"],
                _logger = _logger
            };
            string processpath = _configuration["ftpdata:processpath"];
            string errorpath = _configuration["ftpdata:errorpath"];
            string conectionstring = _configuration["WebOrders:connectionString"];

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Running at: {time}", DateTimeOffset.Now);
                    //servidores a los que se mandará la informacion y jobfolder de donde se sacará
                    var servers = ftpServers.GetServers();
                    foreach (var server in servers)
                    {
                        bool isJF = false;
                        bool isRx = false;
                        foreach (string dir in ftpDetails.ListDirectories())
                        {
                            //verificamos si existe el jobfolder para trabjar con el
                            if (dir == server.jobfolder.Trim())
                            {
                                isJF = true;
                            }
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
                                    //inciamos la clase para el manejo del arcihvo
                                    ProcessFile processFile = new ProcessFile
                                    {
                                        arcName = fileslist[i],
                                        filepath = server.jobfolder + "/" + fileslist[i],
                                        ftpDetails = ftpDetails,
                                        _logger = _logger,
                                        processpath = server.jobfolder + "/" + processpath,
                                        order_data = new WebOrders { connectionString = conectionstring },
                                        clientserver = new WebRequestGet
                                        {
                                            ftpServer = server.name,
                                            ftpUsername = server.username,
                                            ftpPassword = server.password,
                                            _logger = _logger
                                        }

                                    };
                                    if (processFile.ProcessRx())
                                        if (processFile.BuildOrder())
                                        {
                                            try
                                            {
                                                processFile.InsertOrder();
                                            }
                                            catch (Exception ex)
                                            {
                                                _logger.LogError(ex.Message);
                                            }
                                        }
                                    i++;
                                }
                                //Errores en archivos
                                var errorfiles = new List<string>();
                                foreach (string files in fileslist)
                                {
                                    string[] temp = ftpDetails.GetRxFileName(server.jobfolder);
                                    if (temp == null)
                                        break;
                                    foreach (string file in temp)
                                    {//si los archivos no se han movido, tuvieron un error
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
                                        _logger.LogWarning("Problemas con archivo: " + file);
                                        // mover a la carpeta para revisar despues
                                        ftpDetails.MoveFtpFile(server.jobfolder + "/" + file, server.jobfolder + "/" + errorpath + file);
                                    }
                                }
                            }

                        }
                    }
                    _logger.LogInformation("Fin del procesamiento");

                }
                await Task.Delay(20000, stoppingToken);
            }
        }
    }
}

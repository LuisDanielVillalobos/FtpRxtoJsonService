using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Azure;

namespace FtpRxtoJsonService.css
{

    public class WebRequestGet
    {

        public string ftpServer { get; set; }
        public string ftpUsername { get; set; }
        public string ftpPassword { get; set; }
        public string localpath { get; set; }


        public string Download(string path)
        {
            string localFile = localpath + "downloaded_file.rx";
            try
            {
                // Crear la solicitud FTP
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpServer + path);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
                // Obtener respuesta del servidor
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                using (FileStream outputFile = new FileStream(localFile, FileMode.Create, FileAccess.Write))
                {
                    // Copiar bytes del FTP al archivo local
                    responseStream.CopyTo(outputFile);
                }
                // Devuelves la ruta local del archivo descargado
                return localFile;
            }
            catch (WebException ex)
            {
                throw new Exception("downloading", ex);
            }
        }
        public async Task Upload(string localFilePath, string remotePath)//remotePath es tanto donde se guarda como el nombre del archivo
        {
            try
            {
                // remotePath debe incluir el nombre de archivo remoto (ej: "processed/98765-5-5.rx")
                var request = (FtpWebRequest)WebRequest.Create(ftpServer + remotePath);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

                // Opciones recomendadas
                request.UseBinary = true;
                request.UsePassive = true;
                request.KeepAlive = false; // cerrar la conexión al terminar

                // Abrir el archivo local y escribir al stream del request (async)
                using (FileStream fileStream = File.Open(localFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (Stream requestStream = await request.GetRequestStreamAsync())
                {
                    await fileStream.CopyToAsync(requestStream);
                }

                // Obtener la respuesta (síncrono en la API, pero rápido)
                FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync();

            }
            catch (WebException ex)
            {
                throw new Exception("uploading", ex);
            }
        }
        public bool MoveFtpFile(string source, string destination)
        {
            try
            {
                int pos = destination.LastIndexOf('/');
                string ruta = destination.Substring(0, pos + 1);
                string archivo = destination.Substring(pos + 1);
                if (IsFileOnDir(archivo, ruta))
                {
                    Random rng = new Random();
                    string name = Path.GetFileNameWithoutExtension(destination);
                    string ext = Path.GetExtension(destination);
                    int rndom = rng.Next(1000);
                    destination = ruta + name + "-" + rndom.ToString() + ext;
                }
                Uri uriSource = new Uri(this.ftpServer + source, UriKind.Absolute);
                Uri uriDestination = new Uri(this.ftpServer + destination, UriKind.Absolute);

                // Do the files exist?
                /*            if (!FtpFileExists(uriSource.AbsolutePath))
                            {
                                throw (new FileNotFoundException(string.Format("Source '{0}' not found!", uriSource.AbsolutePath)));
                            }
                */
                Uri targetUriRelative = uriSource.MakeRelativeUri(uriDestination);
                //perform rename
                var ftp = (FtpWebRequest)WebRequest.Create(uriSource.AbsoluteUri);
                ftp.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
                ftp.Method = WebRequestMethods.Ftp.Rename;
                ftp.RenameTo = Uri.UnescapeDataString(targetUriRelative.OriginalString);
                FtpWebResponse response = (FtpWebResponse)ftp.GetResponse();

                return true;
            }
            catch (Exception e) { throw new Exception("moving ftp", e); }
        }

        public void Delete(string path)
        {
            //
        }
        public string[] ListDirectories(string dirpath = "")
        {
            string[] dir;
            var content = new List<string>();
            // Crear la solicitud FTP
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpServer + dirpath);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
            request.UsePassive = true;

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            using (Stream responseStream = response.GetResponseStream())
            {
                // Lee la respuesta y la escribe en la cadena de salida
                StreamReader list = new StreamReader(responseStream);
                foreach (string line in list.ReadToEnd().Split('\n'))
                {
                    content.Add(line.Trim());

                }
                dir = content.ToArray();
            }
            return dir;
        }
        public bool IsFileOnDir(string file, string dirpath = "")
        {
            try
            {
                foreach (string dir in ListDirectories(dirpath))
                {
                    if (file == dir)
                        return true;
                }
                return false;
            }
            catch (Exception e) { throw new Exception("searching", e); }
        }
        public string[] GetRxFileName(string dirpath = "")
        {
            try
            {
                string[] dirList = ListDirectories(dirpath);
                var rxfiles = new List<string>();
                // Dividir por salto de línea y limpiar espacios
                //string[] files = dirList.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                // Filtrar solo los que terminen con ".rx"
                foreach (string file in dirList)
                {
                    if (file.EndsWith(".rx"))
                        rxfiles.Add(file);
                }
                string[] rx = rxfiles.ToArray();
                return rx;
            }
            catch (Exception e) { throw new Exception("getting rx", e); }
        }
    }

}

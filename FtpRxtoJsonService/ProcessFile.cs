using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using FtpRxtoJsonService;
using Microsoft.Identity.Client;


namespace FtpRxtoJsonService.css
{
    internal class ProcessFile
    {
        private List<string> lineas = new();
        private List<string> dprovider = new();
        private List<string> frame = new();
        private List<string> lensod = new();
        private List<string> lensos = new();
        private List<string> rxod = new();
        private List<string> rxos = new();
        private List<List<string>> item_list = new();
        private string archivo = "";
        Root? order = null;
        public string conectionstring { get; set; }
        public string arcName { get; set; }
        public WorkerLogger _logger { get; set; }
        public WebRequestGet ftpDetails { get; set; }
        public WebRequestGet clientserver { get; set; }
        //lee el archivo hashref
        public bool ProcessRx(string filepath)
        {
            string line;
            try
            {
                string fs = ftpDetails.Download(filepath);
                StreamReader sr = new StreamReader(fs);
                //Lee la primera linea de texto
                line = sr.ReadLine();
                bool insideOrder = false;
                bool item = false;
                List<string> items = null;
                //lee hasta encontrar el inicio de una orden
                while (line != null)
                {
                    //si encuentra el inicio de una orden
                    if (line.Contains("start_order"))
                    {
                        insideOrder = true;
                        break;
                    }
                    line = sr.ReadLine();
                }
                if (insideOrder == false)
                {
                    sr.Close();
                    throw new Exception("No se encontro una orden en el archivo.");
                }
                //lee hasta el final de la orden
                while (insideOrder && line != null)
                {
                    while (item)
                    {
                        //si contiene campo:valor reemplaza para agregar las comillas "campo":"valor"
                        line = ParseString(line);
                        items.Add(line);
                        line = sr.ReadLine();
                        if (line.Contains("item_end"))
                        {
                            item = false;
                            item_list.Add(items);
                        }
                    }
                    //si contiene campo:valor
                    if (line.Contains(':'))
                    {
                        //si contiene campo:valor remplaza para agregar las comillas "campo":"valor"

                        line = ParseString(line);
                        //comprueba si pertenece a una subclase o no
                        if (line.Contains("lens_od"))
                        { lensod.Add(line); }
                        else if (line.Contains("lens_os"))
                        { lensos.Add(line); }
                        else if (line.Contains("rx_od"))
                        { rxod.Add(line); }
                        else if (line.Contains("rx_os"))
                        { rxos.Add(line); }
                        else if (line.Contains("frame"))
                        { frame.Add(line); }
                        else if (line.Contains("dprovider"))
                        { dprovider.Add(line); }

                        else lineas.Add(line);
                    }
                    //si encontro un item
                    else if (line.Contains("item_start"))
                    {
                        item = true;
                        items = new List<string>();
                    }
                    //final de una order
                    else if (line.Contains("end_order"))
                        insideOrder = false;
                    //siguiente linea
                    line = sr.ReadLine();
                }
                //cierra el archivo
                sr.Close();
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, arcName, "reading Rx");
                return false;
            }
        }
        //escribe el archivo json
        public bool BuildOrder(string localpath)
        {
            try
            {
                archivo = arcName;
                archivo = archivo.Remove(arcName.Length - 3);
                //Pass the filepath and filename to the StreamWriter Constructor
                StreamWriter sw = new StreamWriter(localpath + archivo + "_local.json");
                //inicio del archivo json root
                sw.WriteLine("{");
                sw.WriteLine("\"order\": {");
                //cada seccion de codigo abre {} para delimitar la seccion y escribe todos los campos
                //maneja tambien las comas entre campos y secciones 
                int i = 0;
                while (i < lineas.Count)
                {
                    //remplazamos : por ": " para formar el formato json
                    sw.WriteLine(lineas[i] + ",");
                    i++;
                }
                WriteSection(sw, "dprovider", dprovider);
                WriteSection(sw, "frame", frame);
                WriteSection(sw, "lens_od", lensod);
                WriteSection(sw, "lens_os", lensos);
                WriteSection(sw, "rx_od", rxod);
                WriteSection(sw, "rx_os", rxos);
                //inicio y fin de items
                sw.WriteLine("\"items\": [\n{");
                i = 0;
                while (i < item_list.Count)
                {
                    int j = 0;
                    while (j < item_list[i].Count)
                    {
                        if (j == item_list[i].Count - 1)
                            sw.WriteLine("\t" + item_list[i][j]);
                        else
                            sw.WriteLine("\t" + item_list[i][j] + ',');
                        j++;
                    }
                    if (i < item_list.Count - 1)
                        sw.WriteLine("},\n {");
                    i++;
                }
                sw.WriteLine("}\n]");
                //fin de la orden
                sw.WriteLine("}\n}");
                sw.Close();
                string jsonString = File.ReadAllText(localpath + archivo + "_local.json");
                order = JsonSerializer.Deserialize<Root>(jsonString)!;
                return true;

            }
            catch (Exception e)
            {
                _logger.LogError(e, arcName, "parsing order");
                return false;
            }
        }
        //procesa la orden
        public bool InsertOrder()
        {
            try
            {
                WebOrders order_data = new WebOrders
                {
                    connectionString = conectionstring,
                    EmailFrom = _logger.EmailFrom,
                    EmailTo = _logger.EmailTo,
                    EmailPass = _logger.EmailPass,
                };
                if (order == null)
                {
                    throw new Exception("Order is null.");
                }
                bool ordersucces = false;
                ordersucces = order_data.GetWebOrderFromOrder(order);
                bool insertsucces = false;
                insertsucces = order_data.InsertOrder();
                return true;
            }
            catch (Exception e) { _logger.LogError(e.InnerException, arcName, e.Message); return false; }
        }
        public void ClientOrder(string filepath, string processpath, string localpath)
        {
            try
            {
                ClientOrders clientorder = new ClientOrders
                {
                    localpath = localpath,
                    connectionString = conectionstring
                };


                clientorder.Fillorder(order);

                string clientpath = "";
                clientpath = clientorder.MakeFile(archivo);
                if (clientpath != "")
                    clientserver.Upload(clientpath, archivo + ".json");
                else
                    throw new Exception("Error al generar el archivo");

                ftpDetails.MoveFtpFile(filepath, processpath + arcName);
                _logger.LogInformation("Fin del registro");
            }
            catch (Exception e) { _logger.LogError(e.InnerException, arcName, e.Message); }
        }
        //metodo para escribir cada seccion del json
        private void WriteSection(StreamWriter sw, string section, List<string> values)
        {
            sw.WriteLine("\"" + section + "\": {");
            int i = 0;
            while (i < values.Count)
            {
                if (i < values.Count - 1)
                    sw.WriteLine(values[i] + ',');
                else
                    sw.WriteLine(values[i]);
                i++;
            }
            sw.WriteLine("},");
        }
        //metodo para parsear cada linea y determinar si el valor es numerico o texto
        private string ParseString(string linea)
        {
            // Verificamos si hay separador ':'
            if (!linea.Contains(":"))
                return linea;

            var partes = linea.Split(':', 2);
            string campo = partes[0].Trim();
            string valor = partes[1].Trim();

            // Detectar si es numérico
            bool esNumerico = double.TryParse(valor, NumberStyles.Any, CultureInfo.InvariantCulture, out _);

            // Detectar si inicia con cero y no es 0
            //bool iniciaConCero = valor.Length > 1 && valor.StartsWith("0") && valor != "0.00";

            string line;
            if (valor.StartsWith("+002.00"))
            {
                valor = "2";
            }

            if (esNumerico)// && !iniciaConCero)
            {
                // número normal sin comillas
                if (campo.EndsWith("upc") || campo.EndsWith("desc") || campo.EndsWith("sku")
                    || campo.EndsWith("doc_no") || campo.EndsWith("ship_cust_no"))
                    line = $"\t\"{campo}\": \"{valor}\"";
                else
                {
                    line = $"\t\"{campo}\": {valor}";
                }
            }
            else
            {

                // texto o código con ceros al inicio entre comillas
                //tranforma de 2025-10-15-08-02-23 a datetime
                if (campo == ("date_ordered"))
                {
                    valor = DateTime.ParseExact(valor, "yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture).ToString("yyyy-MM-ddTHH:mm:ss");
                }
                line = $"\t\"{campo}\": \"{valor}\"";
            }

            return line;
        }
    }
}

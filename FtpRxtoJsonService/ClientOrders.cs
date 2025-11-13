
using System.Text.Json;
using FtpRxtoJsonService;
using Microsoft.Data.SqlClient;

namespace FtpRxtoJsonService.css
{

    public class ClientOrders
    {

        public ClientOrders()
        {
            Ponum = "";
            // Los valores numéricos se inicializan automáticamente a 0
            // Las strings se inicializan automáticamente a null, así que las inicializamos a string vacío
            Framesku = "";
            Frametype = "";
            Notes = "";
            Prioritystatus = "";
            Specialstatus = "";
            Accessories1 = "";
            Accessories2 = "";
            Accessories3 = "";
            Accessories4 = "";
            Accessories5 = "";
        }
        public string Ponum { get; set; }
        public double Rsph { get; set; }
        public double Rcyl { get; set; }
        public short Raxis { get; set; }
        public double Radd { get; set; }
        public double Ripd { get; set; }
        public short Rheight { get; set; }
        public double Roc { get; set; }
        public double Lsph { get; set; }
        public double Lcyl { get; set; }
        public short Laxis { get; set; }
        public double Ladd { get; set; }
        public double Lipd { get; set; }
        public short Lheight { get; set; }
        public double Loc { get; set; }
        public short Basecurve { get; set; }
        public string Framesku { get; set; }
        public string Frametype { get; set; }
        public double A { get; set; }
        public double B { get; set; }
        public double Ed { get; set; }
        public double Dbl { get; set; }
        public short Material { get; set; }
        public short Design { get; set; }
        public short Tint { get; set; }
        public short Ar { get; set; }
        public string Notes { get; set; }
        public string Prioritystatus { get; set; }
        public string Specialstatus { get; set; }
        public string Accessories1 { get; set; }
        public string Accessories2 { get; set; }
        public string Accessories3 { get; set; }
        public string Accessories4 { get; set; }
        public string Accessories5 { get; set; }
        public short CentralThickness { get; set; }
        public short EdgeThickness { get; set; }
        public ILogger _logger { get; set; }
        public string connectionString { get; set; }

        public bool Fillorder(Root root)
        {
            if (root == null)
                return false;
            try
            {
                EdgeThickness = 0;//
                Basecurve = 0;//
                CentralThickness = 0;//
                Frametype = root.order.frame.frame_model;
                Dbl = root.order.frame.frame_dbl;
                Prioritystatus = "";
                Specialstatus = "";
                Rsph = root.order.rx_od.rx_od_sphere;
                Rcyl = root.order.rx_od.rx_od_cylinder;
                Raxis = (short)root.order.rx_od.rx_od_axis;
                Radd = root.order.rx_od.rx_od_add;
                Rheight = (short)root.order.rx_od.rx_od_seg_height;
                Ripd = root.order.rx_od.rx_od_near;
                Roc = root.order.x_rx_oc_height_qual; //checar
                Lsph = root.order.rx_os.rx_os_sphere;
                Lcyl = root.order.rx_os.rx_os_cylinder;
                Laxis = (short)root.order.rx_os.rx_os_axis;
                Ladd = root.order.rx_os.rx_os_add;
                Lheight = (short)root.order.rx_os.rx_os_seg_height;
                Lipd = root.order.rx_os.rx_os_near;
                Loc = root.order.x_rx_oc_height_qual; //checar
                A = root.order.frame.frame_a;
                B = root.order.frame.frame_b;
                Ed = root.order.frame.frame_ed;
                WebOrders order = new();

                string antireflection="";
                //vemos que item es el frame y cual el coating para el SKU y AR
                foreach (var item in root.order.items)
                {
                    if (item.item_description == "FRAME")
                        Framesku = item.sku;
                    else if (item.item_description.StartsWith("Coatings"))
                        antireflection = item.item_description;
                }
                if (antireflection.EndsWith("Augen Standard AR"))
                    antireflection = "PREMIUM";
                Ar = (short)GetAntiRefl(antireflection);

                Material = (short)root.order.lens_od.x_lens_od_style_code;
                Design = (short)root.order.lens_od.x_lens_od_material_code;
                Tint = (short)root.order.lens_os.x_lens_os_color_code;
                Notes = root.order.customer_po_num + "-" + root.order.customer_tray_num + root.order.items[0].item_description; //revisar no importa
                Ponum = root.order.customer_tray_num + "-" + root.order.customer_po_num; ;
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
            return true;
        }
        public string MakeFile(string nombre)
        {
            try
            {
                string path = "C:\\Users\\luis3\\Downloads\\" + nombre + ".json";
                string jsonString = "";
                jsonString = JsonSerializer.Serialize(this);
                StreamWriter sw = new StreamWriter(path);
                sw.WriteLine(jsonString);
                sw.Close();
                return path.Trim();
            }
            catch 
            {
                return "";
            }
        }
        public int GetAntiRefl(string anti_refl)
        {
            /*Coatings - Anti-Reflective - Augen Premium AR
                Coatings - Anti - Reflective - Augen Standard AR = 1PREMIUM AR
                Coatings Anti-Reflective Augen Premium AR = 1 PREMIUM AR
                Coatings Anti-Reflective Augen Std Back Side AR = 4 BACKSIDE HD AR
                Coatings Anti - Reflective Augen Prm Back Side AR = 4 BACKSIDE HD AR*/

            string query = "SELECT cl_AR FROM TblAR WHERE AR = @anti_refl";

            using SqlConnection con = new SqlConnection(connectionString);
            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@anti_refl", anti_refl);

            con.Open();
            object result = cmd.ExecuteScalar();

            // Si no encuentra nada devuelve 0 que es NONE en la tabla
            if (result == null || result == DBNull.Value)
                return 0;

            return Convert.ToInt32(result);
        }
    }
}

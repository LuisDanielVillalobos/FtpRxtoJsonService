using System;
using System.Collections.Generic;
using System.Data;

using Microsoft.Data.SqlClient;

namespace FtpRxtoJsonService.css
{

    public class WebOrders
    {

        public string ponumber { get; set; }
        public double rsphere { get; set; }
        public double rcylinder { get; set; }
        public int raxis { get; set; }
        public double raddition { get; set; }
        public double lsphere { get; set; }
        public double lcylinder { get; set; }
        public int laxis { get; set; }
        public double laddition { get; set; }
        public int rheight { get; set; }
        public decimal rdip { get; set; }
        public int lheight { get; set; }
        public decimal ldip { get; set; }
        public double a { get; set; }
        public double b { get; set; }
        public double ed { get; set; }
        public double bridge { get; set; }
        public string antireflection { get; set; }
        public int material { get; set; }
        public int design { get; set; }
        public int tint { get; set; }
        public string notes { get; set; }
        public int year { get; set; }
        public string NumPaquete { get; set; }
        public decimal fardip { get; set; }
        public string connectionString { get; set; }
        private const string storedProcedureName = "usp_InsertOrder";
        public ILogger _logger { get; set; }

        public DataTable GetOrders()
        {
            var datatable = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("select * from TblWebOrders", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(datatable);
                    }
                }
            }
            return datatable;
        }


        public bool InsertOrder()
        {
            WebOrders order = this;
            int rows;
            int returnValue;
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(storedProcedureName, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        // Add parameters
                        cmd.Parameters.AddWithValue("@ponumber", order.ponumber);
                        cmd.Parameters.AddWithValue("@rsphere", order.rsphere);
                        cmd.Parameters.AddWithValue("@rcylinder", order.rcylinder);
                        cmd.Parameters.AddWithValue("@raxis", order.raxis);
                        cmd.Parameters.AddWithValue("@raddition", order.raddition);
                        cmd.Parameters.AddWithValue("@lsphere", order.lsphere);
                        cmd.Parameters.AddWithValue("@lcylinder", order.lcylinder);
                        cmd.Parameters.AddWithValue("@laxis", order.laxis);
                        cmd.Parameters.AddWithValue("@laddition", order.laddition);
                        cmd.Parameters.AddWithValue("@rheight", order.rheight);
                        cmd.Parameters.AddWithValue("@rdip", order.rdip);
                        cmd.Parameters.AddWithValue("@lheight", order.lheight);
                        cmd.Parameters.AddWithValue("@ldip", order.ldip);
                        cmd.Parameters.AddWithValue("@fardip", order.fardip);
                        cmd.Parameters.AddWithValue("@a", order.a);
                        cmd.Parameters.AddWithValue("@b", order.b);
                        cmd.Parameters.AddWithValue("@ed", order.ed);
                        cmd.Parameters.AddWithValue("@bridge", order.bridge);
                        cmd.Parameters.AddWithValue("@antireflection", order.antireflection);
                        cmd.Parameters.AddWithValue("@material", order.material);
                        cmd.Parameters.AddWithValue("@design", order.design);
                        cmd.Parameters.AddWithValue("@tint", order.tint);
                        cmd.Parameters.AddWithValue("@notes", order.notes);
                        cmd.Parameters.AddWithValue("@year", order.year);
                        cmd.Parameters.AddWithValue("@NumPaquete", order.NumPaquete);
                        var returnParam = new SqlParameter("@ReturnVal", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.ReturnValue
                        };
                        cmd.Parameters.Add(returnParam);

                        con.Open();
                        rows = cmd.ExecuteNonQuery();
                        returnValue = Convert.ToInt32(returnParam.Value);
                    }

                }
                return returnValue == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error inserting order: " + ex.Message);
                return false;
            }
        }
        public bool GetWebOrderFromOrder(Root root)
        {
            // Validar que root y root.order no sean nulos
            try
            {
                if (root == null || root.order == null)
                {
                    return false;
                }
                // Asignar valores a las propiedades de WebOrders desde root.order
                rsphere = root.order.rx_od.rx_od_sphere;
                rcylinder = root.order.rx_od.rx_od_cylinder;
                raxis = root.order.rx_od.rx_od_axis;
                raddition = root.order.rx_od.rx_od_add;
                lsphere = root.order.rx_os.rx_os_sphere;
                lcylinder = root.order.rx_os.rx_os_cylinder;
                laxis = root.order.rx_os.rx_os_axis;
                laddition = root.order.rx_os.rx_os_add;

                rheight = (int)root.order.rx_od.rx_od_seg_height;
                rdip = (decimal)root.order.rx_od.rx_od_near;
                lheight = (int)root.order.rx_os.rx_os_seg_height;
                ldip = (decimal)root.order.rx_os.rx_os_near;
                fardip = ldip + rdip;// revisar dip

                a = root.order.frame.frame_a;
                b = root.order.frame.frame_b;
                ed = root.order.frame.frame_ed;
                bridge = root.order.frame.frame_bridge;
                foreach (var item in root.order.items)
                {
                    if (item.item_description == "FRAME")
                        antireflection = root.order.items[0].item_description;

                }
                if (antireflection.EndsWith("Augen Standard AR"))
                    antireflection = "PREMIUM";
                // revisar manda parametro regresa id
                /*Coatings - Anti-Reflective - Augen Premium AR
                 Coatings - Anti-Reflective - Augen Standard AR
                Coatings - Anti - Reflective - Augen Standard AR = 1PREMIUM AR
                Coatings Anti-Reflective Augen Premium AR = 1 PREMIUM AR
                Coatings Anti-Reflective Augen Std Back Side AR = 4 BACKSIDE HD AR
                Coatings Anti - Reflective Augen Prm Back Side AR = 4 BACKSIDE HD AR*/
                material = root.order.lens_od.x_lens_od_style_code;
                if (material == 0)
                {
                    material = 1;
                }
                if (material == 01542)
                { material = 1; }
                design = root.order.lens_od.x_lens_od_material_code;
                if (design == 0)
                {
                    design = 1;
                }
                tint = root.order.lens_od.x_lens_od_color_code;
                year = root.order.date_ordered.Year;
                NumPaquete = root.order.customer_tray_num + "-" + root.order.customer_po_num; // Customer-po-núm - tray-num = numpaquete
                notes = NumPaquete + root.order.items[0].item_description; // revisar no importa
                ponumber = root.order.customer_tray_num + "-" + root.order.customer_po_num;
                
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError("Exception: " + e.Message);
                return false;
            }
        }
    }
}

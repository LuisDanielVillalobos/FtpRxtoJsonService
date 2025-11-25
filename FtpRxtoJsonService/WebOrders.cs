using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using Microsoft.Data.SqlClient;
using static Azure.Core.HttpHeader;

namespace FtpRxtoJsonService.css
{

    public class WebOrders
    {
        public int custnum { get; set; }
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
        //public int year { get; set; }
        public string NumPaquete { get; set; }
        public decimal fardip { get; set; }
        public string connectionString { get; set; }
        private const string storedProcedureName = "usp_InsertWebOrder";
        public ILogger _logger { get; set; }
        public string EmailFrom { get; set; }
        public string EmailPass { get; set; }
        public string EmailTo { get; set; }

        public DataTable GetOrders()
        {
            var datatable = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("select * from TblWebOrdersAugen", con))
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
            int returnValue;
            try
            {
                var datatable = new DataTable();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(storedProcedureName, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        // Add parameters
                        cmd.Parameters.AddWithValue("@custnum", custnum);
                        cmd.Parameters.AddWithValue("@ponumber", ponumber);
                        cmd.Parameters.AddWithValue("@rsphere", rsphere);
                        cmd.Parameters.AddWithValue("@rcylinder", rcylinder);
                        cmd.Parameters.AddWithValue("@raxis", raxis);
                        cmd.Parameters.AddWithValue("@raddition", raddition);
                        cmd.Parameters.AddWithValue("@lsphere", lsphere);
                        cmd.Parameters.AddWithValue("@lcylinder", lcylinder);
                        cmd.Parameters.AddWithValue("@laxis", laxis);
                        cmd.Parameters.AddWithValue("@laddition", laddition);
                        cmd.Parameters.AddWithValue("@rheight", rheight);
                        cmd.Parameters.AddWithValue("@rdip", rdip);
                        cmd.Parameters.AddWithValue("@lheight", lheight);
                        cmd.Parameters.AddWithValue("@ldip", ldip);
                        cmd.Parameters.AddWithValue("@fardip", fardip);
                        cmd.Parameters.AddWithValue("@a", a);
                        cmd.Parameters.AddWithValue("@b", b);
                        cmd.Parameters.AddWithValue("@ed", ed);
                        cmd.Parameters.AddWithValue("@bridge", bridge);
                        cmd.Parameters.AddWithValue("@antireflection", antireflection);
                        cmd.Parameters.AddWithValue("@material", material);
                        cmd.Parameters.AddWithValue("@design", design);
                        cmd.Parameters.AddWithValue("@tint", tint);
                        cmd.Parameters.AddWithValue("@notes", notes);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(datatable);
                        }
                    }

                }

                if (datatable.Rows.Count > 1)
                {
                    NotificarReg(datatable);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error inserting order: " + ex.Message);
                return false;
            }
        }

        private void NotificarReg(DataTable dataTable)
        {
            try
            {
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
                        message.Subject = "Duplicated job " + ponumber;
                        message.IsBodyHtml = true;
                        var Body = new StringBuilder();
                        Body.Append("<h3>Archivo duplicado</h3>");
                        Body.Append($"<p>El archivo con ponumber <b>{ponumber}</b> ya estaba registrado. Se cambió el estado.</p>");
                        Body.Append("<table border='1' cellpadding='6' cellspacing='0' style='border-collapse:collapse;'>");

                        // Encabezados
                        Body.Append("<tr style='background-color:#f0f0f0;'>");
                        foreach (DataColumn col in dataTable.Columns)
                        {
                            Body.Append($"<th>{col.ColumnName}</th>");
                        }
                        Body.Append("</tr>");

                        // Filas
                        foreach (DataRow row in dataTable.Rows)
                        {
                            Body.Append("<tr>");
                            foreach (var item in row.ItemArray)
                            {
                                Body.Append($"<td>{item}</td>");
                            }
                            Body.Append("</tr>");
                        }
                        Body.Append("</table>");
                        message.Body = Body.ToString();
                        client.Send(message);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning("Trouble with email: " + e.Message);
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
                design = root.order.lens_od.x_lens_od_material_code;
                if (design == 0)
                {
                    design = 1;
                }
                tint = root.order.lens_od.x_lens_od_color_code;
                NumPaquete = root.order.customer_tray_num + "-" + root.order.customer_po_num; // Customer-po-núm - tray-num = numpaquete
                notes = NumPaquete + root.order.items[0].item_description; // revisar no importa
                ponumber = root.order.customer_tray_num + "-" + root.order.customer_po_num;
                custnum = root.order.cust_num;
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

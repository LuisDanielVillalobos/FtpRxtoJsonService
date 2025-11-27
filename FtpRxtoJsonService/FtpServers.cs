using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using static FtpRxtoJsonService.FtpServers;

namespace FtpRxtoJsonService
{
    internal class FtpServers
    {
        public class server
        {
            public string name { get; set; }
            public string username { get; set; }
            public string password { get; set; }
            public string jobfolder { get; set; }
        }
        public string connectionString { get; set; }
        public List<server> servers = new();


        private string query = "select * from dbo.LabFtpServer where active = 1";

        public List<server> GetServers()
        {
            try
            {
                var datatable = new DataTable();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.CommandType = CommandType.Text;
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(datatable);
                        }
                    }
                }
                foreach (DataRow row in datatable.Rows)
                {
                    var server = new server
                    {
                        name = row["servername"].ToString().Trim(),
                        username = row["username"].ToString().Trim(),
                        password = row["userpwd"].ToString().Trim(),
                        jobfolder = row["jobfolder"].ToString().Trim()
                    };
                    this.servers.Add(server);
                }
                return servers;
            }
            catch (Exception e) { throw new Exception("getting client servers", e); }
        }
    }
}

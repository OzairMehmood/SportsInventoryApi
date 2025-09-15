using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ProjectApi.Models
{
	public class DBAccess
	{
        static string constr = "Data Source=DESKTOP-GN5TADS\\SQLEXPRESS; Initial Catalog=BiitSportsInventoryManagement; Integrated Security=True;";
        public  SqlConnection con = new SqlConnection(constr);
        public SqlCommand cmd = null;
        public SqlDataReader sdr = null;

        public void OpenConnection()
        {
            if (con.State == System.Data.ConnectionState.Closed)
            {
                con.Open();
            }
        }

        public void CloseConnection()
        {
            if (con.State == System.Data.ConnectionState.Open)
            {
                con.Close();
            }
        }

        public void InsertUpdateDelete(string querry)
        {
            cmd = new SqlCommand(querry, con);
            cmd.ExecuteNonQuery();
        }
        public SqlDataReader GetData(string querry)
        {
            cmd = new SqlCommand(querry, con);
            sdr = cmd.ExecuteReader();
            return sdr;
        }
    }
}
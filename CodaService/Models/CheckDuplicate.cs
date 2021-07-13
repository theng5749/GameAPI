using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

namespace CodaService.Models
{
    public class CheckDuplicate
    {
        public bool CheckDup(string Transaction)
        {
            bool result = false;
            SqlConnection conn = new SqlConnection();
            SqlCommand cmd = new SqlCommand();
            string strcon = "Data Source=172.28.12.196\\sqln2;Initial Catalog=CodaPayDB;User ID=sa;Password=#Ltc1qaz2wsx@dbql";
            CodaPayDBEntities db = new CodaPayDBEntities();
            using (conn)
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                try
                {
                    conn.ConnectionString = strcon;
                    conn.Open();
                    string sql = "select transactionID from tbl_deduct_log where transactionID =@tran_id";
                    cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.Add("@tran_id", SqlDbType.NVarChar, 20).Value = Transaction.ToString().Trim();
                    SqlDataReader dr;
                    dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                catch (Exception)
                {
                    result = false;
                }
            }
            return result;
        }
    }
}
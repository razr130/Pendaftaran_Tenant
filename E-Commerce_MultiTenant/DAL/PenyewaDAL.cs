using E_Commerce_MultiTenant.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace E_Commerce_MultiTenant.DAL
{
    public class PenyewaDAL : IDisposable
    {
        private ECommerce db = new ECommerce();
        public void Dispose()
        {
            db.Dispose();
        }

        public int GetIDPenyewa(string namaperusahaan)
        {
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["ECommerce"].ConnectionString;
            int result = 0;

            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "SELECT [id_penyewa]" +
                    " FROM[MultiTenancy_Sablon].[dbo].[Penyewa] WHERE [nama_perusahaan]='" + namaperusahaan+"'";

                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result = (int)reader["id_penyewa"];
                        }
                    }

                }
                catch (Exception)
                {

                }

                conn.Close();
            }
            return result;
        }

        public int GetIDUI(int idpenyewa)
        {
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["ECommerce"].ConnectionString;
            int result = 0;

            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "SELECT [id_ui]" +
                    " FROM[MultiTenancy_Sablon].[dbo].[Data_UI] WHERE [id_penyewa]=" + idpenyewa.ToString();

                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result = (int)reader["id_ui"];
                        }
                    }

                }
                catch (Exception)
                {

                }

                conn.Close();
            }
            return result;
        }
    }
}
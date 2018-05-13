using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace E_Commerce_MultiTenant.Controllers
{
    public class ShoppingCartController : Controller
    {
        // GET: ShoppingCart
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddtoCart(string subdomain)
        {
            int id_customer = 0;
            int no_order = 0;
            int no_detail = 0;
            var tglorder = DateTime.Now.ToString("yyyy-MM-dd");
            string statusbayar = "belum";
            string dikirm = "tidak";
            if (Session["noorder"] == null)
            {


                string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["ECommerce"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connstring))
                {
                    conn.Open();
                    string query = "SELECT [id_cutomer]" +
                   " FROM[MultiTenancy_Sablon].[dbo].[Customer_" + subdomain + "] WHERE email_customer='" + Session["email"].ToString() + "'";


                    SqlCommand sqlcom = new SqlCommand(query, conn);
                    try
                    {
                        using (SqlDataReader reader = sqlcom.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                id_customer = (int)reader["id_cutomer"];
                            }
                        }

                        sqlcom.CommandText = "INSERT INTO [dbo].[Order_" + subdomain + "]" +
                                "([id_customer]" +
                                ",[tgl_order]" +
                                ",[status_bayar]" +
                                ",[dikirim])" +
                                " output INSERTED.no_order VALUES" +
                                " (" + id_customer + ",'" + tglorder + "','" + statusbayar + "','" + dikirm + "')";
                        no_order = (int)sqlcom.ExecuteScalar();
                        Session["noorder"] = no_order.ToString();

                        if (Session["kategori"].ToString() == "pakaian")
                        {
                            sqlcom.CommandText = "INSERT INTO [dbo].[DetailOrder_" + subdomain + "]" +
                                "([no_order]" +
                                ",[id_produk]" +
                                ",[id_bahan]" +
                                ",[id_jns_sablon]" +
                                ",[desain]" +
                                ",[jumlah]" +
                                ",[subtotal]," +
                                "[catatan])" +
                                " output INSERTED.no_detail VALUES" +
                                " (" + no_order + "," + Session["id_produk"].ToString() + "," + Session["idbahan"] +
                                "," + Session["idsablon"] + ",'" + Session["desain"].ToString() + "'," + Session["jumlahorder"].ToString() +
                                "," + Session["totalhargaall"].ToString() + ",'" + Session["catatan"].ToString() + "')";

                            no_detail = (int)sqlcom.ExecuteScalar();
                            if (Session["anak"] != null)
                            {
                                sqlcom.CommandText = "INSERT INTO[dbo].[UkuranOrder_" + subdomain + "] " +
                                    "([id_ukuran]" +
                                    ",[no_detail] ,[jumlah])" +
                                    " VALUES" +
                                    " (1" +
                                    "," + no_detail +
                                    "," + Session["anak"].ToString() + ")";

                                sqlcom.ExecuteNonQuery();
                            }
                            if (Session["xs"] != null)
                            {
                                sqlcom.CommandText = "INSERT INTO[dbo].[UkuranOrder_" + subdomain + "] " +
                                    "([id_ukuran]" +
                                    ",[no_detail] ,[jumlah])" +
                                    " VALUES" +
                                    " (2" +
                                    "," + no_detail +
                                    "," + Session["xs"].ToString() + ")";

                                sqlcom.ExecuteNonQuery();
                            }
                            if (Session["s"] != null)
                            {
                                sqlcom.CommandText = "INSERT INTO[dbo].[UkuranOrder_" + subdomain + "] " +
                                    "([id_ukuran]" +
                                    ",[no_detail] ,[jumlah])" +
                                    " VALUES" +
                                    " (3" +
                                    "," + no_detail +
                                    "," + Session["s"].ToString() + ")";

                                sqlcom.ExecuteNonQuery();
                            }
                            if (Session["m"] != null)
                            {
                                sqlcom.CommandText = "INSERT INTO[dbo].[UkuranOrder_" + subdomain + "] " +
                                    "([id_ukuran]" +
                                    ",[no_detail] ,[jumlah])" +
                                    " VALUES" +
                                    " (4" +
                                    "," + no_detail +
                                    "," + Session["m"].ToString() + ")";

                                sqlcom.ExecuteNonQuery();
                            }
                            if (Session["l"] != null)
                            {
                                sqlcom.CommandText = "INSERT INTO[dbo].[UkuranOrder_" + subdomain + "] " +
                                    "([id_ukuran]" +
                                    ",[no_detail] ,[jumlah])" +
                                    " VALUES" +
                                    " (5" +
                                    "," + no_detail +
                                    "," + Session["l"].ToString() + ")";

                                sqlcom.ExecuteNonQuery();
                            }
                            if (Session["xl"] != null)
                            {
                                sqlcom.CommandText = "INSERT INTO[dbo].[UkuranOrder_" + subdomain + "] " +
                                    "([id_ukuran]" +
                                    ",[no_detail] ,[jumlah])" +
                                    " VALUES" +
                                    " (6" +
                                    "," + no_detail +
                                    "," + Session["xl"].ToString() + ")";

                                sqlcom.ExecuteNonQuery();
                            }
                            if (Session["xxl"] != null)
                            {
                                sqlcom.CommandText = "INSERT INTO[dbo].[UkuranOrder_" + subdomain + "] " +
                                    "([id_ukuran]" +
                                    ",[no_detail] ,[jumlah])" +
                                    " VALUES" +
                                    " (7" +
                                    "," + no_detail +
                                    "," + Session["xxl"].ToString() + ")";

                                sqlcom.ExecuteNonQuery();
                            }
                            if (Session["xxxl"] != null)
                            {
                                sqlcom.CommandText = "INSERT INTO[dbo].[UkuranOrder_" + subdomain + "] " +
                                    "([id_ukuran]" +
                                    ",[no_detail] ,[jumlah])" +
                                    " VALUES" +
                                    " (8" +
                                    "," + no_detail +
                                    "," + Session["xxxl"].ToString() + ")";

                                sqlcom.ExecuteNonQuery();
                            }
                            if (Session["4xl"] != null)
                            {
                                sqlcom.CommandText = "INSERT INTO[dbo].[UkuranOrder_" + subdomain + "] " +
                                    "([id_ukuran]" +
                                    ",[no_detail] ,[jumlah])" +
                                    " VALUES" +
                                    " (9" +
                                    "," + no_detail +
                                    "," + Session["4xl"].ToString() + ")";

                                sqlcom.ExecuteNonQuery();
                            }
                            if (Session["5xl"] != null)
                            {
                                sqlcom.CommandText = "INSERT INTO[dbo].[UkuranOrder_" + subdomain + "] " +
                                    "([id_ukuran]" +
                                    ",[no_detail] ,[jumlah])" +
                                    " VALUES" +
                                    " (10" +
                                    "," + no_detail +
                                    "," + Session["5xl"].ToString() + ")";

                                sqlcom.ExecuteNonQuery();
                            }

                        }
                        else
                        {
                            sqlcom.CommandText = "INSERT INTO [dbo].[DetailOrder_" + subdomain + "]" +
                                "([no_order]" +
                                ",[id_produk]" +
                                ",[id_bahan]" +
                                ",[id_jns_sablon]" +
                                ",[desain]" +
                                ",[jumlah]" +
                                ",[subtotal]," +
                                "[catatan])" +
                                " VALUES" +
                                "(" + no_order + "," + Session["id_produk"].ToString() + "," + Session["idbahan"] +
                                "," + Session["idsablon"] + ",'" + Session["desain"].ToString() + "'," + Session["jumlahorder"].ToString() +
                                "," + Session["totalhargaall"].ToString() + ",'" + Session["catatan"].ToString() + "')";

                            sqlcom.ExecuteNonQuery();
                        }
                    }


                    catch (Exception)
                    {

                    }
                    conn.Close();
                }
            }
            else
            {
                string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["ECommerce"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connstring))
                {
                    conn.Open();
                    string query = "INSERT INTO [dbo].[DetailOrder_" + subdomain + "]" +
                                "([no_order]" +
                                ",[id_produk]" +
                                ",[id_bahan]" +
                                ",[id_jns_sablon]" +
                                ",[desain]" +
                                ",[jumlah]" +
                                ",[subtotal]," +
                                "[catatan])" +
                                " output INSERTED.no_detail VALUES" +
                                "(" + Session["noorder"].ToString() + "," + Session["id_produk"].ToString() + "," + Session["idbahan"] +
                                "," + Session["idsablon"] + ",'" + Session["desain"].ToString() + "'," + Session["jumlahorder"].ToString() +
                                "," + Session["totalhargaall"].ToString() + ",'" + Session["catatan"].ToString() + "')";

                    SqlCommand sqlcom = new SqlCommand(query, conn);

                    try
                    {
                        no_detail = (int)sqlcom.ExecuteScalar();

                        if (Session["anak"] != null)
                        {
                            sqlcom.CommandText = "INSERT INTO[dbo].[UkuranOrder_" + subdomain + "] " +
                                "([id_ukuran]" +
                                ",[no_detail] ,[jumlah])" +
                                " VALUES" +
                                " (1" +
                                "," + no_detail +
                                "," + Session["anak"].ToString() + ")";

                            sqlcom.ExecuteNonQuery();
                        }
                        if (Session["xs"] != null)
                        {
                            sqlcom.CommandText = "INSERT INTO[dbo].[UkuranOrder_" + subdomain + "] " +
                                "([id_ukuran]" +
                                ",[no_detail] ,[jumlah])" +
                                " VALUES" +
                                " (2" +
                                "," + no_detail +
                                "," + Session["xs"].ToString() + ")";

                            sqlcom.ExecuteNonQuery();
                        }
                        if (Session["s"] != null)
                        {
                            sqlcom.CommandText = "INSERT INTO[dbo].[UkuranOrder_" + subdomain + "] " +
                                "([id_ukuran]" +
                                ",[no_detail] ,[jumlah])" +
                                " VALUES" +
                                " (3" +
                                "," + no_detail +
                                "," + Session["s"].ToString() + ")";

                            sqlcom.ExecuteNonQuery();
                        }
                        if (Session["m"] != null)
                        {
                            sqlcom.CommandText = "INSERT INTO[dbo].[UkuranOrder_" + subdomain + "] " +
                                "([id_ukuran]" +
                                ",[no_detail] ,[jumlah])" +
                                " VALUES" +
                                " (4" +
                                "," + no_detail +
                                "," + Session["m"].ToString() + ")";

                            sqlcom.ExecuteNonQuery();
                        }
                        if (Session["l"] != null)
                        {
                            sqlcom.CommandText = "INSERT INTO[dbo].[UkuranOrder_" + subdomain + "] " +
                                "([id_ukuran]" +
                                ",[no_detail] ,[jumlah])" +
                                " VALUES" +
                                " (5" +
                                "," + no_detail +
                                "," + Session["l"].ToString() + ")";

                            sqlcom.ExecuteNonQuery();
                        }
                        if (Session["xl"] != null)
                        {
                            sqlcom.CommandText = "INSERT INTO[dbo].[UkuranOrder_" + subdomain + "] " +
                                "([id_ukuran]" +
                                ",[no_detail] ,[jumlah])" +
                                " VALUES" +
                                " (6" +
                                "," + no_detail +
                                "," + Session["xl"].ToString() + ")";

                            sqlcom.ExecuteNonQuery();
                        }
                        if (Session["xxl"] != null)
                        {
                            sqlcom.CommandText = "INSERT INTO[dbo].[UkuranOrder_" + subdomain + "] " +
                                "([id_ukuran]" +
                                ",[no_detail] ,[jumlah])" +
                                " VALUES" +
                                " (7" +
                                "," + no_detail +
                                "," + Session["xxl"].ToString() + ")";

                            sqlcom.ExecuteNonQuery();
                        }
                        if (Session["xxxl"] != null)
                        {
                            sqlcom.CommandText = "INSERT INTO[dbo].[UkuranOrder_" + subdomain + "] " +
                                "([id_ukuran]" +
                                ",[no_detail] ,[jumlah])" +
                                " VALUES" +
                                " (8" +
                                "," + no_detail +
                                "," + Session["xxxl"].ToString() + ")";

                            sqlcom.ExecuteNonQuery();
                        }
                        if (Session["4xl"] != null)
                        {
                            sqlcom.CommandText = "INSERT INTO[dbo].[UkuranOrder_" + subdomain + "] " +
                                "([id_ukuran]" +
                                ",[no_detail] ,[jumlah])" +
                                " VALUES" +
                                " (9" +
                                "," + no_detail +
                                "," + Session["4xl"].ToString() + ")";

                            sqlcom.ExecuteNonQuery();
                        }
                        if (Session["5xl"] != null)
                        {
                            sqlcom.CommandText = "INSERT INTO[dbo].[UkuranOrder_" + subdomain + "] " +
                                "([id_ukuran]" +
                                ",[no_detail] ,[jumlah])" +
                                " VALUES" +
                                " (10" +
                                "," + no_detail +
                                "," + Session["5xl"].ToString() + ")";

                            sqlcom.ExecuteNonQuery();
                        }
                    }
                    catch (Exception)
                    {

                    }
                    conn.Close();

                }
            }
            return RedirectToAction("Index", "Produk");
        }
    }
}
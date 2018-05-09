using E_Commerce_MultiTenant.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace E_Commerce_MultiTenant.Controllers
{
    public class ProdukController : Controller
    {
        // GET: Produk
        public ActionResult Index(string subdomain)
        {
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["ECommerce"].ConnectionString;
            List<Produk> result = new List<Produk>();

            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "SELECT [id_produk]" +
                    ",[nama_produk]" +
                    ",[deskripsi]" +
                    ",[kategori]" +
                    ",[foto_produk]" +
                    "FROM[MultiTenancy_Sablon].[dbo].[Produk_" + subdomain + "]";

                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Produk item = new Produk()
                            {
                                id_produk = (int)reader["id_produk"],
                                nama_produk = reader["nama_produk"].ToString(),
                                deskripsi = reader["deskripsi"].ToString(),
                                kategori = reader["kategori"].ToString(),
                                foto_produk = reader["foto_produk"].ToString()
                            };
                            result.Add(item);
                        }
                    }

                }
                catch (Exception)
                {

                }

                conn.Close();
            }
            return View(result);
        }


        public ActionResult CreatePesananPakaian(string subdomain, int id_produk)
        {
            if (Session["user"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {




                var lstbahan = new List<SelectListItem>();
                var lstsablon = new List<SelectListItem>();
                string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["ECommerce"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connstring))
                {
                    conn.Open();
                    string query = "SELECT [id_bahan]" +
                        ",[nama_bahan]" +
                        "FROM[MultiTenancy_Sablon].[dbo].[Bahan_" + subdomain + "] WHERE id_produk=" + id_produk.ToString();

                    SqlCommand sqlcom = new SqlCommand(query, conn);
                    try
                    {
                        using (SqlDataReader reader = sqlcom.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lstbahan.Add(new SelectListItem
                                {
                                    Value = reader["id_bahan"].ToString(),
                                    Text = reader["nama_bahan"].ToString()
                                });
                            }
                            ViewBag.Bahan = lstbahan;
                        }

                        sqlcom.CommandText = "SELECT [id_jns_sablon]" +
                        ",[nama_sablon]" +
                        "FROM[MultiTenancy_Sablon].[dbo].[JenisSablon_" + subdomain + "] WHERE id_produk=" + id_produk.ToString();
                        using (SqlDataReader reader = sqlcom.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lstsablon.Add(new SelectListItem
                                {
                                    Value = reader["id_jns_sablon"].ToString(),
                                    Text = reader["nama_sablon"].ToString()
                                });
                            }
                            ViewBag.Sablon = lstsablon;
                        }
                        sqlcom.CommandText = "SELECT [nama_produk]" +
                        ",[foto_produk]" +
                        "FROM[MultiTenancy_Sablon].[dbo].[Produk_" + subdomain + "] WHERE id_produk=" + id_produk.ToString();
                        using (SqlDataReader reader = sqlcom.ExecuteReader())
                        {

                            if (reader.Read())
                            {
                                Session["namaproduk"] = reader["nama_produk"].ToString();
                                Session["fotoproduk"] = reader["foto_produk"].ToString();
                            }


                        }
                    }
                    catch (Exception)
                    {

                    }

                    conn.Close();
                }

                return View();
            }
        }
        [HttpPost]
        public ActionResult CreatePesananPakaian(string subdomain)
        {

            return RedirectToAction("Index", "Produk");
        }


        public ActionResult CreatePesananNonPakaian(string subdomain, int id)
        {


            return View();

        }
        [HttpPost]
        public ActionResult CreatePesananNonPakaian()
        {

            return View();
        }
    }
}
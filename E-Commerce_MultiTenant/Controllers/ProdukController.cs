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
                var lsttambahan = new List<SelectListItem>();
                var lstukuran = new List<SelectListItem>();
                string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["ECommerce"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connstring))
                {
                    conn.Open();
                    string query = "SELECT [id_bahan]" +
                        ",[id_jns_sablon]" +
                        ",[nama_bahan]" +
                        ",[nama_sablon]" +
                        ",[harga]" +
                        ",count(*)" +
                        " FROM[MultiTenancy_Sablon].[dbo].[View_" + subdomain + "] WHERE id_produk=" + id_produk.ToString() +
                        " group by nama_bahan, nama_sablon,harga,id_bahan,id_jns_sablon" +
                        " having  COUNT(*) > 1 ";

                    SqlCommand sqlcom = new SqlCommand(query, conn);
                    try
                    {
                        using (SqlDataReader reader = sqlcom.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lstbahan.Add(new SelectListItem
                                {
                                    Value = reader["id_bahan"].ToString() + "&" + reader["id_jns_sablon"].ToString(),
                                    Text = "Bahan " + reader["nama_bahan"].ToString() + " dengan sablon " + reader["nama_sablon"].ToString()
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
                        sqlcom.CommandText = "SELECT [id_tambahan]" +
                        ",[nama_tambahan]" +
                        "FROM[MultiTenancy_Sablon].[dbo].[TabelTambahan_" + subdomain + "] WHERE id_produk=" + id_produk.ToString();
                        using (SqlDataReader reader = sqlcom.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader["nama_tambahan"].ToString() != "XXL" && reader["nama_tambahan"].ToString() != "Lebih dari XXL")
                                {
                                    lsttambahan.Add(new SelectListItem
                                    {

                                        Value = reader["id_tambahan"].ToString(),
                                        Text = reader["nama_tambahan"].ToString()
                                    });
                                }
                            }
                            ViewBag.Tambahan = lsttambahan;
                        }
                        sqlcom.CommandText = "SELECT [id_ukuran]" +
                        ",[ukuran]" +
                        "FROM[MultiTenancy_Sablon].[dbo].[Ukuran_" + subdomain + "]";
                        using (SqlDataReader reader = sqlcom.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lstukuran.Add(new SelectListItem
                                {
                                    Value = reader["id_ukuran"].ToString(),
                                    Text = reader["ukuran"].ToString()
                                });
                            }
                            ViewBag.Ukuran = lstukuran;
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
        public ActionResult CreatePesananPakaian(string subdomain, string lstbahan, HttpPostedFileBase desain, int? anak, int? xs, int? s
            , int? m, int? l, int? xl, int? xxl, int? xxxl, int? empatxl, int? limaxl, 
            string tambahan1, string ukuran1, int? jmlhtambahan1, string tambahan2, string ukuran2,
            int? jmlhtambahan2, string tambahan3, string ukuran3, int? jmlhtambahan3)
        {
            Session["coba"] = lstbahan;
            Session["aaa"] = "a";
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
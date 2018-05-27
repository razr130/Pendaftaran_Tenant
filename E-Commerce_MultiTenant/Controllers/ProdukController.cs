using E_Commerce_MultiTenant.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
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
                string query = "SELECT p.[id_produk], p.[nama_produk]" +
                    ",p.[deskripsi]" +
                    ",p.[kategori]" +
                    ",p.[foto_produk]" +
                    ", MIN(b.harga) as harga" +
                    " FROM[Produk_"+subdomain+"] p inner join Bahan_"+subdomain+" b on p.id_produk = b.id_produk where b.harga = (select min(harga) from bahan_"+subdomain+") group by p.[id_produk]" +
                    ",p.[nama_produk]" +
                    ",p.[deskripsi]" +
                    ",p.[kategori]" +
                    ",p.[foto_produk]";

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
                                foto_produk = reader["foto_produk"].ToString(),
                                harga = (int)reader["harga"]
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
                Session["idproduklogin"] = id_produk;
                Session["pakaianlogin"] = "ya";
                return RedirectToAction("Login", "Home");
            }
            else
            {
                Session["idproduklogin"] = null;
                Session["pakaianlogin"] = null;
                Session["kategori"] = "pakaian";
                Session["id_produk"] = id_produk.ToString();
                var lstbahan = new List<SelectListItem>();
                var lstsablon = new List<SelectListItem>();
                var lsttambahan = new List<SelectListItem>();
                var lstukuran = new List<SelectListItem>();
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

                                            Value = reader["nama_tambahan"].ToString(),
                                            Text = reader["nama_tambahan"].ToString()
                                        });
                                    }
                                    
                                    ViewBag.Tambahan = lsttambahan;
                                
                               
                            }
                            
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
                                    Value = reader["ukuran"].ToString(),
                                    Text = reader["ukuran"].ToString()
                                });
                            }
                            ViewBag.Ukuran = lstukuran;
                        }
                        sqlcom.CommandText = "SELECT [nama_produk]" +
                        ",[foto_produk],[deskripsi]" +
                        " FROM[MultiTenancy_Sablon].[dbo].[Produk_" + subdomain + "] WHERE id_produk=" + id_produk.ToString();
                        using (SqlDataReader reader = sqlcom.ExecuteReader())
                        {

                            if (reader.Read())
                            {
                                Session["namaproduk"] = reader["nama_produk"].ToString();
                                Session["fotoproduk"] = reader["foto_produk"].ToString();
                                Session["deskripsi"] = reader["deskripsi"].ToString();
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
        public ActionResult CreatePesananPakaian(string subdomain, string lstbahan, string lstsablon, HttpPostedFileBase desain, string checka4, string checka3, string checkblok, string checkdepanbelakang,
            int? anak, int? xs, int? s
            , int? m, int? l, int? xl, int? xxl, int? xxxl, int? empatxl, int? limaxl, string radiowarna,
            string tambahan1, string ukuran1, int? jmlhtambahan1, string tambahan2, string ukuran2,
            int? jmlhtambahan2, string tambahan3, string ukuran3, int? jmlhtambahan3, string catatan)
        {
            
            var catatanfull = "";
            if (tambahan1 != "")
            {
                catatanfull += "Tambahan " + tambahan1 + " di ukuran " + ukuran1 + " " + jmlhtambahan1 + " pcs.";
            }
            if (tambahan2 != "")
            {
                catatanfull += "Tambahan " + tambahan2 + " di ukuran " + ukuran2 + " " + jmlhtambahan2 + " pcs.";
            }
            if (tambahan3 != "")
            {
                catatanfull += "Tambahan " + tambahan3 + " di ukuran " + ukuran3 + " " + jmlhtambahan3 + " pcs.";
            }
            if (checka4 != null)
            {
                catatanfull += " tipe desain : " + checka4;
            }
            if (checka3 != null)
            {
                catatanfull += " tipe desain : " + checka3;
            }
            if (checkblok != null)
            {
                catatanfull += " tipe desain : " + checkblok;
            }
            if (checkdepanbelakang != null)
            {
                catatanfull += " tipe desain : " + checkdepanbelakang;
            }
            catatanfull += " warna : " + radiowarna;
            catatanfull += " " + catatan;
            Session["catatan"] = catatanfull;         
            Session["idbahan"] = lstbahan;          
            Session["idsablon"] = lstsablon;

            string filePath = "";
            string fileName = Guid.NewGuid().ToString() + "_" + desain.FileName;
            if (desain.ContentLength > 0)
            {

                filePath = Path.Combine(HttpContext.Server.MapPath("~/Content/Images"), fileName);
                desain.SaveAs(filePath);

            }
            Session["desain"] = fileName;
            if (anak != null)
            {
                Session["anak"] = anak.ToString();
            }
            if (xs != null)
            {
                Session["xs"] = xs.ToString();
            }
            if (s != null)
            {
                Session["s"] = s.ToString();
            }
            if (m != null)
            {
                Session["m"] = m.ToString();
            }
            if (l != null)
            {
                Session["l"] = l.ToString();
            }
            if (xl != null)
            {
                Session["xl"] = xl.ToString();
            }
            if (xxl != null)
            {
                Session["xxl"] = xxl.ToString();
            }
            if (xxxl != null)
            {
                Session["xxxl"] = xxxl.ToString();
            }
            if (empatxl != null)
            {
                Session["4xl"] = empatxl.ToString();
            }
            if (limaxl != null)
            {
                Session["5xl"] = limaxl.ToString();
            }

            Session["tambahan1"] = tambahan1;
            Session["tambahan2"] = tambahan2;
            Session["tambahan3"] = tambahan3;
            Session["jmlhtambahan1"] = jmlhtambahan1.ToString();
            Session["jmlhtambahan2"] = jmlhtambahan2.ToString();
            Session["jmlhtambahan3"] = jmlhtambahan3.ToString();
            Session["ukuran1"] = ukuran1;
            Session["ukuran2"] = ukuran2;
            Session["ukuran3"] = ukuran3;

            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["ECommerce"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "SELECT [harga]" +

                    " FROM[MultiTenancy_Sablon].[dbo].[TabelTambahan_" + subdomain + "] WHERE id_produk=" + Session["id_produk"].ToString() + " AND nama_tambahan='XXL'";


                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Session["hargatambahanxxl"] = reader["harga"].ToString();
                        }
                    }
                    sqlcom.CommandText = "SELECT [harga]" +

                 " FROM[MultiTenancy_Sablon].[dbo].[Harga_" + subdomain + "] WHERE id_jns_sablon=" + Session["idsablon"].ToString() + " AND id_bahan=" + Session["idbahan"].ToString() +
                 " AND id_produk=" + Session["id_produk"].ToString();
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Session["hargasatuan"] = reader["harga"].ToString();
                        }
                    }
                    sqlcom.CommandText = "SELECT [harga]" +

                    " FROM[MultiTenancy_Sablon].[dbo].[TabelTambahan_" + subdomain + "] WHERE id_produk=" + Session["id_produk"].ToString() + " AND nama_tambahan='Lebih dari XXL'";
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Session["hargatambahanlebihdarixxl"] = reader["harga"].ToString();
                        }
                    }
                    sqlcom.CommandText = "SELECT [nama_bahan]" +

                    " FROM[MultiTenancy_Sablon].[dbo].[Bahan_" + subdomain + "] WHERE id_bahan=" + Session["idbahan"].ToString();
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Session["namabahan"] = reader["nama_bahan"].ToString();
                        }
                    }
                    sqlcom.CommandText = "SELECT [nama_sablon]" +

                   " FROM[MultiTenancy_Sablon].[dbo].[JenisSablon_" + subdomain + "] WHERE id_jns_sablon=" + Session["idsablon"].ToString();
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Session["namasablon"] = reader["nama_sablon"].ToString();
                        }
                    }

                }

                catch (Exception)
                {

                }
            }


            return RedirectToAction("CalculatePrice", "Produk");
        }


        public ActionResult CreatePesananNonPakaian(string subdomain, int id_produk)
        {

            if (Session["user"] == null)
            {
                Session["idproduklogin"] = id_produk;
                Session["pakaianlogin"] = "tidak";
                return RedirectToAction("Login", "Home", new { idproduk = id_produk, pakaian = "tidak" });
            }
            else
            {
                Session["idproduklogin"] = null;
                Session["pakaianlogin"] = null;
                Session["kategori"] = "nonpakaian";
                Session["id_produk"] = id_produk.ToString();
                var lstbahan = new List<SelectListItem>();
                string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["ECommerce"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connstring))
                {
                    conn.Open();
                    string query = "select p.id_produk, p.nama_produk,b.id_bahan, b.nama_bahan,j.id_jns_sablon, j.nama_sablon from Produk_" + subdomain + " p inner join Bahan_" + subdomain + " b on p.id_produk = b.id_produk" +
                        " inner join JenisSablon_" + subdomain + " j on p.id_produk = j.id_produk where p.id_produk=" + id_produk.ToString() +
                        " group by p.id_produk, p.nama_produk,b.id_bahan, b.nama_bahan,j.id_jns_sablon, j.nama_sablon";




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
                                    Text = reader["nama_bahan"].ToString() + " dengan sablon " + reader["nama_sablon"].ToString()
                                });
                            }
                            ViewBag.Bahan = lstbahan;
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
        public ActionResult CreatePesananNonPakaian(string subdomain, string lstbahan, HttpPostedFileBase desain, int? jumlah, string catatan)
        {

            Session["catatan"] = catatan;
            var idbahan = lstbahan.Split('&')[0];
            Session["idbahan"] = idbahan;
            var idsablon = lstbahan.Split('&')[1];
            Session["idsablon"] = idsablon;

            string filePath = "";
            string fileName = Guid.NewGuid().ToString() + "_" + desain.FileName;
            if (desain.ContentLength > 0)
            {

                filePath = Path.Combine(HttpContext.Server.MapPath("~/Content/Images"), fileName);
                desain.SaveAs(filePath);

            }
            Session["desain"] = fileName;
            Session["jumlah"] = jumlah;
            string query = "SELECT [harga]" +
                 " FROM[MultiTenancy_Sablon].[dbo].[Harga_" + subdomain + "] WHERE id_jns_sablon=" + idsablon.ToString() + " AND id_bahan=" + idbahan.ToString() +
                 " AND id_produk=" + Session["id_produk"].ToString();
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["ECommerce"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();


                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Session["hargasatuan"] = (int)reader["harga"];
                        }
                    }
                    sqlcom.CommandText = "SELECT [nama_bahan]" +

                   " FROM[MultiTenancy_Sablon].[dbo].[Bahan_" + subdomain + "] WHERE id_bahan=" + Session["idbahan"].ToString();
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Session["namabahan"] = reader["nama_bahan"].ToString();
                        }
                    }
                    sqlcom.CommandText = "SELECT [nama_sablon]" +

                   " FROM[MultiTenancy_Sablon].[dbo].[JenisSablon_" + subdomain + "] WHERE id_jns_sablon=" + Session["idsablon"].ToString();
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Session["namasablon"] = reader["nama_sablon"].ToString();
                        }
                    }

                }
                catch (Exception)
                {

                }

                conn.Close();
            }


            return RedirectToAction("CalculatePriceNonPakaian");
        }

        public ActionResult CalculatePrice(string subdomain)
        {
            int hargasatuan = Convert.ToInt32(Session["hargasatuan"]);
            int anak, xs, s, m, l, xl = 0;
            int xxl = 0;
            int xxxl = 0;
            int empatxl = 0;
            int limaxl = 0;
            int jumlahlebihxxl = 0;
            int jumlah = 0;
            int jumlahend = 0;
            int hargatambahan1 = 0;
            int hargatambahan2 = 0;
            int hargatambahan3 = 0;
            int jmlhtambahan1 = 0;
            int jmlhtambahan2 = 0;
            int jmlhtambahan3 = 0;
            int hargaxxl = 0;
            int hargalebihxxl = 0;
            int totalharga = 0;
            int totalhargatambahan = 0;
            int totalhargaall = 0;
           
            if (Session["jmlhtambahan1"].ToString() != "")
            {
                jmlhtambahan1 = Convert.ToInt32(Session["jmlhtambahan1"]);
            }

            if (Session["jmlhtambahan2"].ToString() != "")
            {
                jmlhtambahan2 = Convert.ToInt32(Session["jmlhtambahan2"]);
            }
            if (Session["jmlhtambahan3"].ToString() != "")
            {
                jmlhtambahan3 = Convert.ToInt32(Session["jmlhtambahan3"]);
            }


            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["ECommerce"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "SELECT [harga]" +

                    " FROM[MultiTenancy_Sablon].[dbo].[TabelTambahan_" + subdomain + "] WHERE id_produk=" + Session["id_produk"].ToString() + " AND nama_tambahan='" + Session["tambahan1"].ToString() + "'";


                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            hargatambahan1 = (int)reader["harga"];
                        }
                    }

                    sqlcom.CommandText = "SELECT [harga]" +

                    " FROM[MultiTenancy_Sablon].[dbo].[TabelTambahan_" + subdomain + "] WHERE id_produk=" + Session["id_produk"].ToString() + " AND nama_tambahan='" + Session["tambahan2"].ToString() + "'";
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            hargatambahan2 = (int)reader["harga"];
                        }
                    }
                    sqlcom.CommandText = "SELECT [harga]" +

                    " FROM[MultiTenancy_Sablon].[dbo].[TabelTambahan_" + subdomain + "] WHERE id_produk=" + Session["id_produk"].ToString() + " AND nama_tambahan='" + Session["tambahan3"].ToString() + "'";
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            hargatambahan3 = (int)reader["harga"];
                        }
                    }
                }
                catch (Exception)
                {

                }
            }

            if (Session["anak"] != null)
            {
                anak = Convert.ToInt32(Session["anak"]);
                jumlah += anak;
                ViewBag.anak = anak.ToString();
                int hargaanak = anak * hargasatuan;
                ViewBag.hargaanak = hargaanak.ToString();
            }
            if (Session["xs"] != null)
            {
                xs = Convert.ToInt32(Session["xs"]);
                jumlah += xs;
                ViewBag.xs = xs.ToString();
                int hargaxs = xs * hargasatuan;
                ViewBag.hargaxs = hargaxs.ToString();
            }
            if (Session["s"] != null)
            {
                s = Convert.ToInt32(Session["s"]);
                jumlah += s;
                ViewBag.s = s.ToString();
                int hargas = s * hargasatuan;
                ViewBag.hargas = hargas.ToString();
            }
            if (Session["m"] != null)
            {
                m = Convert.ToInt32(Session["m"]);
                jumlah += m;
                ViewBag.m = m.ToString();
                int hargam = m * hargasatuan;
                ViewBag.hargam = hargam.ToString();
            }
            if (Session["l"] != null)
            {
                l = Convert.ToInt32(Session["l"]);
                jumlah += l;
                ViewBag.l = l.ToString();
                int hargal = l * hargasatuan;
                ViewBag.hargal = hargal.ToString();
            }
            if (Session["xl"] != null)
            {
                xl = Convert.ToInt32(Session["xl"]);
                jumlah += xl;
                ViewBag.xl = xl.ToString();
                int hargaxl = xl * hargasatuan;
                ViewBag.hargaxl = hargaxl.ToString();
            }
            if (Session["xxl"] != null)
            {
                xxl = Convert.ToInt32(Session["xxl"]);
                jumlah += xxl;
                hargaxxl = xxl * Convert.ToInt32(Session["hargatambahanxxl"]);
                ViewBag.xxl = xxl.ToString();
                int hargaxxlsatuan = xxl * hargasatuan;
                ViewBag.hargaxxlsatuan = hargaxxlsatuan.ToString();
            }
            if (Session["xxxl"] != null)
            {
                xxxl = Convert.ToInt32(Session["xxxl"]);
                jumlah += xxxl;
                hargalebihxxl += xxxl * Convert.ToInt32(Session["hargatambahanlebihdarixxl"]);
                ViewBag.xxxl = xxxl.ToString();
                int hargaxxxlsatuan = xxxl * hargasatuan;
                ViewBag.hargaxxxlsatuan = hargaxxxlsatuan.ToString();
            }
            if (Session["4xl"] != null)
            {
                empatxl = Convert.ToInt32(Session["4xl"]);
                jumlah += empatxl;
                hargalebihxxl += empatxl * Convert.ToInt32(Session["hargatambahanlebihdarixxl"]);
                ViewBag.empatxl = empatxl.ToString();
                int harga4xl = empatxl * hargasatuan;
                ViewBag.harga4xl = harga4xl.ToString();
            }
            if (Session["5xl"] != null)
            {
                limaxl = Convert.ToInt32(Session["5xl"]);
                jumlah += limaxl;
                hargalebihxxl += limaxl * Convert.ToInt32(Session["hargatambahanlebihdarixxl"]);
                ViewBag.limexl = limaxl.ToString();
                int harga5xl = limaxl * hargasatuan;
                ViewBag.harga5xl = harga5xl.ToString();
            }

            jumlahend = jumlah + jmlhtambahan1 + jmlhtambahan2 + jmlhtambahan3;
            ViewBag.Jumlahtotal = jumlahend.ToString();

            jumlahlebihxxl = xxxl + empatxl + limaxl;
            ViewBag.lebihxxl = jumlahlebihxxl.ToString();          
            ViewBag.hargaxxl = hargaxxl.ToString();
            ViewBag.hargalebihxxl = hargalebihxxl.ToString();

            totalharga = hargasatuan * jumlah;

            Session["jumlahorder"] = jumlahend.ToString();
            ViewBag.totalharga = totalharga.ToString();

            int tothar1 = (hargasatuan + hargatambahan1) * jmlhtambahan1;
            ViewBag.tothar1 = tothar1.ToString();
            int tothar2 = (hargasatuan + hargatambahan2) * jmlhtambahan2;
            ViewBag.tothar2 = tothar2.ToString();
            int tothar3 = (hargasatuan+hargatambahan3) * jmlhtambahan3;
            ViewBag.tothar3 = tothar3.ToString();

            int totalharganotambahan = totalharga + tothar1 + tothar2 + tothar3;
            ViewBag.totalharganotambahan = totalharganotambahan.ToString();

            totalhargatambahan = hargaxxl + hargalebihxxl;
            totalhargaall = totalharganotambahan + totalhargatambahan;
            ViewBag.Totalhargaall = totalhargaall.ToString();
            Session["totalhargaall"] = totalhargaall.ToString();
            return View();
        }
        public ActionResult CalculatePriceNonPakaian(string subdomain)
        {
            int hargasatuan = Convert.ToInt32(Session["hargasatuan"]);
            int jumlah = Convert.ToInt32(Session["jumlah"]);
            int hargatotal = hargasatuan * jumlah;
            ViewBag.hargasatuan = hargasatuan.ToString();
            ViewBag.jumlah = jumlah.ToString();
            Session["jumlahorder"] = jumlah.ToString();
            ViewBag.hargatotal = hargatotal.ToString();
            Session["totalhargaall"] = hargatotal.ToString();
            return View();
        }
    }
}
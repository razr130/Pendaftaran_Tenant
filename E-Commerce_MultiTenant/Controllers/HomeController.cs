using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using E_Commerce_MultiTenant.DAL;
using E_Commerce_MultiTenant.Models;
using System.IO;


namespace E_Commerce_MultiTenant.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //var host = this.Request.Headers["Host"].Split('.');
            //string nama_perusahaan = host[0];
            //Session["nama_perusahaan"] = subdomain;

            //return RedirectToAction("Indexku","Home");
            return RedirectToAction("Indexku","Home");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public string getWarnaNavbar(string subdo)
        {
            using (PenyewaDAL penyewa = new PenyewaDAL())
            {
                int id_penyewa = penyewa.GetIDPenyewa(subdo);
                int id_ui = penyewa.GetIDUI(id_penyewa);

                string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["Ecommerce"].ConnectionString;
                var result = "";

                using (SqlConnection conn = new SqlConnection(connstring))
                {
                    conn.Open();
                    string query = "SELECT [warna_navbar]" +

                        "FROM[MultiTenancy_Sablon].[dbo].[Data_UI] WHERE id_ui=" + id_ui.ToString();

                    SqlCommand sqlcom = new SqlCommand(query, conn);
                    try
                    {
                        using (SqlDataReader reader = sqlcom.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                                result = reader["warna_navbar"].ToString();
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
        public string getWarnaBG(string subdo)
        {
            using (PenyewaDAL penyewa = new PenyewaDAL())
            {
                int id_penyewa = penyewa.GetIDPenyewa(subdo);
                int id_ui = penyewa.GetIDUI(id_penyewa);

                string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["Ecommerce"].ConnectionString;
                var result = "";

                using (SqlConnection conn = new SqlConnection(connstring))
                {
                    conn.Open();
                    string query = "SELECT [warna_bg]" +

                        "FROM[MultiTenancy_Sablon].[dbo].[Data_UI] WHERE id_ui=" + id_ui.ToString();

                    SqlCommand sqlcom = new SqlCommand(query, conn);
                    try
                    {
                        using (SqlDataReader reader = sqlcom.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                                result = reader["warna_bg"].ToString();
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
        public string getLogo(string subdo)
        {
            using (PenyewaDAL penyewa = new PenyewaDAL())
            {
                int id_penyewa = penyewa.GetIDPenyewa(subdo);
                int id_ui = penyewa.GetIDUI(id_penyewa);

                string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["Ecommerce"].ConnectionString;
                var result = "";

                using (SqlConnection conn = new SqlConnection(connstring))
                {
                    conn.Open();
                    string query = "SELECT [logo]" +

                        "FROM[MultiTenancy_Sablon].[dbo].[Data_UI] WHERE id_ui=" + id_ui.ToString();

                    SqlCommand sqlcom = new SqlCommand(query, conn);
                    try
                    {
                        using (SqlDataReader reader = sqlcom.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                                result = reader["logo"].ToString();
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

        public ActionResult Indexku(string subdomain)
        {
            Session["nama_perusahaan"] = subdomain;
            Session["warnanavbar"] = getWarnaNavbar(subdomain);
            Session["warnabg"] = getWarnaBG(subdomain);
            Session["logo"] = getLogo(subdomain);
            using (PenyewaDAL penyewa = new PenyewaDAL())
            {
                int id_penyewa = penyewa.GetIDPenyewa(subdomain);
                int id_ui = penyewa.GetIDUI(id_penyewa);
                string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["Ecommerce"].ConnectionString;
                List<DataCarausel> result = new List<DataCarausel>();
                using (SqlConnection conn = new SqlConnection(connstring))
                {
                    conn.Open();
                    string query = "SELECT [id_carausel]" +
                        ",[id_ui]" +
                        ",[gambar]" +
                        "FROM[MultiTenancy_Sablon].[dbo].[Data_carausel] WHERE id_ui=" + id_ui.ToString();
                    SqlCommand sqlcom = new SqlCommand(query, conn);
                    try
                    {
                        using (SqlDataReader reader = sqlcom.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DataCarausel item = new DataCarausel()
                                {
                                    id_carausel = (int)reader["id_carausel"],
                                    id_ui = (int)reader["id_ui"],
                                    gambar = reader["gambar"].ToString(),
                                };
                                result.Add(item);
                            }
                        }
                        sqlcom.CommandText = "SELECT [email] FROM [MultiTenancy_Sablon].[dbo].[Penyewa] WHERE nama_perusahaan='"
                            + subdomain + "'";
                        using (SqlDataReader reader = sqlcom.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Session["emailperusahaan"] = reader["email"].ToString();
                            }                       
                        }
                        sqlcom.CommandText = "SELECT TOP (1) [nama_produk],[foto_produk]" +
                            " FROM[MultiTenancy_Sablon].[dbo].[Produk_"+subdomain+"]" +
                            " ORDER BY NEWID()";
                        using (SqlDataReader reader = sqlcom.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                ViewBag.namaproduk1 = reader["nama_produk"].ToString();
                                ViewBag.fotoproduk1 = reader["foto_produk"].ToString();
                            }
                        }
                        sqlcom.CommandText = "SELECT TOP (1) [nama_produk],[foto_produk]" +
                          " FROM[MultiTenancy_Sablon].[dbo].[Produk_" + subdomain + "]" +
                          " ORDER BY NEWID()";
                        using (SqlDataReader reader = sqlcom.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                ViewBag.namaproduk2 = reader["nama_produk"].ToString();
                                ViewBag.fotoproduk2 = reader["foto_produk"].ToString();
                            }
                        }
                        sqlcom.CommandText = "SELECT TOP (1) [nama_produk],[foto_produk]" +
                          " FROM[MultiTenancy_Sablon].[dbo].[Produk_" + subdomain + "]" +
                          " ORDER BY NEWID()";
                        using (SqlDataReader reader = sqlcom.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                ViewBag.namaproduk3 = reader["nama_produk"].ToString();
                                ViewBag.fotoproduk3 = reader["foto_produk"].ToString();
                            }
                        }
                        sqlcom.CommandText = "SELECT TOP (1) [nama_produk],[foto_produk]" +
                          " FROM[MultiTenancy_Sablon].[dbo].[Produk_" + subdomain + "]" +
                          " ORDER BY NEWID()";
                        using (SqlDataReader reader = sqlcom.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                ViewBag.namaproduk4 = reader["nama_produk"].ToString();
                                ViewBag.fotoproduk4 = reader["foto_produk"].ToString();
                            }
                        }
                        sqlcom.CommandText = "SELECT TOP (1) [nama_produk],[foto_produk]" +
                          " FROM[MultiTenancy_Sablon].[dbo].[Produk_" + subdomain + "]" +
                          " ORDER BY NEWID()";
                        using (SqlDataReader reader = sqlcom.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                ViewBag.namaproduk5 = reader["nama_produk"].ToString();
                                ViewBag.fotoproduk5 = reader["foto_produk"].ToString();
                            }
                        }
                        sqlcom.CommandText = "SELECT TOP (1) [nama_produk],[foto_produk]" +
                          " FROM[MultiTenancy_Sablon].[dbo].[Produk_" + subdomain + "]" +
                          " ORDER BY NEWID()";
                        using (SqlDataReader reader = sqlcom.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                ViewBag.namaproduk6 = reader["nama_produk"].ToString();
                                ViewBag.fotoproduk6 = reader["foto_produk"].ToString();
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
        }

        public ActionResult Login()
        {
          
            
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password, string subdomain)
        {
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["Ecommerce"].ConnectionString;
            var result = "";

            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "SELECT [nama_karyawan]" +
                    "FROM[MultiTenancy_Sablon].[dbo].[Karyawan_" + subdomain + "] WHERE email_karyawan='" + email + "' AND password='" + password + "'";
                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result = reader["nama_karyawan"].ToString();
                            Session["user"] = result;
                            Session["email"] = email;
                            Session["role"] = "karyawan";
                        }
                    }
                }
                catch (Exception)
                {
                }
                if (result != "")
                {
                    return RedirectToAction("Indexku", "Home");
                }
                else
                {
                    sqlcom.CommandText = "SELECT [nama_customer],[alamat]" +

                    " FROM[MultiTenancy_Sablon].[dbo].[Customer_" + subdomain + "] WHERE email_customer='" + email + "' AND password='" + password + "'";
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result = reader["nama_customer"].ToString();
                            Session["alamat"] = reader["alamat"].ToString();
                            Session["user"] = result;
                            Session["email"] = email;
                            Session["role"] = "customer";
                        }
                    }                  
                    conn.Close();
                    if (result != "")
                    {
                        if(Session["idproduklogin"] != null)
                        {
                            if(Session["pakaianlogin"].ToString() == "ya")
                            { 
                            return Redirect("http://"+ subdomain +".multitenant.local:58131/Produk/CreatePesananPakaian/?id_produk=" + (int)Session["idproduklogin"]);
                            }
                            else
                            {
                                return Redirect("http://" + subdomain + ".multitenant.local:58131/Produk/CreatePesananNonPakaian/?id_produk=" + (int)Session["idproduklogin"]);
                            }
                        }
                        else
                        {
                            return RedirectToAction("Indexku", "Home");
                        }            
                    }
                    else
                    {
                        TempData["Pesan"] = Helpers.Message.GetPesanLogin("Login gagal !",
                                              "danger", "Email atau password salah.");
                        return View();
                    }
                    
                }
               
            }
            
        }

        public ActionResult Logout()
        {
            Session["user"] = null;
            Session["role"] = null;
            return RedirectToAction("Indexku", "Home");
        }

        public ActionResult Create()
        {

            return View();
        }
        [HttpPost]
        public ActionResult Create(string subdomain, string nama, string email, string password, string tempatlahir, string tanggal, string bulan, string tahun, string notelp, string alamat, string jns_kelamin)
        {
            string tgllahir = tanggal + "-" + bulan + "-" + tahun;
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["ECommerce"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "USE [MultiTenancy_Sablon]" +
                    "INSERT INTO[dbo].[Customer_" + subdomain + "]" +
                    " ([nama_customer]" +
                    ",[email_customer]" +
                    ",[password]" +
                    ",[tempat_lahir]" +
                    ",[tgl_lahir]" +
                    ",[no_telp]" +
                    ",[alamat]" +
                    ",[jns_kelamin])" +

                    " VALUES" +
                    "('" + nama + "' ,'" + email + "' ,'" + password + "' ,'" + tempatlahir + "' ,'" + tgllahir + "' ,'" + notelp + "' ,'" + alamat + "' ,'" + jns_kelamin + "')";

                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    sqlcom.ExecuteNonQuery();
                    Session["user"] = nama;
                    Session["role"] = "customer";
                    Session["email"] = email;
                    Session["alamat"] = alamat;
                    TempData["Pesan"] = Helpers.Message.GetPesan("Berhasil !",
                                          "success", "Pendaftaran akun berhasil !");
                }
                catch (Exception ex)
                {
                    TempData["Pesan"] = Helpers.Message.GetPesan("Error !",
                                          "danger", ex.Message);
                }

                conn.Close();
            }
            if (Session["idproduklogin"] != null)
            {
                if (Session["pakaianlogin"].ToString() == "ya")
                {
                    return Redirect("http://" + subdomain + ".multitenant.local:58131/Produk/CreatePesananPakaian/?id_produk=" + (int)Session["idproduklogin"]);
                    
                }
                else
                {
                    return Redirect("http://" + subdomain + ".multitenant.local:58131/Produk/CreatePesananNonPakaian/?id_produk=" + (int)Session["idproduklogin"]);
                }
            }
            else
            {
                return RedirectToAction("Konfirmasi", "Home");
            }
            
        }

        public ActionResult Konfirmasi()
        {
            return View();
        }
    }
}
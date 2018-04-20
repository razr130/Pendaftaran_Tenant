using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pendaftaran_Tenant.Models;
using Pendaftaran_Tenant.DAL;
using System.Data.SqlClient;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using System.IO;

namespace Pendaftaran_Tenant.Controllers
{
    public class PenyewaController : Controller
    {
        private PendaftaranTenant db = new PendaftaranTenant();
        // GET: Penyewa
        public ActionResult Index()
        {

            using (PenyewaDAL svpenyewa = new PenyewaDAL())
            {
                var result = svpenyewa.GetData().ToList();
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

        public ActionResult Login(Penyewa penyewa)
        {
            var results = db.Penyewas.SingleOrDefault(m => m.email == penyewa.email && m.password == penyewa.password);
            if (results != null)
            {
                if (results.status_bayar == false)
                {
                    TempData["Pesan"] = Helpers.Message.GetPesan(" ",
                      "danger", "Maaf, pembayaran anda belum dikonfirmasi");
                    return View();
                }
                else
                {
                    Session["user"] = penyewa;
                    using (PenyewaDAL sewa = new PenyewaDAL())
                    {

                        Session["role"] = sewa.getRole(penyewa.email).ToString();
                        Session["id_penyewa"] = sewa.getId(penyewa.email).ToString();
                    }
                    string nama_perusahaan;
                    int idpenyewa = Convert.ToInt32(Session["id_penyewa"]);

                    using (PenyewaDAL sewa = new PenyewaDAL())
                    {

                        nama_perusahaan = sewa.getNamaPerusahaan(idpenyewa).ToString();
                        Session["nama_perusahaan"] = nama_perusahaan;

                    }
                    if (Session["role"].ToString() == "Admin")
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return RedirectToAction("CreateUI", "Penyewa");
                    }
                   
                }
            }
            else
            {
                TempData["Pesan"] = Helpers.Message.GetPesan(" ",
                      "danger", "Email atau password salah");

            }
            return View();
        }

        public ActionResult Logout()
        {
            Session["user"] = null;
            Session["role"] = null;
            return RedirectToAction("Index", "Home");
        }
        public ActionResult Create()
        {

            return View();
        }
        [HttpPost]
        public ActionResult Create(Penyewa penyewa)
        {
            penyewa.status_bayar = false;
            penyewa.role = "Penyewa";
            using (PenyewaDAL svPny = new PenyewaDAL())
            {
                try
                {
                    svPny.Add(penyewa);
                    //TempData["Pesan"] = Helpers.Message.GetPesan("Sukses !",
                    //  "success", "Data Barang " + barang.nama_barang + " berhasil ditambah");
                }
                catch (Exception ex)
                {
                    TempData["Pesan"] = Helpers.Message.GetPesan("Error !",
                                          "danger", ex.Message);
                }
            }
            return RedirectToAction("Index", "Home");
        }
        public ActionResult Edit(int id)
        {
            using (PenyewaDAL pny = new PenyewaDAL())
            {
                var editstatus = new Penyewa
                {
                    id_penyewa = id,
                    status_bayar = true
                };
                pny.Edit(editstatus);
            }
            return RedirectToAction("Index");
        }
        public ActionResult CreateUI()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateUI(Data_UI dataui, HttpPostedFileBase uploadimage, string color1, string color2)
        {
            string filePath = "";
            if (uploadimage.ContentLength > 0)
            {
                string fileName = Guid.NewGuid().ToString() + "_" + uploadimage.FileName;
                filePath = Path.Combine(HttpContext.Server.MapPath("~/Content/Images"), fileName);
                uploadimage.SaveAs(filePath);
                dataui.logo = fileName;
            }
            dataui.warna_bg = color1;
            dataui.warna_navbar = color2;
            dataui.id_penyewa = Convert.ToInt32(Session["id_penyewa"]);
            using (PenyewaDAL svBrg = new PenyewaDAL())
            {
                try
                {
                    svBrg.AddUI(dataui);
                    TempData["Pesan"] = Helpers.Message.GetPesan("Sukses !",
                      "success", "Data UI berhasil ditambah");
                    Session["id_ui"] = svBrg.getIdUI(Convert.ToInt32(Session["id_penyewa"]));
                }
                catch (Exception ex)
                {
                    TempData["Pesan"] = Helpers.Message.GetPesan("Error !",
                                          "danger", ex.Message);
                }
            }
            return RedirectToAction("CreateCarausel");
        }
        public int GetCount()
        {
            string stmt = "SELECT COUNT(gambar) FROM Data_carausel WHERE id_ui = " + Session["id_ui"].ToString() + ";";
            int count = 0;
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;

            using (SqlConnection thisConnection = new SqlConnection(connstring))
            {
                using (SqlCommand cmdCount = new SqlCommand(stmt, thisConnection))
                {
                    thisConnection.Open();
                    count = (int)cmdCount.ExecuteScalar();
                }
            }
            return count;
        }
        public ActionResult CreateCarausel()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCarausel(Data_carausel datacarausel, HttpPostedFileBase uploadimage)
        {
           if(GetCount() == 4)
            {
                TempData["Pesan"] = Helpers.Message.GetPesan("Melebihi kapasitas",
                          "danger", "Anda sudah memasukkan 4 gambar carausel");
                return View();

            }
           else
            {
                string filePath = "";
                if (uploadimage.ContentLength > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + "_" + uploadimage.FileName;
                    filePath = Path.Combine(HttpContext.Server.MapPath("~/Content/Images"), fileName);
                    uploadimage.SaveAs(filePath);
                    datacarausel.gambar = fileName;
                }

                datacarausel.id_ui = Convert.ToInt32(Session["id_ui"]);
                using (PenyewaDAL svBrg = new PenyewaDAL())
                {
                    try
                    {
                        svBrg.AddCarausel(datacarausel);
                        TempData["Pesan"] = Helpers.Message.GetPesan("Sukses !",
                          "success", "Data UI berhasil ditambah");
                    }
                    catch (Exception ex)
                    {
                        TempData["Pesan"] = Helpers.Message.GetPesan("Error !",
                                              "danger", ex.Message);
                    }
                }
                return View();
            }
                
        }
        public ActionResult CreateProduk()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateProduk(string namaproduk, string deskripsi, HttpPostedFileBase fotoproduk)
        {
            string filePath = "";
            string fileName = Guid.NewGuid().ToString() + "_" + fotoproduk.FileName;
            if (fotoproduk.ContentLength > 0)
            {
                
                filePath = Path.Combine(HttpContext.Server.MapPath("~/Content/Images"), fileName);
                fotoproduk.SaveAs(filePath);
                
            }
            string nama_perusahaan;
            int id = Convert.ToInt32(Session["id_penyewa"]);

            using (PenyewaDAL sewa = new PenyewaDAL())
            {

                nama_perusahaan = sewa.getNamaPerusahaan(id).ToString();
            }
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "USE [MultiTenancy_Sablon]" +
                    "INSERT INTO[dbo].[Produk_"+ nama_perusahaan +"]" +
                    "([nama_produk]" +
                    ",[deskripsi]" +
                    ",[foto_produk])" +
                    "VALUES" +
                    "('" + namaproduk + "' ,'"+ deskripsi +"' ,'"+ fileName +"')";

                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    sqlcom.ExecuteNonQuery();
                    TempData["Pesan"] = Helpers.Message.GetPesan("Berhasil !",
                                          "success", "data produk berhasil ditambah");
                }
                catch (Exception ex)
                {
                    TempData["Pesan"] = Helpers.Message.GetPesan("Error !",
                                          "danger", ex.Message);
                }

                conn.Close();
            }
                return View();
        }



        public ActionResult IndexProduk()
        {
            string nama_perusahaan = Session["nama_perusahaan"].ToString();
            //string nama_perusahaan = "b";            


            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            List<Produk> result = new List<Produk>();
            
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "SELECT [id_produk]" +
                    ",[nama_produk]" +
                    ",[deskripsi]" +
                    ",[foto_produk]" +
                    "FROM[MultiTenancy_Sablon].[dbo].[Produk_" + nama_perusahaan + "]";

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

        public string GetNamaproduk()
        {
            string nama_perusahaan = Session["nama_perusahaan"].ToString();
            //string nama_perusahaan = "b";
            
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            string result = "";

            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "SELECT [nama_produk]" +
                    " FROM[MultiTenancy_Sablon].[dbo].[Produk_" + nama_perusahaan + "] WHERE [id_produk]=" + Session["id_produk"].ToString();

                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        if(reader.Read())
                        {
                            result = string.Format("{0}", reader["nama_produk"]);
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

        public ActionResult IndexBahan()
        {


            string nama_perusahaan = Session["nama_perusahaan"].ToString();
            

            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            List<Bahan> result = new List<Bahan>();

            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "SELECT [id_bahan]" +
                    ",[id_produk]" +
                    ",[nama_bahan]" +
                    " FROM[MultiTenancy_Sablon].[dbo].[Bahan_" + nama_perusahaan + "]";

                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Bahan item = new Bahan()
                            {
                                id_bahan = (int)reader["id_produk"],
                                id_produk = (int)reader["id_produk"],
                                nama_bahan = reader["nama_bahan"].ToString()
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
        public ActionResult CreateBahan(int id)
        {
            Session["id_produk"] = id;
            Session["nama_produk"] = GetNamaproduk().ToString();

            return View();
        }

        [HttpPost]
        public ActionResult CreateBahan(string namabahan)
        {
            int idproduk = (int)Session["id_produk"];
            string nama_perusahaan;
            int id = Convert.ToInt32(Session["id_penyewa"]);

            using (PenyewaDAL sewa = new PenyewaDAL())
            {

                nama_perusahaan = sewa.getNamaPerusahaan(id).ToString();
            }
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "USE [MultiTenancy_Sablon]" +
                    "INSERT INTO[dbo].[Bahan_" + nama_perusahaan + "]" +
                    "([id_produk]" +
                    ",[nama_bahan])" +
                    " VALUES" +
                    "('" + idproduk + "' ,'" + namabahan + "')";

                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    sqlcom.ExecuteNonQuery();
                    TempData["Pesan"] = Helpers.Message.GetPesan("Berhasil !",
                                          "success", "data produk berhasil ditambah");
                }
                catch (Exception ex)
                {
                    TempData["Pesan"] = Helpers.Message.GetPesan("Error !",
                                          "danger", ex.Message);
                }

                conn.Close();
            }



            return RedirectToAction("IndexBahan","Penyewa");
        }

        public ActionResult CreateBahan2()
        {        

            return View();
        }

        [HttpPost]
        public ActionResult CreateBahan2(string namabahan)
        {
            int idproduk = (int)Session["id_produk"];
            string nama_perusahaan;
            int id = Convert.ToInt32(Session["id_penyewa"]);

            using (PenyewaDAL sewa = new PenyewaDAL())
            {

                nama_perusahaan = sewa.getNamaPerusahaan(id).ToString();
            }
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "USE [MultiTenancy_Sablon]" +
                    "INSERT INTO[dbo].[Bahan_" + nama_perusahaan + "]" +
                    "([id_produk]" +
                    ",[nama_bahan])" +
                    "VALUES" +
                    "('" + idproduk + "' ,'" + namabahan + "')";

                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    sqlcom.ExecuteNonQuery();
                    TempData["Pesan"] = Helpers.Message.GetPesan("Berhasil !",
                                          "success", "data produk berhasil ditambah");
                }
                catch (Exception ex)
                {
                    TempData["Pesan"] = Helpers.Message.GetPesan("Error !",
                                          "danger", ex.Message);
                }

                conn.Close();
            }



            return RedirectToAction("IndexBahan", "Penyewa");
        }

        public ActionResult IndexJenisSablon()
        {


            string nama_perusahaan = Session["nama_perusahaan"].ToString();


            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            List<JenisSablon> result = new List<JenisSablon>();

            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "SELECT [id_jns_sablon]" +
                    ",[id_produk]" +
                    ",[nama_sablon]" +
                    " FROM[MultiTenancy_Sablon].[dbo].[JenisSablon_" + nama_perusahaan + "]";

                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            JenisSablon item = new JenisSablon()
                            {
                                id_jns_sablon = (int)reader["id_jns_sablon"],
                                id_produk = (int)reader["id_produk"],
                                nama_sablon = reader["nama_sablon"].ToString()
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
        public ActionResult CreateJenisSablon(int id)
        {
            Session["id_produk"] = id;
            Session["nama_produk"] = GetNamaproduk().ToString();

            return View();
        }

        [HttpPost]
        public ActionResult CreateJenisSablon(string namajenissablon)
        {
            int idproduk = (int)Session["id_produk"];
            string nama_perusahaan;
            int id = Convert.ToInt32(Session["id_penyewa"]);

            using (PenyewaDAL sewa = new PenyewaDAL())
            {

                nama_perusahaan = sewa.getNamaPerusahaan(id).ToString();
            }
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "USE [MultiTenancy_Sablon]" +
                    "INSERT INTO[dbo].[JenisSablon_" + nama_perusahaan + "]" +
                    "([id_produk]" +
                    ",[nama_sablon])" +
                    " VALUES" +
                    "('" + idproduk + "' ,'" + namajenissablon + "')";

                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    sqlcom.ExecuteNonQuery();
                    TempData["Pesan"] = Helpers.Message.GetPesan("Berhasil !",
                                          "success", "data produk berhasil ditambah");
                }
                catch (Exception ex)
                {
                    TempData["Pesan"] = Helpers.Message.GetPesan("Error !",
                                          "danger", ex.Message);
                }

                conn.Close();
            }



            return RedirectToAction("IndexJenisSablon", "Penyewa");
        }

        public ActionResult CreateJenisSablon2()
        {

            return View();
        }

        [HttpPost]
        public ActionResult CreateJenisSablon2(string namajenissablon)
        {
            int idproduk = (int)Session["id_produk"];
            string nama_perusahaan;
            int id = Convert.ToInt32(Session["id_penyewa"]);

            using (PenyewaDAL sewa = new PenyewaDAL())
            {

                nama_perusahaan = sewa.getNamaPerusahaan(id).ToString();
            }
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "USE [MultiTenancy_Sablon]" +
                    "INSERT INTO[dbo].[JenisSablon_" + nama_perusahaan + "]" +
                    "([id_produk]" +
                    ",[nama_sablon])" +
                    "VALUES" +
                    "('" + idproduk + "' ,'" + namajenissablon + "')";

                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    sqlcom.ExecuteNonQuery();
                    TempData["Pesan"] = Helpers.Message.GetPesan("Berhasil !",
                                          "success", "data produk berhasil ditambah");
                }
                catch (Exception ex)
                {
                    TempData["Pesan"] = Helpers.Message.GetPesan("Error !",
                                          "danger", ex.Message);
                }

                conn.Close();
            }



            return RedirectToAction("IndexJenisSablon", "Penyewa");
        }
        public ActionResult Addtable(int id)
        {
            string nama_perusahaan;
            using (PenyewaDAL sewa = new PenyewaDAL())
            {

                nama_perusahaan = sewa.getNamaPerusahaan(id).ToString();
            }
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "USE [MultiTenancy_Sablon]" + 

                    

                    " SET ANSI_NULLS ON" +

                    

                    " SET QUOTED_IDENTIFIER ON" +

                    

                    " CREATE TABLE[dbo].[Karyawan_" + nama_perusahaan + "](" +

                    "[id_karyawan][int] IDENTITY(1, 1) NOT NULL," +

                    "[nama_karyawan] [varchar] (50) NULL," +

                    "[email_karyawan] [varchar] (50) NULL," +

                    "[password] [varchar] (50) NULL," +

                    "[tempat_lahir] [varchar] (15) NULL," +

                    "[tgl_lahir] [date] NULL," +

                    "[no_telp] [char](12) NULL," +

                    "[alamat] [varchar] (100) NULL," +

                    "[jns_kelamin] [char](1) NULL," +

                    "CONSTRAINT[PK_Karyawan_" + nama_perusahaan + "] PRIMARY KEY CLUSTERED" +

                    "(" +

                    "[id_karyawan] ASC" +

                    ")WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]" +

                    ") ON[PRIMARY]" +

                    

                    " USE[MultiTenancy_Sablon]" +

                    

                    " SET ANSI_NULLS ON" +

                    

                    " SET QUOTED_IDENTIFIER ON" +

                    

                    " CREATE TABLE[dbo].[Customer_" + nama_perusahaan + "]" +

                    "(" +

                    "[id_cutomer][int] IDENTITY(1,1) NOT NULL," +

                    "[nama_customer] [varchar] (50) NULL," +

                    "[email_customer] [varchar] (50) NULL," +

                    "[password] [varchar] (50) NULL," +

                    "[tempat_lahir] [varchar] (15) NULL," +

                    "[tgl_lahir] [date] NULL," +

                    "[no_telp] [char](12) NULL," +

                    "[alamat] [varchar] (100) NULL," +

                    "[jns_kelamin] [char](1) NULL," +

                    "CONSTRAINT[PK_Customer_" + nama_perusahaan + "] PRIMARY KEY CLUSTERED" +

                    "(" +

                    "[id_cutomer] ASC" +

                    ")WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]" +

                    ") ON[PRIMARY]" +

                    

                    " USE[MultiTenancy_Sablon]" +

                    

                    " SET ANSI_NULLS ON" +

                    
                    " SET QUOTED_IDENTIFIER ON" +
                    
                    " CREATE TABLE[dbo].[Produk_" + nama_perusahaan + "]" +
                    "(" +
                    "[id_produk][int] IDENTITY(1,1) NOT NULL," +
                    "[nama_produk] [varchar] (50) NULL," +
                    "[deskripsi] [varchar] (100) NULL," +
                    "[foto_produk] [varchar] (100) NULL," +
                    "CONSTRAINT[PK_Produk_" + nama_perusahaan + "] PRIMARY KEY CLUSTERED" +
                    "(" +
                    "[id_produk] ASC" +
                    ")WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]" +
                    ") ON[PRIMARY]" +
                    
                    " USE[MultiTenancy_Sablon]" +
                    
                    " SET ANSI_NULLS ON" +
                    
                    " SET QUOTED_IDENTIFIER ON" +
                    
                    " CREATE TABLE[dbo].[Bahan_" + nama_perusahaan + "]" +
                    "(" +
                    "[id_bahan][int] IDENTITY(1,1) NOT NULL," +
                    "[id_produk] [int] NOT NULL," +
                    "[nama_bahan] [varchar] (50) NULL," +
                    "CONSTRAINT[PK_Bahan_" + nama_perusahaan + "] PRIMARY KEY CLUSTERED" +
                    "(" +
                    "[id_bahan] ASC" +
                    ")WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]" +
                    ") ON[PRIMARY]" +
                    
                    " ALTER TABLE[dbo].[Bahan_" + nama_perusahaan + "] WITH CHECK ADD CONSTRAINT[FK_Bahan_" + nama_perusahaan + "_Produk_" + nama_perusahaan + "] FOREIGN KEY([id_produk])" +
                    " REFERENCES[dbo].[Produk_" + nama_perusahaan + "]" +
                    "([id_produk])" +
                    
                    " ALTER TABLE[dbo].[Bahan_" + nama_perusahaan + "]" +
                    " CHECK CONSTRAINT[FK_Bahan_" + nama_perusahaan + "_Produk_" + nama_perusahaan + "]" +
                    
                    " USE[MultiTenancy_Sablon]" +
                    
                    " SET ANSI_NULLS ON" +
                    
                    " SET QUOTED_IDENTIFIER ON" +
                    
                    " CREATE TABLE[dbo].[JenisSablon_" + nama_perusahaan + "]" +
                    "(" +
                    "[id_jns_sablon][int] IDENTITY(1,1) NOT NULL," +
                    "[id_produk] [int] NULL," +
                    "[nama_sablon] [varchar] (50) NULL," +
                    "CONSTRAINT[PK_JenisSablon_" + nama_perusahaan + "] PRIMARY KEY CLUSTERED" +
                    "(" +
                    "[id_jns_sablon] ASC" +
                    ")WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]" +
                    ") ON[PRIMARY]" +
                    
                    " ALTER TABLE[dbo].[JenisSablon_" + nama_perusahaan + "] WITH CHECK ADD CONSTRAINT[FK_JenisSablon_" + nama_perusahaan + "_Produk_" + nama_perusahaan + "] FOREIGN KEY([id_produk])" +
                    " REFERENCES[dbo].[Produk_" + nama_perusahaan + "]" +
                    "([id_produk])" +
                    
                    " ALTER TABLE[dbo].[JenisSablon_" + nama_perusahaan + "]" +
                    " CHECK CONSTRAINT[FK_JenisSablon_" + nama_perusahaan + "_Produk_" + nama_perusahaan + "]" +
                    
                    " USE[MultiTenancy_Sablon]" +
                    
                    " SET ANSI_NULLS ON" +
                    
                    " SET QUOTED_IDENTIFIER ON" +
                    
                    " CREATE TABLE[dbo].[Harga_" + nama_perusahaan + "]" +
                    "([id_harga] [int] IDENTITY(1,1) NOT NULL," +
                    "[id_produk][int] NULL," +
                    "[id_bahan][int] NULL," +
                    "[id_jns_sablon][int] NULL," +
                    "[harga][int] NULL," +
                    "CONSTRAINT[PK_Harga_" + nama_perusahaan + "]" + "PRIMARY KEY CLUSTERED" +
                    "(" +
                    "[id_harga] ASC" +
                    ")WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]" +
                   
                    ") ON[PRIMARY]" +
                    
                    " ALTER TABLE[dbo].[Harga_" + nama_perusahaan + "] WITH CHECK ADD CONSTRAINT[FK_Harga_" + nama_perusahaan + "_Bahan_" + nama_perusahaan + "] FOREIGN KEY([id_bahan])" +
                    " REFERENCES[dbo].[Bahan_" + nama_perusahaan + "]" +
                    "([id_bahan])" +
                    
                    " ALTER TABLE[dbo].[Harga_" + nama_perusahaan + "]" +
                    " CHECK CONSTRAINT[FK_Harga_" + nama_perusahaan + "_Bahan_" + nama_perusahaan + "]" +
                    
                    " ALTER TABLE[dbo].[Harga_" + nama_perusahaan + "] WITH CHECK ADD CONSTRAINT[FK_Harga_" + nama_perusahaan + "_JenisSablon_" + nama_perusahaan + "] FOREIGN KEY([id_jns_sablon])" +
                    " REFERENCES[dbo].[JenisSablon_" + nama_perusahaan + "]" +
                    "([id_jns_sablon])" +
                    
                    " ALTER TABLE[dbo].[Harga_" + nama_perusahaan + "]" +
                    " CHECK CONSTRAINT[FK_Harga_" + nama_perusahaan + "_JenisSablon_" + nama_perusahaan + "]" +
                    
                    " ALTER TABLE[dbo].[Harga_" + nama_perusahaan + "] WITH CHECK ADD CONSTRAINT[FK_Harga_" + nama_perusahaan + "_Produk_" + nama_perusahaan + "] FOREIGN KEY([id_produk])" +
                    " REFERENCES[dbo].[Produk_" + nama_perusahaan + "]" +
                    "([id_produk])" +
                    
                    " ALTER TABLE[dbo].[Harga_" + nama_perusahaan + "]" +
                    " CHECK CONSTRAINT[FK_Harga_" + nama_perusahaan + "_Produk_" + nama_perusahaan + "]" +
                    
                    " USE[MultiTenancy_Sablon]" + 
                    
                    " SET ANSI_NULLS ON" +
                    
                    " SET QUOTED_IDENTIFIER ON" +
                    
                    " CREATE TABLE[dbo].[Ukuran_" + nama_perusahaan + "]" +
                    "(" +
                    "[id_ukuran][int] IDENTITY(1,1) NOT NULL," +
                    "[ukuran] [char](5) NULL," +
                    "CONSTRAINT[PK_Ukuran_" + nama_perusahaan + "] PRIMARY KEY CLUSTERED" +
                    "(" +
                    "[id_ukuran] ASC" +
                    ")WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]" +
                    ") ON[PRIMARY]" +
                    
                    " USE[MultiTenancy_Sablon]" +
                    
                    " SET ANSI_NULLS ON" +
                    
                    " SET QUOTED_IDENTIFIER ON" +
                    
                    " CREATE TABLE[dbo].[Order_" + nama_perusahaan + "]" +
                    "(" +
                    "[no_order][int] IDENTITY(1,1) NOT NULL," +
                    "[id_customer] [int] NULL," +
                    "[tgl_order] [date] NULL," +
                    "[total_harga] [int] NULL," +
                    "CONSTRAINT[PK_Order_" + nama_perusahaan + "] PRIMARY KEY CLUSTERED" +
                    "(" +
                    "[no_order] ASC" +
                    ")WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]" +
                    ") ON[PRIMARY]" +
                    
                    " ALTER TABLE[dbo].[Order_" + nama_perusahaan + "] WITH CHECK ADD CONSTRAINT[FK_Order_" + nama_perusahaan + "_Customer_" + nama_perusahaan + "] FOREIGN KEY([id_customer])" +
                    " REFERENCES[dbo].[Customer_" + nama_perusahaan + "]" +
                    "([id_cutomer])" +
                    
                    " ALTER TABLE[dbo].[Order_" + nama_perusahaan + "]" +
                    " CHECK CONSTRAINT[FK_Order_" + nama_perusahaan + "_Customer_" + nama_perusahaan + "]" +
                    
                    " USE[MultiTenancy_Sablon]" +
                    
                    " SET ANSI_NULLS ON" +
                    
                    " SET QUOTED_IDENTIFIER ON" +
                    
                    " CREATE TABLE[dbo].[DetailOrder_" + nama_perusahaan + "]" +
                    "(" +
                    "[no_detail][int] IDENTITY(1,1) NOT NULL," +
                    "[no_order] [int] NULL," +
                    "[id_produk] [int] NULL," +
                    "[id_bahan] [int] NULL," +
                    "[id_jns_sablon] [int] NULL," +
                    "[desain] [image] NULL," +
                    "[jumlah] [int] NULL," +
                    "CONSTRAINT[PK_DetailOrder_" + nama_perusahaan + "] PRIMARY KEY CLUSTERED" +
                    "(" +
                    "[no_detail] ASC" +
                    ")WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]" +
                    ") ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]" +
                    
                    " ALTER TABLE[dbo].[DetailOrder_" + nama_perusahaan + "] WITH CHECK ADD CONSTRAINT[FK_DetailOrder_" + nama_perusahaan + "_Bahan_" + nama_perusahaan + "] FOREIGN KEY([id_bahan])" +
                    " REFERENCES[dbo].[Bahan_" + nama_perusahaan + "]" +
                    "([id_bahan])" +
                    
                    " ALTER TABLE[dbo].[DetailOrder_" + nama_perusahaan + "]" +
                    " CHECK CONSTRAINT[FK_DetailOrder_" + nama_perusahaan + "_Bahan_" + nama_perusahaan + "]" +
                    
                    " ALTER TABLE[dbo].[DetailOrder_" + nama_perusahaan + "] WITH CHECK ADD CONSTRAINT[FK_DetailOrder_" + nama_perusahaan + "_JenisSablon_" + nama_perusahaan + "] FOREIGN KEY([id_jns_sablon])" +
                    " REFERENCES[dbo].[JenisSablon_" + nama_perusahaan + "]" +
                    "([id_jns_sablon])" +
                    
                    " ALTER TABLE[dbo].[DetailOrder_" + nama_perusahaan + "]" +
                    " CHECK CONSTRAINT[FK_DetailOrder_" + nama_perusahaan + "_JenisSablon_" + nama_perusahaan + "]" +
                    
                    " ALTER TABLE[dbo].[DetailOrder_" + nama_perusahaan + "] WITH CHECK ADD CONSTRAINT[FK_DetailOrder_" + nama_perusahaan + "_Order_" + nama_perusahaan + "] FOREIGN KEY([no_order])" +
                    " REFERENCES[dbo].[Order_" + nama_perusahaan + "]" +
                    "([no_order])" +
                    
                    " ALTER TABLE[dbo].[DetailOrder_" + nama_perusahaan + "]" +
                    " CHECK CONSTRAINT[FK_DetailOrder_" + nama_perusahaan + "_Order_" + nama_perusahaan + "]" +
                    
                    " ALTER TABLE[dbo].[DetailOrder_" + nama_perusahaan + "] WITH CHECK ADD CONSTRAINT[FK_DetailOrder_" + nama_perusahaan + "_Produk_" + nama_perusahaan + "] FOREIGN KEY([id_produk])" +
                    " REFERENCES[dbo].[Produk_" + nama_perusahaan + "]" +
                    "([id_produk])" +
                    
                    " ALTER TABLE[dbo].[DetailOrder_" + nama_perusahaan + "]" +
                    " CHECK CONSTRAINT[FK_DetailOrder_" + nama_perusahaan + "_Produk_" + nama_perusahaan + "]" +
                    
                    " USE[MultiTenancy_Sablon]" +
                    
                    " SET ANSI_NULLS ON" +
                    
                    " SET QUOTED_IDENTIFIER ON" +
                    
                    " CREATE TABLE[dbo].[UkuranOrder_" + nama_perusahaan + "]" +
                    "([id_ukuran_order] [int] IDENTITY(1,1) NOT NULL," +
                    "[id_ukuran][int] NULL," +
                    "[no_detail][int] NULL," +
                    "[jumlah][int] NULL," +
                    "CONSTRAINT[PK_UkuranOrder_c] PRIMARY KEY CLUSTERED" +
                    "(" +
                    "[id_ukuran_order] ASC" +
                    ")WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]" +
                    
                    ") ON[PRIMARY]" +
                    
                    " ALTER TABLE[dbo].[UkuranOrder_" + nama_perusahaan + "] WITH CHECK ADD CONSTRAINT[FK_UkuranOrder_" + nama_perusahaan + "_DetailOrder_" + nama_perusahaan + "] FOREIGN KEY([no_detail])" +
                    " REFERENCES[dbo].[DetailOrder_" + nama_perusahaan + "]" +
                    "([no_detail])" +
                    
                    " ALTER TABLE[dbo].[UkuranOrder_" + nama_perusahaan + "]" +
                    " CHECK CONSTRAINT[FK_UkuranOrder_" + nama_perusahaan + "_DetailOrder_" + nama_perusahaan + "]" +
                    
                    " ALTER TABLE[dbo].[UkuranOrder_" + nama_perusahaan + "] WITH CHECK ADD CONSTRAINT[FK_UkuranOrder_" + nama_perusahaan + "_Ukuran_" + nama_perusahaan + "] FOREIGN KEY([id_ukuran])" +
                    " REFERENCES[dbo].[Ukuran_" + nama_perusahaan + "]" +
                    "([id_ukuran])" +
                    
                    " ALTER TABLE[dbo].[UkuranOrder_" + nama_perusahaan + "]" +
                    " CHECK CONSTRAINT[FK_UkuranOrder_" + nama_perusahaan + "_Ukuran_" + nama_perusahaan + "]" +
                    "USE [MultiTenancy_Sablon]" +
                    "SET ANSI_NULLS ON" +
                    "SET QUOTED_IDENTIFIER ON" +
                    "CREATE VIEW[dbo].[ViewHarga_" + nama_perusahaan + "]" +
                    "AS" +
                    "SELECT        dbo.Bahan_" + nama_perusahaan + "." + "nama_bahan, dbo.JenisSablon_" + nama_perusahaan + "." + "nama_sablon, dbo.Produk_" + nama_perusahaan + "." + "nama_produk, dbo.Harga_" + nama_perusahaan + "." + "harga, dbo.Bahan_" + nama_perusahaan + "." + "id_bahan, dbo.JenisSablon_" + nama_perusahaan + "." + "id_jns_sablon, dbo.Produk_" + nama_perusahaan + "." + "id_produk" +
                    "FROM            dbo.Bahan_" + nama_perusahaan + " LEFT OUTER JOIN" +
                    "dbo.Produk_" + nama_perusahaan + " ON dbo.Bahan_" + nama_perusahaan + "." + "id_produk = dbo.Produk_" + nama_perusahaan + "." + "id_produk LEFT OUTER JOIN" +
                    "dbo.JenisSablon_" + nama_perusahaan + " ON dbo.Produk_" + nama_perusahaan + "." + "id_produk = dbo.JenisSablon_" + nama_perusahaan + "." + "id_produk LEFT OUTER JOIN" +
                    "dbo.Harga_" + nama_perusahaan + " ON dbo.Bahan_" + nama_perusahaan + "." + "id_bahan = dbo.Harga_" + nama_perusahaan + "." + "id_bahan AND dbo.Produk_" + nama_perusahaan + "." + "id_produk = dbo.Harga_" + nama_perusahaan + "." + "id_produk AND dbo.JenisSablon_" + nama_perusahaan + "." + "id_jns_sablon = dbo.Harga_" + nama_perusahaan + "." + "id_jns_sablon" +
                    "USE [MultiTenancy_Sablon]" +
                    "SET ANSI_NULLS ON" +
                    "SET QUOTED_IDENTIFIER ON" +
                    "CREATE TRIGGER[dbo].[ShowInsert_" + nama_perusahaan + "]" + "on[MultiTenancy_Sablon].[dbo].[ViewHarga_" + nama_perusahaan + "]" +
                    "INSTEAD OF UPDATE" +
                    "AS" +
                    "BEGIN" +
                    "declare @id_produk int" +
                    "declare @id_bahan int" +
                    "declare @id_jns_sablon int" +
                    "declare @harga int" +
                    "select @id_produk = p.id_produk" +
                    "from Produk_" + nama_perusahaan + " p" +
                    "join inserted" +
                    "on inserted.nama_produk = p.nama_produk" +
                    "select @id_bahan = b.id_bahan" +
                    "from Bahan_" + nama_perusahaan + " b" +
                    "join inserted" +
                    "on inserted.nama_bahan = b.nama_bahan" +
                    "select @id_jns_sablon = j.id_jns_sablon" +
                    "from JenisSablon_" + nama_perusahaan + " j" +
                    "join inserted" +
                    "on inserted.nama_sablon = j.nama_sablon" +
                    "select @harga = harga from inserted where id_produk = @id_produk and id_bahan = @id_bahan and id_jns_sablon = @id_jns_sablon" +
                    "insert into Harga_" + nama_perusahaan + "(id_produk, id_bahan, id_jns_sablon, harga) values(@id_produk, @id_bahan, @id_jns_sablon, @harga)" +
                    "END"                   
                    ;

                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    sqlcom.ExecuteNonQuery();
                    TempData["Pesan"] = Helpers.Message.GetPesan("Berhasil !",
                                          "success", "Tabel untuk perusahaan " + nama_perusahaan+ " berhasil ditambah");
                }
                catch (Exception ex)
                {
                    TempData["Pesan"] = Helpers.Message.GetPesan("Error !",
                                          "danger", ex.Message);
                }
                
                conn.Close();
            }
            return RedirectToAction("Index");
        }

        
    }
}
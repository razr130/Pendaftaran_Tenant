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
                    return RedirectToAction("CreateUI", "Penyewa");
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
                }
                catch (Exception ex)
                {
                    TempData["Pesan"] = Helpers.Message.GetPesan("Error !",
                                          "danger", ex.Message);
                }
            }
            return View();
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
                    "[foto_produk] [image] NULL," +
                    "CONSTRAINT[PK_Produk_" + nama_perusahaan + "] PRIMARY KEY CLUSTERED" +
                    "(" +
                    "[id_produk] ASC" +
                    ")WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]" +
                    ") ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]" +
                    
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
                    "(" +
                    "[id_produk][int] NULL," +
                    "[id_bahan][int] NULL," +
                    "[id_jns_sablon][int] NULL," +
                    "[harga][int] NULL" +
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
                    "(" +
                    "[id_ukuran][int] NULL," +
                    "[no_detail][int] NULL," +
                    "[jumlah][int] NULL" +
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
                    " CHECK CONSTRAINT[FK_UkuranOrder_" + nama_perusahaan + "_Ukuran_" + nama_perusahaan + "]"
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
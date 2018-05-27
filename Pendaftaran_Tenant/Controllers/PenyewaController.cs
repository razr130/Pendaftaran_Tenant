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
using System.Net.Mail;
using System.Net;
using System.Text;

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
        public ActionResult IndexCarausel()
        {
            using (PenyewaDAL svpenyewa = new PenyewaDAL())
            {
                int id = svpenyewa.getIdUI(Convert.ToInt32(Session["id_penyewa"]));
                var result = svpenyewa.GetDataCarausel(id).ToList();
                return View(result);
            }
        }

        public ActionResult DeleteCarausel(int id)
        {
            string nama_perusahaan = Session["nama_perusahaan"].ToString();
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "USE [MultiTenancy_Sablon]" +
                    "DELETE FROM [dbo].[Data_carausel]" +
                    " WHERE [id_carausel]=" + id;

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
            return RedirectToAction("IndexCarausel");
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
                        return RedirectToAction("Index2", "Home");
                    }
                    else
                    {
                        var results2 = db.Data_UI.SingleOrDefault(m => m.id_penyewa == idpenyewa);
                        if (results2 != null)
                        {
                            return RedirectToAction("Index2", "Home");
                        }
                        else
                        {
                            return RedirectToAction("CreateUI", "Penyewa");
                        }

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
                    Session["email"] = penyewa.email;
                    Session["namaperusahaanemail"] = penyewa.nama_perusahaan;
                    Session["alamatemail"] = penyewa.alamat;
                    Session["notelpemail"] = penyewa.no_telp;
                    //TempData["Pesan"] = Helpers.Message.GetPesan("Sukses !",
                    //  "success", "Data Barang " + barang.nama_barang + " berhasil ditambah");
                }
                catch (Exception ex)
                {
                    //TempData["Pesan"] = Helpers.Message.GetPesan("Error !",
                    //                      "danger", ex.Message);
                }
            }
            return RedirectToAction("KonfirmasiDaftar", "Penyewa");
        }

        public JsonResult SendMailToUser()
        {

            bool result = false;

            result = SendEmail(Session["email"].ToString(), "Konfirmasi pendaftaran dan informasi pembayaran",
                "<p>Pendaftaran anda telah berhasil,<br />" +
                "Nama Perusahaan : " + Session["namaperusahaanemail"].ToString() + "<br />" +
                "Alamat : " + Session["alamatemail"].ToString() + "<br />" +
                "No Telp : " + Session["notelpemail"].ToString() + "<br />" +
                "<br/>" +
                "Biaya pendaftaran sebesar : Rp 99999999 <br />" +
                "Biaya tersebut sudah termasuk biaya pengadaan website, serta maintenance bulanan <br />" +
                "<br />" +
                "Biaya pendaftaran dapat dibayarkan melalui rekening : <br />" +
                "BNI : 344449404940 <br />" +
                "BCA : 28102819111829 <br />" +
                "Mandiri : 73289372321 <br />" +
                "<br />" +
                "Pembayaran dilakukan maksimal 1x24 jam, bila pembayaran tidak dilakukan dalam jangka waktu tersebut, <br />" +
                "maka pendaftaran dinyatakan hangus.</p>");

            return Json(result, JsonRequestBehavior.AllowGet);

        }

        public bool SendEmail(string toEmail, string subject, string emailBody)
        {

            try
            {
                string senderEmail = System.Configuration.ConfigurationManager.AppSettings["SenderEmail"].ToString();
                string senderPassword = System.Configuration.ConfigurationManager.AppSettings["SenderPassword"].ToString();

                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                client.EnableSsl = true;
                client.Timeout = 100000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(senderEmail, senderPassword);

                MailMessage mailMessage = new MailMessage(senderEmail, toEmail, subject, emailBody);
                mailMessage.IsBodyHtml = true;
                mailMessage.BodyEncoding = UTF8Encoding.UTF8;
                client.Send(mailMessage);

                return true;

            }
            catch (Exception ex)
            {
                return false;

            }

        }



        public ActionResult KonfirmasiDaftar()
        {
            SendMailToUser();
            using (PenyewaDAL tenan = new PenyewaDAL())
            {
                int id = tenan.getId(Session["email"].ToString());
                var result = tenan.GetDataBaru(id).ToList();
                return View(result);
            }

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
                    //TempData["Pesan"] = Helpers.Message.GetPesan("Sukses !",
                    //"success", "Data UI berhasil ditambah");
                    //Session["id_ui"] = svBrg.getIdUI(Convert.ToInt32(Session["id_penyewa"]));
                }
                catch (Exception ex)
                {
                    //TempData["Pesan"] = Helpers.Message.GetPesan("Error !",
                    //                      "danger", ex.Message);
                }
            }
            return RedirectToAction("CreateCarausel");
        }
        public int GetCount()
        {
            int idui;
            using (PenyewaDAL svui = new PenyewaDAL())
            {
                idui = svui.getIdUI(Convert.ToInt32(Session["id_penyewa"]));
            }
            string stmt = "SELECT COUNT(gambar) FROM Data_carausel WHERE id_ui = " + idui + ";";
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
            TempData["Pesan2"] = Helpers.Message.GetPesan("Info data carausel",
                          "warning", "Anda sudah memasukkan " + GetCount().ToString() + " gambar carausel dari maksimal 4 gambar");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCarausel(Data_carausel datacarausel, HttpPostedFileBase uploadimage)
        {
            if (GetCount() == 4)
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

                //datacarausel.id_ui = Convert.ToInt32(Session["id_ui"]);
                using (PenyewaDAL svBrg = new PenyewaDAL())
                {
                    try
                    {
                        datacarausel.id_ui = svBrg.getIdUI(Convert.ToInt32(Session["id_penyewa"]));
                        svBrg.AddCarausel(datacarausel);
                        //TempData["Pesan"] = Helpers.Message.GetPesan("Sukses !",
                        //  "success", "Data carausel berhasil ditambah");
                        TempData["Pesan2"] = Helpers.Message.GetPesan("Info data carausel",
                          "warning", "Anda sudah memasukkan " + GetCount().ToString() + " gambar carausel dari maksimal 4 gambar");
                    }
                    catch (Exception ex)
                    {
                        //TempData["Pesan"] = Helpers.Message.GetPesan("Error !",
                        //                      "danger", ex.Message);
                    }
                }
                return RedirectToAction("IndexCarausel", "Penyewa");
            }

        }
        public ActionResult IndexKaryawan()
        {


            string nama_perusahaan = Session["nama_perusahaan"].ToString();


            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            List<Karyawan> result = new List<Karyawan>();

            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "SELECT [id_karyawan]" +
                    ",[nama_karyawan]" +
                    ",[email_karyawan]" +
                    ",[password]" +
                    ",[tempat_lahir]" +
                    ",[tgl_lahir]" +
                    ",[no_telp]" +
                    ",[alamat]" +
                    ",[jns_kelamin]" +
                    " FROM[MultiTenancy_Sablon].[dbo].[Karyawan_" + nama_perusahaan + "]";



                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Karyawan item = new Karyawan()
                            {
                                id_karyawan = (int)reader["id_karyawan"],
                                nama_karyawan = reader["nama_karyawan"].ToString(),
                                email_karyawan = reader["email_karyawan"].ToString(),
                                password = reader["password"].ToString(),
                                tempat_lahir = reader["tempat_lahir"].ToString(),
                                tgl_lahir = reader["tgl_lahir"].ToString(),
                                no_telp = reader["no_telp"].ToString(),
                                alamat = reader["alamat"].ToString(),
                                jns_kelamin = reader["jns_kelamin"].ToString()

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
        public ActionResult IndexKaryawanBig()
        {




            string nama_perusahaan = Session["nama_perusahaan"].ToString();


            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            List<Karyawan> result = new List<Karyawan>();

            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "SELECT [id_karyawan]" +
                    ",[nama_karyawan]" +
                    ",[email_karyawan]" +
                    ",[password]" +
                    ",[tempat_lahir]" +
                    ",[tgl_lahir]" +
                    ",[no_telp]" +
                    ",[alamat]" +
                    ",[jns_kelamin]" +
                    " FROM[MultiTenancy_Sablon].[dbo].[Karyawan_" + nama_perusahaan + "]";



                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Karyawan item = new Karyawan()
                            {
                                id_karyawan = (int)reader["id_karyawan"],
                                nama_karyawan = reader["nama_karyawan"].ToString(),
                                email_karyawan = reader["email_karyawan"].ToString(),
                                password = reader["password"].ToString(),
                                tempat_lahir = reader["tempat_lahir"].ToString(),
                                tgl_lahir = reader["tgl_lahir"].ToString(),
                                no_telp = reader["no_telp"].ToString(),
                                alamat = reader["alamat"].ToString(),
                                jns_kelamin = reader["jns_kelamin"].ToString()

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

        public ActionResult CreateKaryawan()
        {


            return View();
        }
        [HttpPost]
        public ActionResult CreateKaryawan(string nama_karyawan, string email_karyawan, string password, string tempat_lahir, string tanggal, string bulan, string tahun, string no_telp, string alamat, string jns_kelamin)
        {

            string nama_perusahaan;
            int id = Convert.ToInt32(Session["id_penyewa"]);

            using (PenyewaDAL sewa = new PenyewaDAL())
            {

                nama_perusahaan = sewa.getNamaPerusahaan(id).ToString();
            }
            string tgllahir = tanggal + "-" + bulan + "-" + tahun;
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "USE [MultiTenancy_Sablon]" +
                    "INSERT INTO[dbo].[Karyawan_" + nama_perusahaan + "]" +
                    " ([nama_karyawan]" +
                    ",[email_karyawan]" +
                    ",[password]" +
                    ",[tempat_lahir]" +
                    ",[tgl_lahir]" +
                    ",[no_telp]" +
                    ",[alamat]" +
                    ",[jns_kelamin])" +

                    " VALUES" +
                    "('" + nama_karyawan + "' ,'" + email_karyawan + "' ,'" + password + "' ,'" + tempat_lahir + "' ,'" + tgllahir + "' ,'" + no_telp + "' ,'" + alamat + "' ,'" + jns_kelamin + "')";

                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    sqlcom.ExecuteNonQuery();
                    //TempData["Pesan"] = Helpers.Message.GetPesan("Berhasil !",
                    //                      "success", "data karyawan ditambah, data karyawan dapat direview ulang melalui Beranda setelah menyelesaikan proses pendaftaran");
                }
                catch (Exception ex)
                {
                    //TempData["Pesan"] = Helpers.Message.GetPesan("Error !",
                    //                      "danger", ex.Message);
                }

                conn.Close();
            }



            return RedirectToAction("IndexKaryawan", "Penyewa");
        }

        public ActionResult CreateKaryawan2()
        {


            return View();
        }
        [HttpPost]
        public ActionResult CreateKaryawan2(string nama_karyawan, string email_karyawan, string password, string tempat_lahir, string tanggal, string bulan, string tahun, string no_telp, string alamat, string jns_kelamin)
        {

            string nama_perusahaan;
            int id = Convert.ToInt32(Session["id_penyewa"]);
            string tgllahir = tanggal + "-" + bulan + "-" + tahun;
            using (PenyewaDAL sewa = new PenyewaDAL())
            {

                nama_perusahaan = sewa.getNamaPerusahaan(id).ToString();
            }
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "USE [MultiTenancy_Sablon]" +
                    "INSERT INTO[dbo].[Karyawan_" + nama_perusahaan + "]" +
                    " ([nama_karyawan]" +
                    ",[email_karyawan]" +
                    ",[password]" +
                    ",[tempat_lahir]" +
                    ",[tgl_lahir]" +
                    ",[no_telp]" +
                    ",[alamat]" +
                    ",[jns_kelamin])" +

                    " VALUES" +
                    "('" + nama_karyawan + "' ,'" + email_karyawan + "' ,'" + password + "' ,'" + tempat_lahir + "' ,'" + tgllahir + "' ,'" + no_telp + "' ,'" + alamat + "' ,'" + jns_kelamin + "')";

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



            return RedirectToAction("IndexKaryawan", "Penyewa");
        }
        public ActionResult EditKaryawan(int id)
        {

            string nama_perusahaan = Session["nama_perusahaan"].ToString();
            Session["id_karyawan"] = id.ToString();


            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            Karyawan result = new Karyawan();

            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "SELECT [id_karyawan]" +
                    ",[nama_karyawan]" +
                    ",[email_karyawan]" +
                    ",[password]" +
                    ",[tempat_lahir]" +
                    ",[tgl_lahir]" +
                    ",[no_telp]" +
                    ",[alamat]" +
                    ",[jns_kelamin]" +
                    " FROM[MultiTenancy_Sablon].[dbo].[Karyawan_" + nama_perusahaan + "]" +
                    " WHERE [id_karyawan]=" + id.ToString();

                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {

                        if (reader.Read())
                        {
                            result.id_karyawan = (int)reader["id_karyawan"];
                            result.nama_karyawan = reader["nama_karyawan"].ToString();
                            result.email_karyawan = reader["email_karyawan"].ToString();
                            result.password = reader["password"].ToString();
                            result.tempat_lahir = reader["tempat_lahir"].ToString();
                            result.tgl_lahir = reader["tgl_lahir"].ToString();
                            result.tanggal = reader["tgl_lahir"].ToString().Split('-')[0];
                            result.bulan = reader["tgl_lahir"].ToString().Split('-')[1];
                            result.tahun = reader["tgl_lahir"].ToString().Split('-')[2];
                            result.no_telp = reader["no_telp"].ToString();
                            result.alamat = reader["alamat"].ToString();
                            result.jns_kelamin = reader["jns_kelamin"].ToString();
                        };
                    }

                }
                catch (Exception)
                {

                }

                conn.Close();
            }
            return View(result);


        }
        [HttpPost]
        public ActionResult EditKaryawan(string nama_karyawan, string email_karyawan, string password, string tempat_lahir, string tanggal, string bulan, string tahun, string no_telp, string alamat, string jns_kelamin)
        {
            var tgl_lahir = tanggal + "-" + bulan + "-" + tahun;
            string nama_perusahaan = Session["nama_perusahaan"].ToString();
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "USE [MultiTenancy_Sablon]" +
                    "UPDATE[dbo].[Karyawan_" + nama_perusahaan + "]" +
                    "SET[nama_karyawan] = '" + nama_karyawan + "'" +
                    ",[email_karyawan] = '" + email_karyawan + "'" +
                    ",[password] = '" + password + "'" +
                    ",[tempat_lahir] = '" + tempat_lahir + "'" +
                    ",[tgl_lahir] = '" + tgl_lahir + "'" +
                    ",[no_telp] = '" + no_telp + "'" +
                    ",[alamat] = '" + alamat + "'" +
                    ",[jns_kelamin] = '" + jns_kelamin + "'" +
                    " WHERE [id_karyawan]=" + Session["id_karyawan"].ToString();


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
            return RedirectToAction("IndexKaryawan");
        }

        public ActionResult EditKaryawan2(int id)
        {

            string nama_perusahaan = Session["nama_perusahaan"].ToString();
            Session["id_karyawan"] = id.ToString();


            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            Karyawan result = new Karyawan();

            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "SELECT [id_karyawan]" +
                    ",[nama_karyawan]" +
                    ",[email_karyawan]" +
                    ",[password]" +
                    ",[tempat_lahir]" +
                    ",[tgl_lahir]" +
                    ",[no_telp]" +
                    ",[alamat]" +
                    ",[jns_kelamin]" +
                    " FROM[MultiTenancy_Sablon].[dbo].[Karyawan_" + nama_perusahaan + "]" +
                    " WHERE [id_karyawan]=" + id.ToString();

                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {

                        if (reader.Read())
                        {
                            result.id_karyawan = (int)reader["id_karyawan"];
                            result.nama_karyawan = reader["nama_karyawan"].ToString();
                            result.email_karyawan = reader["email_karyawan"].ToString();
                            result.password = reader["password"].ToString();
                            result.tempat_lahir = reader["tempat_lahir"].ToString();
                            result.tgl_lahir = reader["tgl_lahir"].ToString();
                            result.tanggal = reader["tgl_lahir"].ToString().Split('-')[0];
                            result.bulan = reader["tgl_lahir"].ToString().Split('-')[1];
                            result.tahun = reader["tgl_lahir"].ToString().Split('-')[2];
                            result.no_telp = reader["no_telp"].ToString();
                            result.alamat = reader["alamat"].ToString();
                            result.jns_kelamin = reader["jns_kelamin"].ToString();
                        };
                    }

                }
                catch (Exception)
                {

                }

                conn.Close();
            }
            return View(result);


        }
        [HttpPost]
        public ActionResult EditKaryawan2(string nama_karyawan, string email_karyawan, string password, string tempat_lahir, string tanggal, string bulan, string tahun, string no_telp, string alamat, string jns_kelamin)
        {
            var tgl_lahir = tanggal + "-" + bulan + "-" + tahun;
            string nama_perusahaan = Session["nama_perusahaan"].ToString();
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "USE [MultiTenancy_Sablon]" +
                    "UPDATE[dbo].[Karyawan_" + nama_perusahaan + "]" +
                    "SET[nama_karyawan] = '" + nama_karyawan + "'" +
                    ",[email_karyawan] = '" + email_karyawan + "'" +
                    ",[password] = '" + password + "'" +
                    ",[tempat_lahir] = '" + tempat_lahir + "'" +
                    ",[tgl_lahir] = '" + tgl_lahir + "'" +
                    ",[no_telp] = '" + no_telp + "'" +
                    ",[alamat] = '" + alamat + "'" +
                    ",[jns_kelamin] = '" + jns_kelamin + "'" +
                    " WHERE [id_karyawan]=" + Session["id_karyawan"].ToString();


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
            return RedirectToAction("IndexKaryawanBig");
        }


        public ActionResult DeleteKaryawan(int id)
        {
            string nama_perusahaan = Session["nama_perusahaan"].ToString();
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "USE [MultiTenancy_Sablon]" +
                    "DELETE FROM [dbo].[Karyawan_" + nama_perusahaan + "]" +
                    " WHERE [id_karyawan]=" + id;

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
            return RedirectToAction("IndexProdukBig");
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
                    "INSERT INTO[dbo].[Produk_" + nama_perusahaan + "]" +
                    "([nama_produk]" +
                    ",[deskripsi]" +
                    ",[foto_produk])" +
                    "VALUES" +
                    "('" + namaproduk + "' ,'" + deskripsi + "' ,'" + fileName + "')";

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
            return RedirectToAction("IndexProduk", "Penyewa");
        }
        public ActionResult CreateProduk2()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateProduk2(string namaproduk, string deskripsi, string kategori, HttpPostedFileBase fotoproduk,
            int? co30s, int? co24s, int? co20s, int? ca30s,
int? ca24s,
int? polyester,
int? tc,
int? viscose,
int? cvc,
int? hyget,
int? polo,
int? rubber,
int? rubberekstra,
int? rubbermatsui,
int? pigmen,
int? plastisol,
int? gitd,
int? flocking,
int? glitter,
int? foil,
int? dtg,
int? polyfex,
int? d3d,
int? da4,
int? da3,
int? blok,
int? depanbelakang,
int? lengan,
int? xxl,
int? lebihxxl,
int? lenganpanjang,
int? raglan,
int? hoodie,
int? d300,
int? d400,
int? furing,
int? babyripstop,
int? kanvas,
int? blacu,
int? rubbertas,
int? jahitsewarna,
int? jahitbanyakwarna,
int? kanvassepatu,
int? kulit,
int? cat,
int? sabloncap,
int? datar,
int? gelas,
int? custom,
int? catkeramik,
int? cap,
int? fleececotton30s,
int? fleececotton16s,
int? fleececvc30s,
int? babyterry,
int? rubberjaket,
int? bordirjaket,
int? dischargejaket,
int? da4jaket,
int? da3jaket,
int? blokjaket,
int? depanbelakangjaket,
int? lenganjaket,
int? xxljaket,
int? lebihxxljaket,
int? drill,
int? hisovi,
int? nagata,
int? bordirkorsa,
int? dada,
int? belakang,
int? lengankorsa,
int? xxlkorsa,
int? lebihxxlkorsa)
        {
            int idproduk = 0;
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
                    " INSERT INTO[dbo].[Produk_" + nama_perusahaan + "]" +
                    "([nama_produk]" +
                    ",[deskripsi]" +
                    ",[kategori]" +
                    ",[foto_produk])" +
                    "VALUES" +
                    "('" + namaproduk + "','" + deskripsi + "','" + kategori + "','" + fileName + "')";

                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    sqlcom.ExecuteNonQuery();

                    var res = "";
                    sqlcom.CommandText = "SELECT [id_produk]" +
                    " FROM[MultiTenancy_Sablon].[dbo].[Produk_" + nama_perusahaan + "] WHERE [nama_produk]='" + namaproduk + "'";

                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            res = string.Format("{0}", reader["id_produk"].ToString());
                            idproduk = Convert.ToInt32(res);
                            Session["id_produk_gabung"] = idproduk.ToString();
                        }
                    }

                    //bahan

                    if (co30s != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
                            "([id_produk]" +
                            ",[nama_bahan]" +
                            ",[harga])" +
                            " VALUES" +
                            " (" + idproduk + ",'Cotton Combed 30s'," + co30s + ")";

                        sqlcom.ExecuteNonQuery();

                    }
                    if (co24s != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
                            "([id_produk]" +
                            ",[nama_bahan]" +
                            ",[harga])" +
                            " VALUES" +
                            " (" + idproduk + ",'Cotton Combed 24s'," + co24s + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (co20s != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
                            "([id_produk]" +
                            ",[nama_bahan]" +
                            ",[harga])" +
                            " VALUES" +
                            " (" + idproduk + ",'Cotton Combed 20s'," + co20s + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (ca30s != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
                            "([id_produk]" +
                            ",[nama_bahan]" +
                            ",[harga])" +
                            " VALUES" +
                            " (" + idproduk + ",'Cardet 30s'," + ca30s + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (ca24s != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_bahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Cardet 24s'," + ca24s + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (tc != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_bahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Tetteron Cotton'," + tc + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (viscose != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_bahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Viscose'," + viscose + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (cvc != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_bahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Cotton Viscose'," + cvc + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (hyget != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_bahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Hyget'," + hyget + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (d300 != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_bahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'D300'," + d300 + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (d400 != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_bahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'D400'," + d400 + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (furing != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_bahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Furing'," + furing + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (babyripstop != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_bahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Cardet 30s'," + ca30s + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (kanvas != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_bahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Kanvas'," + kanvas + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (blacu != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_bahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Blacu'," + blacu + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (kanvassepatu != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_bahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Kanvas'," + kanvassepatu + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (kulit != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_bahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Kulit'," + kulit + ")";

                        sqlcom.ExecuteNonQuery();

                    }
                    if (datar != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_bahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Datar'," + datar + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (gelas != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_bahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Gelas'," + gelas + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (custom != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_bahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Custom'," + custom + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (polo != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_bahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Polo'," + polo + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (fleececotton30s != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_bahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Fleece Cotton 30s'," + fleececotton30s + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (fleececotton16s != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_bahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Fleece Cotton 16s'," + fleececotton16s + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (fleececvc30s != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_bahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Fleece CVC 30s'," + fleececvc30s + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (babyterry != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_bahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Babyterry'," + babyterry + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (drill != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_bahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Drill'," + drill + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (hisovi != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_bahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Hisovi'," + hisovi + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (nagata != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_bahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Nagata'," + nagata + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    //jenissablon
                    if (rubber != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[JenisSablon_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_sablon]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Rubber'," + rubber + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (rubberekstra != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[JenisSablon_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_sablon]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Rubber Ekstra'," + rubberekstra + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (rubbermatsui != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[JenisSablon_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_sablon]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Rubber Matsui'," + rubbermatsui + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (pigmen != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[JenisSablon_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_sablon]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Pigmen'," + pigmen + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (plastisol != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[JenisSablon_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_sablon]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Plastisol'," + plastisol + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (gitd != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[JenisSablon_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_sablon]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Glow in the Dark'," + gitd + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (flocking != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[JenisSablon_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_sablon]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Flocking'," + flocking + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (glitter != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[JenisSablon_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_sablon]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Glitter'," + glitter + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (foil != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[JenisSablon_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_sablon]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Foil'," + foil + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (dtg != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[JenisSablon_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_sablon]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'DTG'," + dtg + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (polyfex != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[JenisSablon_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_sablon]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Polyfex'," + polyfex + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (d3d != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[JenisSablon_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_sablon]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'3D'," + d3d + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (jahitsewarna != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[JenisSablon_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_sablon]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Jahit Satu Warna'," + jahitsewarna + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (jahitbanyakwarna != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[JenisSablon_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_sablon]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Jahit Banyak Warna'," + jahitbanyakwarna + ")";

                        sqlcom.ExecuteNonQuery();
                    }

                    if (cat != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[JenisSablon_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_sablon]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Cat'," + cat + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (sabloncap != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[JenisSablon_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_sablon]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Sablon Cap'," + sabloncap + ")";

                        sqlcom.ExecuteNonQuery();
                    }

                    if (catkeramik != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[JenisSablon_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_sablon]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Cat Keramik'," + catkeramik + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (cap != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[JenisSablon_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_sablon]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Cap'," + cap + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (rubbertas != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[JenisSablon_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_sablon]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Rubber'," + rubbertas + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (rubberjaket != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[JenisSablon_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_sablon]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Rubber'," + rubberjaket + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (bordirjaket != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[JenisSablon_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_sablon]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Bordir'," + bordirjaket + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (dischargejaket != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[JenisSablon_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_sablon]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Discharge'," + dischargejaket + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (bordirkorsa != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[JenisSablon_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_sablon]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Bordir'," + bordirkorsa + ")";

                        sqlcom.ExecuteNonQuery();
                    }


                    //tambahan
                    if (da4 != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[TabelTambahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_tambahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'A4'," + da4 + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (da3 != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[TabelTambahan_" + nama_perusahaan + "]" +
                          "([id_produk]" +
                          ",[nama_tambahan]" +
                          ",[harga])" +
                          " VALUES" +
                          " (" + idproduk + ",'A3'," + da3 + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (blok != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[TabelTambahan_" + nama_perusahaan + "]" +
                          "([id_produk]" +
                          ",[nama_tambahan]" +
                          ",[harga])" +
                          " VALUES" +
                          " (" + idproduk + ",'Blok'," + blok + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (depanbelakang != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[TabelTambahan_" + nama_perusahaan + "]" +
                          "([id_produk]" +
                          ",[nama_tambahan]" +
                          ",[harga])" +
                          " VALUES" +
                          " (" + idproduk + ",'Depan Belakang'," + depanbelakang + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (lengan != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[TabelTambahan_" + nama_perusahaan + "]" +
                          "([id_produk]" +
                          ",[nama_tambahan]" +
                          ",[harga])" +
                          " VALUES" +
                          " (" + idproduk + ",'Lengan'," + lengan + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (xxl != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[TabelTambahan_" + nama_perusahaan + "]" +
                          "([id_produk]" +
                          ",[nama_tambahan]" +
                          ",[harga])" +
                          " VALUES" +
                          " (" + idproduk + ",'XXL'," + xxl + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (lebihxxl != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[TabelTambahan_" + nama_perusahaan + "]" +
                          "([id_produk]" +
                          ",[nama_tambahan]" +
                          ",[harga])" +
                          " VALUES" +
                          " (" + idproduk + ",'Lebih dari XXL'," + lebihxxl + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (lenganpanjang != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[TabelTambahan_" + nama_perusahaan + "]" +
                          "([id_produk]" +
                          ",[nama_tambahan]" +
                          ",[harga])" +
                          " VALUES" +
                          " (" + idproduk + ",'Lengan Panjang'," + lenganpanjang + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (raglan != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[TabelTambahan_" + nama_perusahaan + "]" +
                          "([id_produk]" +
                          ",[nama_tambahan]" +
                          ",[harga])" +
                          " VALUES" +
                          " (" + idproduk + ",'Raglan'," + raglan + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (hoodie != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[TabelTambahan_" + nama_perusahaan + "]" +
                          "([id_produk]" +
                          ",[nama_tambahan]" +
                          ",[harga])" +
                          " VALUES" +
                          " (" + idproduk + ",'Hoodie'," + hoodie + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (da4jaket != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[TabelTambahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_tambahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'A4'," + da4jaket + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (da3jaket != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[TabelTambahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_tambahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'A3'," + da3jaket + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (blokjaket != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[TabelTambahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_tambahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Blok'," + blokjaket + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (depanbelakangjaket != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[TabelTambahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_tambahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Depan Belakang'," + depanbelakangjaket + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (lenganjaket != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[TabelTambahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_tambahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Lengan'," + lenganjaket + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (xxljaket != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[TabelTambahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_tambahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'XXL'," + xxljaket + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (lebihxxljaket != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[TabelTambahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_tambahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Lebih dari XXL'," + lebihxxljaket + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (dada != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[TabelTambahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_tambahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Dada'," + dada + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (belakang != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[TabelTambahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_tambahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Belakang'," + belakang + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (lengankorsa != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[TabelTambahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_tambahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Lengan'," + lengankorsa + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (xxlkorsa != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[TabelTambahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_tambahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'XXL'," + xxlkorsa + ")";

                        sqlcom.ExecuteNonQuery();
                    }
                    if (lebihxxlkorsa != null)
                    {
                        sqlcom.CommandText = "INSERT INTO [dbo].[TabelTambahan_" + nama_perusahaan + "]" +
                           "([id_produk]" +
                           ",[nama_tambahan]" +
                           ",[harga])" +
                           " VALUES" +
                           " (" + idproduk + ",'Lebih dari XXL'," + lebihxxlkorsa + ")";

                        sqlcom.ExecuteNonQuery();
                    }

                    TempData["Pesan"] = Helpers.Message.GetPesan("Berhasil tambah produk !",
                                      "success", "Berhasil");

                }
                catch (Exception ex)
                {
                    TempData["Pesan"] = Helpers.Message.GetPesan("Error di tambah produk !",
                                     "danger", ex.Message);
                }
                conn.Close();
            }
            return View();
        }
        //public ActionResult CreateProduk2(string namaproduk, string deskripsi, string kategori, HttpPostedFileBase fotoproduk,
        //     string bahan1, string bahan2, string bahan3, string bahan4, string bahan5, string sablon1, string sablon2, string sablon3, string sablon4, string sablon5,
        //     int? harga1, int? harga2, int? harga3, int? harga4, int? harga5,
        //    string tambahan1, string tambahan2, string tambahan3, string tambahan4, string tambahan5,
        //    int? hargatam1, int? hargatam2, int? hargatam3, int? hargatam4, int? hargatam5)
        //{
        //    int? hargatam1fix, hargatam2fix, hargatam3fix, hargatam4fix, hargatam5fix;
        //    if (hargatam1 != null)
        //    {
        //        hargatam1fix = hargatam1;
        //    }
        //    else
        //    {
        //        hargatam1fix = 0;
        //    }
        //    if (hargatam2 != null)
        //    {
        //        hargatam2fix = hargatam2;
        //    }
        //    else
        //    {
        //        hargatam2fix = 0;
        //    }
        //    if (hargatam3 != null)
        //    {
        //        hargatam3fix = hargatam3;
        //    }
        //    else
        //    {
        //        hargatam3fix = 0;
        //    }
        //    if (hargatam4 != null)
        //    {
        //        hargatam4fix = hargatam4;
        //    }
        //    else
        //    {
        //        hargatam4fix = 0;
        //    }
        //    if (hargatam5 != null)
        //    {
        //        hargatam5fix = hargatam5;
        //    }
        //    else
        //    {
        //        hargatam5fix = 0;
        //    }
        //    int idproduk;
        //    string filePath = "";
        //    string fileName = Guid.NewGuid().ToString() + "_" + fotoproduk.FileName;
        //    if (fotoproduk.ContentLength > 0)
        //    {

        //        filePath = Path.Combine(HttpContext.Server.MapPath("~/Content/Images"), fileName);
        //        fotoproduk.SaveAs(filePath);

        //    }
        //    string nama_perusahaan;
        //    int id = Convert.ToInt32(Session["id_penyewa"]);

        //    using (PenyewaDAL sewa = new PenyewaDAL())
        //    {

        //        nama_perusahaan = sewa.getNamaPerusahaan(id).ToString();
        //    }
        //    string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
        //    using (SqlConnection conn = new SqlConnection(connstring))
        //    {
        //        conn.Open();
        //        string query = "USE [MultiTenancy_Sablon]" +
        //            " INSERT INTO[dbo].[Produk_" + nama_perusahaan + "]" +
        //            "([nama_produk]" +
        //            ",[deskripsi]" +
        //            ",[kategori]" +
        //            ",[foto_produk])" +
        //            "VALUES" +
        //            "('" + namaproduk + "','" + deskripsi + "','" + kategori + "','" + fileName + "')";

        //        SqlCommand sqlcom = new SqlCommand(query, conn);
        //        try
        //        {
        //            sqlcom.ExecuteNonQuery();

        //            var res = "";
        //            sqlcom.CommandText = "SELECT [id_produk]" +
        //            " FROM[MultiTenancy_Sablon].[dbo].[Produk_" + nama_perusahaan + "] WHERE [nama_produk]='" + namaproduk + "'";

        //            using (SqlDataReader reader = sqlcom.ExecuteReader())
        //            {
        //                if (reader.Read())
        //                {
        //                    res = string.Format("{0}", reader["id_produk"].ToString());
        //                    idproduk = Convert.ToInt32(res);
        //                    Session["id_produk_gabung"] = idproduk.ToString();
        //                }
        //            }

        //            sqlcom.CommandText =
        //           " INSERT INTO [dbo].[TabelTambahan_" + nama_perusahaan + "]" +
        //           "([id_produk]," +
        //           "[nama_tambahan],[harga])" +
        //           " VALUES" +
        //           "(" + Session["id_produk_gabung"].ToString() + ",'" + tambahan1 + "'," + hargatam1fix + "),(" + Session["id_produk_gabung"].ToString() + ",'" + tambahan2 + "'," + hargatam2fix + "),(" + Session["id_produk_gabung"].ToString() + ",'" + tambahan3 + "'," + hargatam3fix + "),(" + Session["id_produk_gabung"].ToString() + ",'" + tambahan4 + "'," + hargatam4fix + "),(" + Session["id_produk_gabung"].ToString() + ",'" + tambahan5 + "'," + hargatam5fix + ")" +
        //               " delete from TabelTambahan_" + nama_perusahaan + " " +
        //               " where nama_tambahan is null or nama_tambahan = ''"
        //               ;
        //            sqlcom.ExecuteNonQuery();

        //            if (bahan1 != "" && sablon1 != "")
        //            {

        //                int idbahan;
        //                int idsablon;
        //                sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
        //                    "([id_produk]" +
        //                    ",[nama_bahan])" +
        //                    " output INSERTED.id_bahan VALUES" +
        //                    " (" + Session["id_produk_gabung"].ToString() + ",'" + bahan1 + "')";

        //                idbahan = (int)sqlcom.ExecuteScalar();


        //                sqlcom.CommandText = " INSERT INTO [dbo].[JenisSablon_" + nama_perusahaan + "]" +
        //                    "([id_produk]" +
        //                    ",[nama_sablon])" +
        //                    " output INSERTED.id_jns_sablon VALUES" +
        //                    " (" + Session["id_produk_gabung"].ToString() + ",'" + sablon1 + "')";

        //                idsablon = (int)sqlcom.ExecuteScalar();


        //                sqlcom.CommandText = "INSERT INTO [dbo].[Harga_" + nama_perusahaan + "]" +
        //                    " ([id_produk]" +
        //                    ",[id_bahan]" +
        //                    ",[id_jns_sablon]" +
        //                    ",[harga])" +
        //                    " VALUES" +
        //                    "(" + Session["id_produk_gabung"].ToString() +
        //                    "," + /*Session["id_bahan"].ToString()*/ idbahan.ToString() +
        //                    "," + /*Session["id_sablon"].ToString()*/ idsablon.ToString() +
        //                    "," + harga1 + ")";

        //                sqlcom.ExecuteNonQuery();

        //            }
        //            if (bahan2 != "" && sablon2 != "")
        //            {
        //                int idbahan;
        //                int idsablon;
        //                sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
        //                    "([id_produk]" +
        //                    ",[nama_bahan])" +
        //                    " output INSERTED.id_bahan VALUES" +
        //                    " (" + Session["id_produk_gabung"].ToString() + ",'" + bahan2 + "')";

        //                idbahan = (int)sqlcom.ExecuteScalar();


        //                sqlcom.CommandText = " INSERT INTO [dbo].[JenisSablon_" + nama_perusahaan + "]" +
        //                    "([id_produk]" +
        //                    ",[nama_sablon])" +
        //                    " output INSERTED.id_jns_sablon VALUES" +
        //                    " (" + Session["id_produk_gabung"].ToString() + ",'" + sablon2 + "')";

        //                idsablon = (int)sqlcom.ExecuteScalar();


        //                sqlcom.CommandText = "INSERT INTO [dbo].[Harga_" + nama_perusahaan + "]" +
        //                    " ([id_produk]" +
        //                    ",[id_bahan]" +
        //                    ",[id_jns_sablon]" +
        //                    ",[harga])" +
        //                    " VALUES" +
        //                    "(" + Session["id_produk_gabung"].ToString() +
        //                    "," + /*Session["id_bahan"].ToString()*/ idbahan.ToString() +
        //                    "," + /*Session["id_sablon"].ToString()*/ idsablon.ToString() +
        //                    "," + harga2 + ")";

        //                sqlcom.ExecuteNonQuery();
        //            }
        //            if (bahan3 != "" && sablon3 != "")
        //            {
        //                int idbahan;
        //                int idsablon;
        //                sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
        //                    "([id_produk]" +
        //                    ",[nama_bahan])" +
        //                    " output INSERTED.id_bahan VALUES" +
        //                    " (" + Session["id_produk_gabung"].ToString() + ",'" + bahan3 + "')";

        //                idbahan = (int)sqlcom.ExecuteScalar();


        //                sqlcom.CommandText = " INSERT INTO [dbo].[JenisSablon_" + nama_perusahaan + "]" +
        //                    "([id_produk]" +
        //                    ",[nama_sablon])" +
        //                    " output INSERTED.id_jns_sablon VALUES" +
        //                    " (" + Session["id_produk_gabung"].ToString() + ",'" + sablon3 + "')";

        //                idsablon = (int)sqlcom.ExecuteScalar();


        //                sqlcom.CommandText = "INSERT INTO [dbo].[Harga_" + nama_perusahaan + "]" +
        //                    " ([id_produk]" +
        //                    ",[id_bahan]" +
        //                    ",[id_jns_sablon]" +
        //                    ",[harga])" +
        //                    " VALUES" +
        //                    "(" + Session["id_produk_gabung"].ToString() +
        //                    "," + /*Session["id_bahan"].ToString()*/ idbahan.ToString() +
        //                    "," + /*Session["id_sablon"].ToString()*/ idsablon.ToString() +
        //                    "," + harga3 + ")";

        //                sqlcom.ExecuteNonQuery();
        //            }
        //            if (bahan4 != "" && sablon4 != "")
        //            {
        //                int idbahan;
        //                int idsablon;
        //                sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
        //                    "([id_produk]" +
        //                    ",[nama_bahan])" +
        //                    " output INSERTED.id_bahan VALUES" +
        //                    " (" + Session["id_produk_gabung"].ToString() + ",'" + bahan4 + "')";

        //                idbahan = (int)sqlcom.ExecuteScalar();


        //                sqlcom.CommandText = " INSERT INTO [dbo].[JenisSablon_" + nama_perusahaan + "]" +
        //                    "([id_produk]" +
        //                    ",[nama_sablon])" +
        //                    " output INSERTED.id_jns_sablon VALUES" +
        //                    " (" + Session["id_produk_gabung"].ToString() + ",'" + sablon4 + "')";

        //                idsablon = (int)sqlcom.ExecuteScalar();


        //                sqlcom.CommandText = "INSERT INTO [dbo].[Harga_" + nama_perusahaan + "]" +
        //                    " ([id_produk]" +
        //                    ",[id_bahan]" +
        //                    ",[id_jns_sablon]" +
        //                    ",[harga])" +
        //                    " VALUES" +
        //                    "(" + Session["id_produk_gabung"].ToString() +
        //                    "," + /*Session["id_bahan"].ToString()*/ idbahan.ToString() +
        //                    "," + /*Session["id_sablon"].ToString()*/ idsablon.ToString() +
        //                    "," + harga4 + ")";

        //                sqlcom.ExecuteNonQuery();
        //            }
        //            if (bahan5 != "" && sablon5 != "")
        //            {
        //                int idbahan;
        //                int idsablon;
        //                sqlcom.CommandText = "INSERT INTO [dbo].[Bahan_" + nama_perusahaan + "]" +
        //                    "([id_produk]" +
        //                    ",[nama_bahan])" +
        //                    " output INSERTED.id_bahan VALUES" +
        //                    " (" + Session["id_produk_gabung"].ToString() + ",'" + bahan5 + "')";

        //                idbahan = (int)sqlcom.ExecuteScalar();


        //                sqlcom.CommandText = " INSERT INTO [dbo].[JenisSablon_" + nama_perusahaan + "]" +
        //                    "([id_produk]" +
        //                    ",[nama_sablon])" +
        //                    " output INSERTED.id_jns_sablon VALUES" +
        //                    " (" + Session["id_produk_gabung"].ToString() + ",'" + sablon5 + "')";

        //                idsablon = (int)sqlcom.ExecuteScalar();


        //                sqlcom.CommandText = "INSERT INTO [dbo].[Harga_" + nama_perusahaan + "]" +
        //                    " ([id_produk]" +
        //                    ",[id_bahan]" +
        //                    ",[id_jns_sablon]" +
        //                    ",[harga])" +
        //                    " VALUES" +
        //                    "(" + Session["id_produk_gabung"].ToString() +
        //                    "," + /*Session["id_bahan"].ToString()*/ idbahan.ToString() +
        //                    "," + /*Session["id_sablon"].ToString()*/ idsablon.ToString() +
        //                    "," + harga5 + ")";

        //                sqlcom.ExecuteNonQuery();
        //            }



        //            TempData["Pesan"] = Helpers.Message.GetPesan("Berhasil !",
        //                                  "success", "data produk " + namaproduk + " berhasil ditambah");
        //        }
        //        catch (Exception ex)
        //        {
        //            TempData["Pesan"] = Helpers.Message.GetPesan("Error di tambah produk !",
        //                                  "danger", ex.Message);
        //        }

        //        conn.Close();
        //    }
        //    return View();
        //}

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
        public ActionResult IndexProdukBig()
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
        //public ActionResult IndexProdukReview()
        //{

        //    string nama_perusahaan = Session["nama_perusahaan"].ToString();
        //    //string nama_perusahaan = "b";            


        //    string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
        //    List<ProdukViewModel> result = new List<ProdukViewModel>();
        //    List<Bahan> result2 = new List<Bahan>();
        //    List<JenisSablon> result3 = new List<JenisSablon>();

        //    using (SqlConnection conn = new SqlConnection(connstring))
        //    {
        //        conn.Open();
        //        string query = "SELECT [id_produk]" +
        //            ",[nama_produk]" +
        //            ",[deskripsi]" +
        //            ",[foto_produk]" +
        //            "FROM[MultiTenancy_Sablon].[dbo].[Produk_" + nama_perusahaan + "]";

        //        SqlCommand sqlcom = new SqlCommand(query, conn);
        //        try
        //        {
        //            using (SqlDataReader reader = sqlcom.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    ProdukViewModel item = new ProdukViewModel()
        //                    {
        //                        id_produk = (int)reader["id_produk"],
        //                        nama_produk = reader["nama_produk"].ToString(),
        //                        deskripsi = reader["deskripsi"].ToString(),
        //                        foto_produk = reader["foto_produk"].ToString()
        //                    };
        //                    result.Add(item);
        //                }
        //            }

        //        }
        //        catch (Exception)
        //        {

        //        }
        //        sqlcom.CommandText = "SELECT [id_bahan]" +
        //            ",[id_produk]" +
        //            ",[nama_bahan]" +

        //            " FROM[MultiTenancy_Sablon].[dbo].[Bahan_" + nama_perusahaan + "]";
        //        using (SqlDataReader reader = sqlcom.ExecuteReader())
        //        {
        //            while (reader.Read())
        //            {
        //                ProdukViewModel item = new ProdukViewModel()
        //                {
        //                    id_produk = (int)reader["id_produk"],
        //                    Bahan.id = (int)["id_bahan"],
        //                    nama_bahan = reader["nama_bahan"].ToString()

        //                };
        //                result..Add(item);
        //            }
        //        }
        //        conn.Close();
        //    }
        //    return View(result);
        //}
        public ActionResult EditProduk(int id, string namaproduk)
        {
            string nama_perusahaan = Session["nama_perusahaan"].ToString();
            Session["id_produk"] = id.ToString();
            Session["nama_produk"] = namaproduk;

            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            Produk result = new Produk();

            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "SELECT [id_produk]" +
                    ",[nama_produk]" +
                    ",[deskripsi]" +
                    ",[foto_produk]" +
                    " FROM[MultiTenancy_Sablon].[dbo].[Produk_" + nama_perusahaan + "] WHERE [id_produk]=" + id.ToString();

                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {

                        if (reader.Read())
                        {
                            result.id_produk = (int)reader["id_produk"];
                            result.nama_produk = reader["nama_produk"].ToString();
                            result.deskripsi = reader["deskripsi"].ToString();
                            result.foto_produk = reader["foto_produk"].ToString();
                        };
                    }

                }
                catch (Exception)
                {

                }

                conn.Close();
            }
            return View(result);

        }
        [HttpPost]
        public ActionResult EditProduk(string nama_produk, string deskripsi, HttpPostedFileBase foto_produk)
        {
            string filePath = "";
            string fileName = Guid.NewGuid().ToString() + "_" + foto_produk.FileName;
            if (foto_produk.ContentLength > 0)
            {

                filePath = Path.Combine(HttpContext.Server.MapPath("~/Content/Images"), fileName);
                foto_produk.SaveAs(filePath);

            }
            string nama_perusahaan = Session["nama_perusahaan"].ToString();
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "USE [MultiTenancy_Sablon]" +
                    "UPDATE [dbo].[Produk_" + nama_perusahaan + "]" +
                    " SET [nama_produk] ='" + nama_produk + "'," +
                    "[deskripsi]='" + deskripsi + "'," +
                    "[foto_produk]='" + fileName + "'" +
                    " WHERE [id_produk]=" + Session["id_produk"].ToString();

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
            return RedirectToAction("IndexProdukBig");
        }
        public ActionResult DeleteProduk(int id)
        {
            string nama_perusahaan = Session["nama_perusahaan"].ToString();
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "USE [MultiTenancy_Sablon]" +
                    "DELETE FROM [dbo].[Produk_" + nama_perusahaan + "]" +
                    " WHERE [id_produk]=" + id;

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
            return RedirectToAction("IndexProdukBig");
        }
        public ActionResult DeleteBahan(int id)
        {
            string nama_perusahaan = Session["nama_perusahaan"].ToString();
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "USE [MultiTenancy_Sablon]" +
                    "DELETE FROM [dbo].[Bahan_" + nama_perusahaan + "]" +
                    " WHERE [id_bahan]=" + id;

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
            return RedirectToAction("IndexBahanEdit");
        }

        //public ActionResult CreateBahandanSablon()
        //{
        //    return View();
        //}

        //[HttpPost]

        //public ActionResult CreateBahandanSablon(int id)

        //{ 
        //    return View();
        //}

        public ActionResult DeleteSablon(int id)
        {
            string nama_perusahaan = Session["nama_perusahaan"].ToString();
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "USE [MultiTenancy_Sablon]" +
                    "DELETE FROM [dbo].[JenisSablon_" + nama_perusahaan + "]" +
                    " WHERE [id_jns_sablon]=" + id;

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
            return RedirectToAction("IndexJenisSablonEdit");
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
                        if (reader.Read())
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
        public ActionResult IndexBahan(int id)
        {
            Session["id_produk"] = id;
            Session["nama_produk"] = GetNamaproduk().ToString();

            string nama_perusahaan = Session["nama_perusahaan"].ToString();


            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            List<Bahan> result = new List<Bahan>();

            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "SELECT [id_bahan]" +
                    ",[id_produk]" +
                    ",[nama_bahan]" +
                    " FROM[MultiTenancy_Sablon].[dbo].[Bahan_" + nama_perusahaan + "] WHERE [id_produk]=" + Session["id_produk"].ToString();

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
        public ActionResult IndexBahanBig(int id)
        {
            Session["id_produk"] = id;
            Session["nama_produk"] = GetNamaproduk().ToString();

            string nama_perusahaan = Session["nama_perusahaan"].ToString();


            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            List<Bahan> result = new List<Bahan>();

            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "SELECT [id_bahan]" +
                    ",[id_produk]" +
                    ",[nama_bahan]" +
                    ",[harga]"+
                    " FROM[MultiTenancy_Sablon].[dbo].[Bahan_" + nama_perusahaan + "] WHERE [id_produk]=" + Session["id_produk"].ToString();

                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Bahan item = new Bahan()
                            {
                                id_bahan = (int)reader["id_bahan"],
                                id_produk = (int)reader["id_produk"],
                                nama_bahan = reader["nama_bahan"].ToString(),
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
        public ActionResult IndexBahanEdit()
        {

            Session["nama_produk"] = GetNamaproduk().ToString();

            string nama_perusahaan = Session["nama_perusahaan"].ToString();


            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            List<Bahan> result = new List<Bahan>();

            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "SELECT [id_bahan]" +
                    ",[id_produk]" +
                    ",[nama_bahan]" +
                    " FROM[MultiTenancy_Sablon].[dbo].[Bahan_" + nama_perusahaan + "] WHERE [id_produk]=" + Session["id_produk"].ToString();

                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Bahan item = new Bahan()
                            {
                                id_bahan = (int)reader["id_bahan"],
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
        public ActionResult CreateBahan()
        {


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



            return RedirectToAction("IndexBahan", "Penyewa", new { id = (int)Session["id_produk"] });
        }
        public ActionResult EditBahan(int id, string namabahan)
        {

            string nama_perusahaan = Session["nama_perusahaan"].ToString();
            Session["id_bahan"] = id.ToString();
            Session["nama_bahan"] = namabahan;

            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            Bahan result = new Bahan();

            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "SELECT [id_bahan]" +
                    ",[id_produk]" +
                    ",[nama_bahan]" +
                    " FROM[MultiTenancy_Sablon].[dbo].[Bahan_" + nama_perusahaan + "] WHERE [id_bahan]=" + id.ToString();

                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {

                        if (reader.Read())
                        {
                            result.id_bahan = (int)reader["id_bahan"];
                            result.id_produk = (int)reader["id_produk"];
                            result.nama_bahan = reader["nama_bahan"].ToString();
                        };
                    }

                }
                catch (Exception)
                {

                }

                conn.Close();
            }
            return View(result);


        }
        [HttpPost]
        public ActionResult EditBahan(string nama_bahan)
        {
            string nama_perusahaan = Session["nama_perusahaan"].ToString();
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "USE [MultiTenancy_Sablon]" +
                    "UPDATE [dbo].[Bahan_" + nama_perusahaan + "]" +
                    " SET [nama_bahan] ='" + nama_bahan + "'" +
                    " WHERE [id_bahan]=" + Session["id_bahan"].ToString();

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
            return RedirectToAction("IndexBahanEdit");
        }
        public ActionResult IndexJenisSablon(int id)
        {

            Session["id_produk"] = id;
            Session["nama_produk"] = GetNamaproduk().ToString();
            string nama_perusahaan = Session["nama_perusahaan"].ToString();


            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            List<JenisSablon> result = new List<JenisSablon>();

            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "SELECT [id_jns_sablon]" +
                    ",[id_produk]" +
                    ",[nama_sablon]" +
                    " FROM[MultiTenancy_Sablon].[dbo].[JenisSablon_" + nama_perusahaan + "] WHERE [id_produk]=" + Session["id_produk"].ToString();

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
                catch (Exception ex)
                {
                    TempData["Pesan"] = Helpers.Message.GetPesan("Error !",
                                          "danger", ex.Message);
                }

                conn.Close();
            }
            return View(result);
        }
        public ActionResult IndexJenisSablonBig(int id)
        {

            Session["id_produk"] = id;
            Session["nama_produk"] = GetNamaproduk().ToString();
            string nama_perusahaan = Session["nama_perusahaan"].ToString();


            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            List<JenisSablon> result = new List<JenisSablon>();

            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "SELECT [id_jns_sablon]" +
                    ",[id_produk]" +
                    ",[nama_sablon]" +
                    " FROM[MultiTenancy_Sablon].[dbo].[JenisSablon_" + nama_perusahaan + "] WHERE [id_produk]=" + Session["id_produk"].ToString();

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
                catch (Exception ex)
                {
                    TempData["Pesan"] = Helpers.Message.GetPesan("Error !",
                                          "danger", ex.Message);
                }

                conn.Close();
            }
            return View(result);
        }
        public ActionResult IndexJenisSablonEdit()
        {


            Session["nama_produk"] = GetNamaproduk().ToString();
            string nama_perusahaan = Session["nama_perusahaan"].ToString();


            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            List<JenisSablon> result = new List<JenisSablon>();

            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "SELECT [id_jns_sablon]" +
                    ",[id_produk]" +
                    ",[nama_sablon]" +
                    " FROM[MultiTenancy_Sablon].[dbo].[JenisSablon_" + nama_perusahaan + "] WHERE [id_produk]=" + Session["id_produk"].ToString();

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
                catch (Exception ex)
                {
                    TempData["Pesan"] = Helpers.Message.GetPesan("Error !",
                                          "danger", ex.Message);
                }

                conn.Close();
            }
            return View(result);
        }
        public ActionResult EditJenisSablon(int id, string namasablon)
        {

            string nama_perusahaan = Session["nama_perusahaan"].ToString();
            Session["id_sablon"] = id.ToString();
            Session["nama_sablon"] = namasablon;

            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            JenisSablon result = new JenisSablon();

            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "SELECT [id_jns_sablon]" +
                    ",[id_produk]" +
                    ",[nama_sablon]" +
                    " FROM[MultiTenancy_Sablon].[dbo].[JenisSablon_" + nama_perusahaan + "] WHERE [id_jns_sablon]=" + id.ToString();

                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {

                        if (reader.Read())
                        {
                            result.id_jns_sablon = (int)reader["id_jns_sablon"];
                            result.id_produk = (int)reader["id_produk"];
                            result.nama_sablon = reader["nama_sablon"].ToString();
                        };
                    }

                }
                catch (Exception)
                {

                }

                conn.Close();
            }
            return View(result);


        }
        [HttpPost]
        public ActionResult EditJenisSablon(string nama_sablon)
        {
            string nama_perusahaan = Session["nama_perusahaan"].ToString();
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "USE [MultiTenancy_Sablon]" +
                    "UPDATE [dbo].[JenisSablon_" + nama_perusahaan + "]" +
                    " SET [nama_sablon] ='" + nama_sablon + "'" +
                    " WHERE [id_jns_sablon]=" + Session["id_sablon"].ToString();

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
            return RedirectToAction("IndexJenisSablonEdit");
        }
        public ActionResult CreateJenisSablon()
        {


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



            return RedirectToAction("IndexJenisSablon", "Penyewa", new { id = (int)Session["id_produk"] });
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

        public ActionResult EditHarga(int id)
        {
            string nama_perusahaan = Session["nama_perusahaan"].ToString();
            Session["id_harga"] = id.ToString();

            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            Harga result = new Harga();

            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "SELECT [harga]" +

                    " FROM[MultiTenancy_Sablon].[dbo].[Harga_" + nama_perusahaan + "] WHERE [id_harga]=" + id.ToString();

                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {

                        if (reader.Read())
                        {
                            result.harga = (int)reader["harga"];

                        };
                    }

                }
                catch (Exception)
                {

                }

                conn.Close();
            }
            return View(result);

        }

        [HttpPost]

        public ActionResult EditHarga(string harga)
        {
            string nama_perusahaan = Session["nama_perusahaan"].ToString();
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "USE [MultiTenancy_Sablon]" +
                    "UPDATE [dbo].[Harga_" + nama_perusahaan + "]" +
                    " SET [harga] =" + harga +
                    " WHERE [id_harga]=" + Session["id_harga"].ToString();

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
            return RedirectToAction("IndexViewHargaFix");
        }

        public List<Bahan> getbahan()
        {
            string nama_perusahaan = Session["nama_perusahaan"].ToString();
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;

            List<Bahan> result = new List<Bahan>();
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();

                string query = "SELECT[id_bahan]" +
                    ",[id_produk]" +
                    ",[nama_bahan]" +
                    ",[harga]" +
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
                                id_bahan = (int)reader["id_bahan"],
                                nama_bahan = reader["nama_bahan"].ToString(),
                              
                                id_produk = (int)reader["id_produk"],
                                harga = (int)reader["harga"],
                               


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
            return result;
        }

        public List<JenisSablon> getsablon()
        {
            string nama_perusahaan = Session["nama_perusahaan"].ToString();
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;

            List<JenisSablon> result = new List<JenisSablon>();
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();

                string query = "SELECT[id_jns_sablon]" +
                    ",[id_produk]" +
                    ",[nama_sablon]" +
                    ",[harga]" +
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
                                nama_sablon = reader["nama_sablon"].ToString(),

                                id_produk = (int)reader["id_produk"],
                                harga = (int)reader["harga"],



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
            return result;
        }
        public List<Tambahan> gettambahan()
        {
            string nama_perusahaan = Session["nama_perusahaan"].ToString();
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;

            List<Tambahan> result = new List<Tambahan>();
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();

                string query = "SELECT[id_tambahan]" +
                    ",[id_produk]" +
                    ",[nama_tambahan]" +
                    ",[harga]" +
                    " FROM[MultiTenancy_Sablon].[dbo].[TabelTambahan_" + nama_perusahaan + "]";
                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Tambahan item = new Tambahan()
                            {
                                id_tambahan = (int)reader["id_tambahan"],
                                nama_tambahan = reader["nama_tambahan"].ToString(),

                                id_produk = (int)reader["id_produk"],
                                harga = (int)reader["harga"],



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
            return result;
        }

        public ActionResult IndexProdukLengkap()
        {
            string nama_perusahaan = Session["nama_perusahaan"].ToString();


            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            List<Produk> result = new List<Produk>();

            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "SELECT [id_produk],[nama_produk],[deskripsi],[kategori],[foto_produk] " +
                    " FROM[MultiTenancy_Sablon].[dbo].[Produk_"+nama_perusahaan+"]";

                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Produk item = new Produk()
                            {
                                nama_produk = reader["nama_produk"].ToString(),
                                deskripsi = reader["deskripsi"].ToString(),
                                kategori = reader["kategori"].ToString(),
                                foto_produk = reader["foto_produk"].ToString(),
                                id_produk = (int)reader["id_produk"],
                               


                            };
                            result.Add(item);
                        }

                    }
                }
                catch (Exception)
                {

                }
            }
            ViewBag.Bahan = getbahan();
            ViewBag.Sablon = getsablon();
            ViewBag.Tambahan = gettambahan();
            return View(result);
        }

        public ActionResult IndexViewHargaFix()
        {
            string nama_perusahaan = Session["nama_perusahaan"].ToString();


            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            List<ProdukLengkapViewModel> result = new List<ProdukLengkapViewModel>();

            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "SELECT [nama_bahan]" +
                    ",[harga]" +
                    ",[nama_sablon]" +
                    ",[nama_produk]" +
                    ",[nama_tambahan]" +
                    ",[Harga_tambahan]" +
                    ",[foto_produk]" +
                    ",[id_harga]" +
                    " FROM[MultiTenancy_Sablon].[dbo].[View_" + nama_perusahaan + "]";

                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ProdukLengkapViewModel item = new ProdukLengkapViewModel()
                            {
                                nama_produk = reader["nama_produk"].ToString(),
                                nama_bahan = reader["nama_bahan"].ToString(),
                                nama_sablon = reader["nama_sablon"].ToString(),
                                foto_produk = reader["foto_produk"].ToString(),
                                nama_tambahan = reader["nama_tambahan"].ToString(),
                                //harga = (int)reader["harga"],
                                harga_tambahan = (int)reader["Harga_tambahan"],
                                id_harga = (int)reader["id_harga"]

                            };
                            result.Add(item);
                        }

                    }
                }
                catch (Exception)
                {

                }
            }
            return View(result);
        }

        public ActionResult IndexViewHarga()
        {
            string nama_perusahaan = Session["nama_perusahaan"].ToString();


            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["PendaftaranTenant"].ConnectionString;
            List<HargaViewModel> result = new List<HargaViewModel>();

            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "SELECT [nama_bahan]" +
                    ",[nama_sablon]" +
                    ",[nama_produk]" +
                    ",[harga]" +
                    ",[id_bahan]" +
                    ",[id_jns_sablon]" +
                    ",[id_produk]" +
                    " FROM [MultiTenancy_Sablon].[dbo].[View_" + nama_perusahaan + "]";

                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            if (reader["nama_sablon"] == DBNull.Value && reader["id_jns_sablon"] == DBNull.Value && reader["harga"] == DBNull.Value && reader["nama_bahan"] == DBNull.Value && reader["id_bahan"] == DBNull.Value)
                            {
                                HargaViewModel item = new HargaViewModel()
                                {
                                    nama_bahan = null,
                                    nama_sablon = null,
                                    nama_produk = reader["nama_produk"].ToString(),
                                    harga = 0,
                                    id_bahan = null,
                                    id_jns_sablon = null,
                                    id_produk = (int)reader["id_produk"]

                                };
                                result.Add(item);
                            }
                            else if (reader["nama_bahan"] == DBNull.Value && reader["id_bahan"] == DBNull.Value && reader["harga"] == DBNull.Value)
                            {
                                HargaViewModel item = new HargaViewModel()
                                {
                                    nama_bahan = null,
                                    nama_sablon = reader["nama_sablon"].ToString(),
                                    nama_produk = reader["nama_produk"].ToString(),
                                    harga = 0,
                                    id_bahan = null,
                                    id_jns_sablon = (int)reader["id_jns_sablon"],
                                    id_produk = (int)reader["id_produk"]

                                };
                                result.Add(item);
                            }
                            else if (reader["nama_sablon"] == DBNull.Value && reader["id_jns_sablon"] == DBNull.Value && reader["harga"] == DBNull.Value)
                            {
                                HargaViewModel item = new HargaViewModel()
                                {
                                    nama_bahan = reader["nama_bahan"].ToString(),
                                    nama_sablon = null,
                                    nama_produk = reader["nama_produk"].ToString(),
                                    harga = 0,
                                    id_bahan = (int)reader["id_bahan"],
                                    id_jns_sablon = null,
                                    id_produk = (int)reader["id_produk"]

                                };
                                result.Add(item);
                            }
                            else if (reader["harga"] == DBNull.Value)
                            {
                                HargaViewModel item = new HargaViewModel()
                                {
                                    nama_bahan = reader["nama_bahan"].ToString(),
                                    nama_sablon = reader["nama_sablon"].ToString(),
                                    nama_produk = reader["nama_produk"].ToString(),
                                    harga = 0,
                                    id_bahan = (int)reader["id_bahan"],
                                    id_jns_sablon = (int)reader["id_jns_sablon"],
                                    id_produk = (int)reader["id_produk"]

                                };
                                result.Add(item);
                            }
                            else
                            {
                                HargaViewModel item = new HargaViewModel()
                                {
                                    nama_bahan = reader["nama_bahan"].ToString(),
                                    nama_sablon = reader["nama_sablon"].ToString(),
                                    nama_produk = reader["nama_produk"].ToString(),
                                    harga = (int)reader["harga"],
                                    id_bahan = (int)reader["id_bahan"],
                                    id_jns_sablon = (int)reader["id_jns_sablon"],
                                    id_produk = (int)reader["id_produk"]

                                };
                                result.Add(item);
                            }


                        }
                    }

                }
                catch (Exception ex)
                {
                    //TempData["Pesan"] = Helpers.Message.GetPesan("Error !",
                    //                      "danger", ex.Message);
                }

                conn.Close();
            }
            return View(result);
        }
        public ActionResult CreateViewHarga()
        {
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
                string query = "CREATE VIEW [dbo].[View_" + nama_perusahaan + "]" +
                    " AS" +
                    " SELECT        dbo.Bahan_" + nama_perusahaan + ".nama_bahan, dbo.Harga_" + nama_perusahaan + ".harga, dbo.JenisSablon_" + nama_perusahaan + ".nama_sablon, dbo.Produk_" + nama_perusahaan + ".nama_produk, dbo.TabelTambahan_" + nama_perusahaan + ".nama_tambahan, " +
                    "dbo.TabelTambahan_" + nama_perusahaan + ".harga AS Harga_tambahan, dbo.Produk_" + nama_perusahaan + ".foto_produk, dbo.Harga_" + nama_perusahaan + ".id_harga, dbo.Bahan_" + nama_perusahaan + ".id_bahan, dbo.JenisSablon_" + nama_perusahaan + ".id_jns_sablon, dbo.Produk_" + nama_perusahaan + ".id_produk" +
                    " FROM            dbo.Bahan_" + nama_perusahaan + " INNER JOIN" +
                    " dbo.Harga_" + nama_perusahaan + " ON dbo.Bahan_" + nama_perusahaan + ".id_bahan = dbo.Harga_" + nama_perusahaan + ".id_bahan INNER JOIN" +
                    " dbo.JenisSablon_" + nama_perusahaan + " ON dbo.Harga_" + nama_perusahaan + ".id_jns_sablon = dbo.JenisSablon_" + nama_perusahaan + ".id_jns_sablon INNER JOIN" +
                    " dbo.Produk_" + nama_perusahaan + " ON dbo.Bahan_" + nama_perusahaan + ".id_produk = dbo.Produk_" + nama_perusahaan + ".id_produk AND dbo.Harga_" + nama_perusahaan + ".id_produk = dbo.Produk_" + nama_perusahaan + ".id_produk AND" +
                    " dbo.JenisSablon_" + nama_perusahaan + ".id_produk = dbo.Produk_" + nama_perusahaan + ".id_produk INNER JOIN" +
                    " dbo.TabelTambahan_" + nama_perusahaan + " ON dbo.Produk_" + nama_perusahaan + ".id_produk = dbo.TabelTambahan_" + nama_perusahaan + ".id_produk";

                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    sqlcom.ExecuteNonQuery();
                    //TempData["Pesan"] = Helpers.Message.GetPesan("Berhasil bikin view!",
                    //"success", "Tabel untuk perusahaan " + nama_perusahaan + " berhasil ditambah");
                }
                catch (Exception)
                {
                    //TempData["Pesan"] = Helpers.Message.GetPesan("Error bikin view !",
                    //"danger", ex.Message);
                    return RedirectToAction("IndexProdukLengkap", "Penyewa");

                    //return RedirectToAction("IndexViewHarga", "Penyewa");
                }


                conn.Close();
            }
            return RedirectToAction("IndexProdukLengkap", "Penyewa");

        }
        public ActionResult AddHarga(int? idbahan, int idproduk, int? idsablon, int harga)
        {
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
                string query = "";
                if (idbahan == null)
                {
                    query = "USE [MultiTenancy_Sablon]" +
                   "INSERT INTO[dbo].[Harga_" + nama_perusahaan + "]" +
                   "([id_produk]" +
                   ",[id_jns_sablon]" +
                   ",[harga])" +
                   "VALUES" +
                   "(" + idproduk.ToString() +
                   "," + idsablon.ToString() +
                   "," + harga + ")";
                }
                else if (idsablon == null)
                {
                    query = "USE [MultiTenancy_Sablon]" +
                   "INSERT INTO[dbo].[Harga_" + nama_perusahaan + "]" +
                   "([id_produk]" +
                   ",[id_bahan]" +
                   ",[harga])" +
                   "VALUES" +
                   "(" + idproduk.ToString() +
                   "," + idbahan.ToString() +
                   "," + harga + ")";
                }
                else
                {
                    query = "USE [MultiTenancy_Sablon]" +
                   "INSERT INTO[dbo].[Harga_" + nama_perusahaan + "]" +
                   "([id_produk]" +
                   ",[id_bahan]" +
                   ",[id_jns_sablon]" +
                   ",[harga])" +
                   "VALUES" +
                   "(" + idproduk.ToString() +
                   "," + idbahan.ToString() +
                   "," + idsablon.ToString() +
                   "," + harga + ")";
                }


                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    sqlcom.ExecuteNonQuery();

                }
                catch (Exception ex)
                {

                    //TempData["Pesan"] = Helpers.Message.GetPesan("Error bikin view !",
                    //"danger", ex.Message);


                }

                conn.Close();

            }
            return RedirectToAction("IndexViewHarga", "Penyewa");
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

                    "[tgl_lahir] [varchar] (50) NULL," +

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

                    "[tgl_lahir] [varchar] (50) NULL," +

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
                    "[kategori] [varchar] (20) NULL," +
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
                    "[harga] [int] NULL," +
                    "CONSTRAINT[PK_Bahan_" + nama_perusahaan + "] PRIMARY KEY CLUSTERED" +
                    "(" +
                    "[id_bahan] ASC" +
                    ")WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]" +
                    ") ON[PRIMARY]" +

                    " ALTER TABLE[dbo].[Bahan_" + nama_perusahaan + "] WITH CHECK ADD CONSTRAINT[FK_Bahan_" + nama_perusahaan + "_Produk_" + nama_perusahaan + "] FOREIGN KEY([id_produk])" +
                    " REFERENCES[dbo].[Produk_" + nama_perusahaan + "]" +
                    "([id_produk])" +
                    " ON DELETE CASCADE" +

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
                    "[harga] [int] NULL," +
                    "CONSTRAINT[PK_JenisSablon_" + nama_perusahaan + "] PRIMARY KEY CLUSTERED" +
                    "(" +
                    "[id_jns_sablon] ASC" +
                    ")WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]" +
                    ") ON[PRIMARY]" +

                    " ALTER TABLE[dbo].[JenisSablon_" + nama_perusahaan + "] WITH CHECK ADD CONSTRAINT[FK_JenisSablon_" + nama_perusahaan + "_Produk_" + nama_perusahaan + "] FOREIGN KEY([id_produk])" +
                    " REFERENCES[dbo].[Produk_" + nama_perusahaan + "]" +
                    "([id_produk])" +
                    " ON DELETE CASCADE" +

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
                    " ON DELETE CASCADE" +
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

                     "[status_bayar] [char](5) NULL," +
                      "[tgl_order] [date] NULL," +
                    "[total_harga] [int] NULL," +
                    "[dikirim] [char](5) NULL," +
                    " CONSTRAINT[PK_Order_" + nama_perusahaan + "] PRIMARY KEY CLUSTERED" +
                    " (" +
                    " [no_order] ASC" +
                    ")WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]" +
                    ") ON[PRIMARY]" +

                    " ALTER TABLE[dbo].[Order_" + nama_perusahaan + "] WITH CHECK ADD CONSTRAINT[FK_Order_" + nama_perusahaan + "_Customer_" + nama_perusahaan + "] FOREIGN KEY([id_customer])" +
                    " REFERENCES[dbo].[Customer_" + nama_perusahaan + "]" +
                    " ([id_cutomer])" +

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
                    "[warna][varchar](12) NULL," +
                    "[desain] [varchar](100) NULL," +
                    "[jumlah] [int] NULL," +
                    "[subtotal] [int] NULL," +
                    "[catatan] [varchar](200) NULL," +
                    "CONSTRAINT[PK_DetailOrder_" + nama_perusahaan + "] PRIMARY KEY CLUSTERED" +
                    "(" +
                    "[no_detail] ASC" +
                    ")WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]" +
                    ") ON[PRIMARY]" +

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
                    "[tambahan][varchar](100) NULL," +
                    "CONSTRAINT[PK_UkuranOrder_" + nama_perusahaan + "]" + " PRIMARY KEY CLUSTERED" +
                    "(" +
                    "[id_ukuran_order] ASC" +
                    ")WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]" +

                    ") ON[PRIMARY]" +

                    " ALTER TABLE[dbo].[UkuranOrder_" + nama_perusahaan + "] WITH CHECK ADD CONSTRAINT[FK_UkuranOrder_" + nama_perusahaan + "_DetailOrder_" + nama_perusahaan + "] FOREIGN KEY([no_detail])" +
                    " REFERENCES[dbo].[DetailOrder_" + nama_perusahaan + "]" +
                    "([no_detail]) ON DELETE CASCADE" +

                    " ALTER TABLE[dbo].[UkuranOrder_" + nama_perusahaan + "]" +
                    " CHECK CONSTRAINT[FK_UkuranOrder_" + nama_perusahaan + "_DetailOrder_" + nama_perusahaan + "]" +

                    " ALTER TABLE[dbo].[UkuranOrder_" + nama_perusahaan + "] WITH CHECK ADD CONSTRAINT[FK_UkuranOrder_" + nama_perusahaan + "_Ukuran_" + nama_perusahaan + "] FOREIGN KEY([id_ukuran])" +
                    " REFERENCES[dbo].[Ukuran_" + nama_perusahaan + "]" +
                    "([id_ukuran])" +

                    " ALTER TABLE[dbo].[UkuranOrder_" + nama_perusahaan + "]" +
                    " CHECK CONSTRAINT[FK_UkuranOrder_" + nama_perusahaan + "_Ukuran_" + nama_perusahaan + "]" +
                    " USE [MultiTenancy_Sablon]" +
                    " SET ANSI_NULLS ON" +
                    " SET QUOTED_IDENTIFIER ON" +
                    " CREATE TABLE[dbo].[TabelTambahan_" + nama_perusahaan + "](" +
                    "[id_tambahan][int] IDENTITY(1, 1) NOT NULL," +
                    "[id_produk] [int] NULL," +
                    "[nama_tambahan] [varchar](50) NULL," +
                    "[harga] [int] NULL," +
                    "CONSTRAINT[PK_TabelTambahan_" + nama_perusahaan + "] PRIMARY KEY CLUSTERED" +
                    "(" +
                    "[id_tambahan] ASC" +
                    ")WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]" +
                    ") ON[PRIMARY]" +
                    " ALTER TABLE[dbo].[TabelTambahan_" + nama_perusahaan + "] WITH CHECK ADD CONSTRAINT[FK_TabelTambahan_" + nama_perusahaan + "_TabelTambahan_" + nama_perusahaan + "] FOREIGN KEY([id_produk])" +
                    " REFERENCES[dbo].[Produk_" + nama_perusahaan + "]" +
                    "([id_produk])" +
                    " ON DELETE CASCADE" +
                    " ALTER TABLE[dbo].[TabelTambahan_" + nama_perusahaan + "]" +
                    " CHECK CONSTRAINT[FK_TabelTambahan_" + nama_perusahaan + "_TabelTambahan_" + nama_perusahaan + "]" +
                    " INSERT INTO [dbo].[Ukuran_" + nama_perusahaan + "]" +
                    "([ukuran])" +
                    " VALUES" +
                    " ('Anak'), ('XS'), ('S'), ('M'), ('L'), ('XL'), ('XXL'), ('XXXL'), ('4XL'), ('5XL')"


                   ;
                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    sqlcom.ExecuteNonQuery();
                    TempData["Pesan"] = Helpers.Message.GetPesan("Berhasil !",
                                          "success", "Tabel untuk perusahaan " + nama_perusahaan + " berhasil ditambah");
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

        public ActionResult Selesai()
        {
            return View();
        }

    }
}
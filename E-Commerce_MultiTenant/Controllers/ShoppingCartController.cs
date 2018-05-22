using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using E_Commerce_MultiTenant.Models;

namespace E_Commerce_MultiTenant.Controllers
{
    public class ShoppingCartController : Controller
    {
        // GET: ShoppingCart
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ShowCart(string subdomain)
        {
            if (Session["noorder"] != null)
            {


                string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["ECommerce"].ConnectionString;
                List<DetailOrder> result = new List<DetailOrder>();

                using (SqlConnection conn = new SqlConnection(connstring))
                {
                    conn.Open();
                    string query = "SELECT d.[no_detail]" +
                        ", d.[no_order]" +
                        ",d.[id_produk]" +
                        ",d.[id_bahan]" +
                        ",d.[id_jns_sablon]" +
                        ",d.[desain]" +
                        ",d.[jumlah]" +
                        ",d.[subtotal]" +
                        ",d.[catatan]," +
                        "p.nama_produk," +
                        "b.nama_bahan," +
                        "j.nama_sablon" +
                        " FROM DetailOrder_" + subdomain + " d inner join Produk_" + subdomain + " p on d.id_produk = p.id_produk inner join Bahan_" + subdomain + " b on d.id_bahan = b.id_bahan" +
                        " inner join JenisSablon_" + subdomain + " j on d.id_jns_sablon = j.id_jns_sablon WHERE d.no_order =" + Session["noorder"].ToString();

                    SqlCommand sqlcom = new SqlCommand(query, conn);
                    try
                    {
                        using (SqlDataReader reader = sqlcom.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DetailOrder item = new DetailOrder()
                                {
                                    no_detail = (int)reader["no_detail"],
                                    no_order = (int)reader["no_order"],
                                    id_produk = (int)reader["id_produk"],
                                    id_bahan = (int)reader["id_bahan"],
                                    id_jns_sablon = (int)reader["id_jns_sablon"],
                                    desain = reader["desain"].ToString(),
                                    jumlah = (int)reader["jumlah"],
                                    subtotal = (int)reader["subtotal"],
                                    namaproduk = reader["nama_produk"].ToString(),
                                    namabahan = reader["nama_bahan"].ToString(),
                                    namasablon = reader["nama_sablon"].ToString()

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
            else
            {
                return RedirectToAction("CartKosong");
            }
        }
        public ActionResult CartKosong()
        {
            return View();
        }

        public ActionResult Invoice(string subdomain, string kirim)
        {
            int totalharga = 0;
            ViewBag.noorder = Session["noorder"].ToString();
            List<DetailOrder> result = new List<DetailOrder>();
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["ECommerce"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                string query = "select sum(subtotal) from DetailOrder_" + subdomain + " where no_order =" + Session["noorder"].ToString();
                SqlCommand sqlcom = new SqlCommand(query, conn);
                try
                {
                    totalharga = (int)sqlcom.ExecuteScalar();

                    sqlcom.CommandText = "UPDATE [dbo].[Order_" + subdomain + "] SET [total_harga]=" + totalharga + ", [dikirim]='"+kirim+"' WHERE no_order=" + Session["noorder"].ToString();
                    sqlcom.ExecuteNonQuery();
                    

                    sqlcom.CommandText = "SELECT [nama_customer],[no_telp],[alamat]" +
               " FROM[MultiTenancy_Sablon].[dbo].[Customer_" + subdomain + "] WHERE email_customer='" + Session["email"].ToString() + "'";

                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            ViewBag.namacustomer = reader["nama_customer"].ToString();
                            ViewBag.notelp = reader["no_telp"].ToString();
                            ViewBag.alamat = reader["alamat"].ToString();
                        }
                    }
                    sqlcom.CommandText = "SELECT [tgl_order],[dikirim],[total_harga]" +
               " FROM[MultiTenancy_Sablon].[dbo].[Order_" + subdomain + "] WHERE no_order=" + Session["noorder"].ToString();
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            ViewBag.tglorder = Convert.ToDateTime(reader["tgl_order"]).ToString("yyyy-MM-dd");
                            Session["dikirim"]= reader["dikirim"].ToString();

                            ViewBag.totalharga = reader["total_harga"].ToString();
                        }
                    }

                    sqlcom.CommandText = "SELECT d.[no_detail]" +
                        ", d.[no_order]" +
                        ",d.[id_produk]" +
                        ",d.[id_bahan]" +
                        ",d.[id_jns_sablon]" +
                        ",d.[desain]" +
                        ",d.[jumlah]" +
                        ",d.[subtotal]" +
                        ",d.[catatan]," +
                        "p.nama_produk," +
                        "b.nama_bahan," +
                        "j.nama_sablon" +
                        " FROM DetailOrder_" + subdomain + " d inner join Produk_" + subdomain + " p on d.id_produk = p.id_produk inner join Bahan_" + subdomain + " b on d.id_bahan = b.id_bahan" +
                        " inner join JenisSablon_" + subdomain + " j on d.id_jns_sablon = j.id_jns_sablon WHERE d.no_order =" + Session["noorder"].ToString();
                    using (SqlDataReader reader = sqlcom.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DetailOrder item = new DetailOrder()
                            {
                                no_detail = (int)reader["no_detail"],
                                no_order = (int)reader["no_order"],
                                id_produk = (int)reader["id_produk"],
                                id_bahan = (int)reader["id_bahan"],
                                id_jns_sablon = (int)reader["id_jns_sablon"],
                                desain = reader["desain"].ToString(),
                                jumlah = (int)reader["jumlah"],
                                subtotal = (int)reader["subtotal"],
                                namaproduk = reader["nama_produk"].ToString(),
                                namabahan = reader["nama_bahan"].ToString(),
                                namasablon = reader["nama_sablon"].ToString()

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
            Session["noorder"] = null;
            return View(result);
        }

        public JsonResult SendMailToUser(string table)
        {
           
            bool result = false;

            result = SendEmail(Session["email"].ToString(), "Konfirmasi pemesanan dan informasi pembayaran",
                "<p>Pemesanan anda telah berhasil,<br />" +
               
                "Pesanan yang telah dilakukan berupa : <br />"+
                table +
                "<br />" +
                "Dengan total biaya sebesar : "+ ViewBag.totalharga +
                "Biaya pemesanan dapat dibayarkan melalui rekening : <br />" +
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


        public ActionResult KonfirmasiKirim()
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
            Session["totalhargaall"] = null;
            Session["desain"] = null;
            Session["jumlahorder"] = null;
            Session["catatan"] = null;
            Session["anak"] = null;
            Session["xs"] = null;
            Session["s"] = null;
            Session["m"] = null;
            Session["l"] = null;
            Session["xl"] = null;
            Session["xxl"] = null;
            Session["xxxl"] = null;
            Session["4xl"] = null;
            Session["5xl"] = null;


            return RedirectToAction("Index", "Produk");
        }
    }
}
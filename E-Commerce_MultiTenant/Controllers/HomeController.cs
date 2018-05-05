using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using E_Commerce_MultiTenant.DAL;
using E_Commerce_MultiTenant.Models;

namespace E_Commerce_MultiTenant.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string subdomain)
        {
            //var host = this.Request.Headers["Host"].Split('.');
            //string nama_perusahaan = host[0];
            Session["nama_perusahaan"] = subdomain;

            //return RedirectToAction("Indexku","Home");
            return View();
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
        public ActionResult Indexku(string subdomain)
        {
            //var host = this.Request.Headers["Host"].Split('.');
            //string nama_perusahaan = host[0];
            Session["nama_perusahaan"] = subdomain;

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
                        "FROM[MultiTenancy_Sablon].[dbo].[Data_carausel] WHERE id_ui="+id_ui.ToString();

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

                    }
                    catch (Exception)
                    {

                    }

                    conn.Close();
                }
                return View(result);
            }

                
        }
    }
}
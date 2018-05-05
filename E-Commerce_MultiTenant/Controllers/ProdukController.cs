using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace E_Commerce_MultiTenant.Controllers
{
    public class ProdukController : Controller
    {
        // GET: Produk
        public ActionResult Index()
        {
            //var fullAddress = HttpContent.Request.Headers["Host"].Split('.');
            var host = this.Request.Headers["Host"].Split('.');
            string nama_perusahaan = host[0];
            Session["nama_perusahaan"] = nama_perusahaan;
            return View();
        }
    }
}
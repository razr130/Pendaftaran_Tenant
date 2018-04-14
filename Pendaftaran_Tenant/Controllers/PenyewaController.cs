using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pendaftaran_Tenant.Models;
using Pendaftaran_Tenant.DAL;

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
            if(results != null)
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
                    }
                    return RedirectToAction("Index", "Home");
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
            return RedirectToAction("Index","Home");
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
    }
}
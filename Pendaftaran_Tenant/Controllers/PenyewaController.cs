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
        // GET: Penyewa
        public ActionResult Index()
        {

            using (PenyewaDAL svpenyewa = new PenyewaDAL())
            {
                var result = svpenyewa.GetData().ToList();
                return View(result);
            }
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]

    public ActionResult Create(Penyewa penyewa)
        {
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
                    //TempData["Pesan"] = Helpers.Message.GetPesan("Error !",
                    //                      "danger", ex.Message);
                }
            }
            return RedirectToAction("Index");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Pendaftaran_Tenant.Models;

namespace Pendaftaran_Tenant.DAL
{
    public class PenyewaDAL : IDisposable
    {
        private PendaftaranTenant db = new PendaftaranTenant();

        public IQueryable<Penyewa> GetData()
        {
            var results = from b in db.Penyewas
                          orderby b.nama_perusahaan
                          select b;

            return results;
        }

        public void Add(Penyewa penyewa)
        {
            try
            {
                db.Penyewas.Add(penyewa);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void LogIn(string txtEmail, string txtPassword)
        {
            var results = db.Penyewas.SingleOrDefault(m => m.email == txtEmail && m.password == txtPassword);
  
        }




        public void Dispose()
        {
            db.Dispose();
        }
    }
}
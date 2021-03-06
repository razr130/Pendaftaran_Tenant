﻿using System;
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
        public IQueryable<Penyewa> GetDataBaru(int id)
        {
            var results = from b in db.Penyewas
                          orderby b.nama_perusahaan
                          where b.id_penyewa == id
                          select b;

            return results;
        }
        public IQueryable<Data_carausel>GetDataCarausel(int id)
        {
            var results = from b in db.Data_carausel
                          where b.id_ui == id
                          select b;
            return results;

        }

        public Penyewa GetDataById(int id)

        {

            var result = (from b in db.Penyewas

                          where b.id_penyewa == id

                          select b).SingleOrDefault();

            return result;

        }

        

        public void AddUI(Data_UI ui)
        {
            try
            {
                db.Data_UI.Add(ui);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void AddCarausel(Data_carausel carausel)
        {
            try
            {
                db.Data_carausel.Add(carausel);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
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

        public string getRole(string email)
        {
            var results = (from b in db.Penyewas where b.email == email select b.role).SingleOrDefault();
            return results;
        }
        public int getId(string email)
        {
            var results = (from b in db.Penyewas where b.email == email select b.id_penyewa).SingleOrDefault();
            return results;
        }
        public int getIdUI(int id_penyewa)
        {
            var results = (from b in db.Data_UI where b.id_penyewa == id_penyewa select b.id_ui).SingleOrDefault();
            return results;
        }

        public string getNamaPerusahaan(int id)
        {
            var results = (from b in db.Penyewas where b.id_penyewa == id select b.nama_perusahaan).SingleOrDefault();
            return results;
        }

        public void LogIn(string txtEmail, string txtPassword)
        {
            var results = db.Penyewas.SingleOrDefault(m => m.email == txtEmail && m.password == txtPassword);
  
        }

        public void Edit(Penyewa pny)
        {
            var result = GetDataById(pny.id_penyewa);

            if (result != null)

            {
                result.status_bayar = pny.status_bayar;
                db.SaveChanges();
            }
            else
            {
                throw new Exception("Data tidak ditemukan !");
            }

        }


        public void Dispose()
        {
            db.Dispose();
        }
    }
}
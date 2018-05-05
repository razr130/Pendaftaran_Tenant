using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace E_Commerce_MultiTenant.Models
{
    public class Penyewa
    {
        
        public int id_penyewa { get; set; }

        
        public string nama_perusahaan { get; set; }

       
        public string email { get; set; }

    
        public string password { get; set; }

        
        public string alamat { get; set; }

        
        public string no_telp { get; set; }

        public bool status_bayar { get; set; }

     
        public string role { get; set; }

    }
}
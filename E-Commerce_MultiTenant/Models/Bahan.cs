using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace E_Commerce_MultiTenant.Models
{
    public class Bahan
    {
        
        public int id_bahan { get; set; }
        public int id_produk { get; set; }
        
        public string nama_bahan { get; set; }
        public int harga { get; set; }
    }
}
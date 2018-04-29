using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pendaftaran_Tenant.Models
{
    public class ProdukViewModel
    {
        
        public int id_produk { get; set; }       
        public string nama_produk { get; set; }        
        public string deskripsi { get; set; }        
        public string foto_produk { get; set; }

        public List<Bahan> Bahan { get; set; }
        public List<JenisSablon> JenisSablon { get; set; }
    }
}
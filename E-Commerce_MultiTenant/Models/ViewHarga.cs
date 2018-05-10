using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace E_Commerce_MultiTenant.Models
{
    public class ViewHarga
    {
        public int id_produk { get; set; }
        public int id_bahan { get; set; }
        public int id_jns_sablon { get; set; }
        public int id_harga { get; set; }
        public string nama_bahan { get; set; }
        public string nama_sablon { get; set; }
        public string nama_tambahan { get; set; }
        public string foto_produk { get; set; }
        public int harga { get; set; }
        public int harga_tambahan { get; set; }

    }
}
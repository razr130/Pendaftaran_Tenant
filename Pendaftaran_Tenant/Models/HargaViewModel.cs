using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pendaftaran_Tenant.Models
{
    public class HargaViewModel
    {
        public string nama_bahan { get; set; }
        public string nama_sablon { get; set; }
        public string nama_produk { get; set; }
        public int? harga { get; set; }
        public int? id_bahan { get; set; }
        public int? id_jns_sablon { get; set; }
        public int id_produk { get; set; }
    }
}
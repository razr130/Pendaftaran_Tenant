using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pendaftaran_Tenant.Models
{
    public class ProdukLengkapViewModel
    {
        public string nama_bahan { get; set; }
        public string nama_sablon { get; set; }
        public string nama_produk { get; set; }
        public string foto_produk { get; set; }
        public string nama_tambahan { get; set; }
        public int harga { get; set; }
        public int harga_tambahan { get; set; }
    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pendaftaran_Tenant.Models
{
    public class Harga
    {
        public int id_harga { get; set; }
        public int id_produk { get; set; }
        public int id_bahan { get; set; }
        public int id_jns_sablon { get; set; }
        public int harga { get; set; }
    }
}
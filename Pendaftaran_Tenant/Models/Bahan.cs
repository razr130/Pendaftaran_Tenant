using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pendaftaran_Tenant.Models
{
    public class Bahan
    {
        [Key]
        public int id_bahan { get; set; }
        public int id_produk { get; set; }
        [StringLength(50)]
        public string nama_bahan { get; set; }
        public int harga { get; set; }
    }
}
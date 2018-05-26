using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pendaftaran_Tenant.Models
{
    public class Tambahan
    {
        [Key]
        public int id_tambahan { get; set; }
        public int id_produk { get; set; }
        [StringLength(50)]
        public string nama_tambahan { get; set; }
        public int harga { get; set; }
    }
}
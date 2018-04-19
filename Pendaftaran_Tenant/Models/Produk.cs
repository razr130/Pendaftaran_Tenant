using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pendaftaran_Tenant.Models
{
    public class Produk
    {
        [Key]
        public int id_produk { get; set; }
        [StringLength(50)]
        public string nama_produk { get; set; }
        [StringLength(100)]
        public string deskripsi { get; set; }
        [StringLength(100)]
        public string foto_produk { get; set; }

    }
}
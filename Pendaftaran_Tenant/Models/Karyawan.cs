using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pendaftaran_Tenant.Models
{
    public class Karyawan
    {
        [Key]
        public int id_karyawan { get; set; }
        [StringLength(50)]
        public string nama_karyawan { get; set; }
        [StringLength(50)]
        public string email_karyawan { get; set; }
        [StringLength(50)]
        public string password { get; set; }
        [StringLength(15)]
        public string tempat_lahir { get; set; }
        public string tgl_lahir { get; set; }
        [StringLength(12)]
        public string no_telp { get; set; }
        [StringLength(100)]
        public string alamat { get; set; }
        public string jns_kelamin { get; set; }
    }
}
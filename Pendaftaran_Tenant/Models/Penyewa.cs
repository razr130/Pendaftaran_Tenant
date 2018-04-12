namespace Pendaftaran_Tenant
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Penyewa")]
    public partial class Penyewa
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Penyewa()
        {
            Data_UI = new HashSet<Data_UI>();
        }

        [Key]
        public int id_penyewa { get; set; }

        [StringLength(50)]
        public string nama_perusahaan { get; set; }

        [StringLength(50)]
        public string email { get; set; }

        [StringLength(50)]
        public string password { get; set; }

        [StringLength(50)]
        public string alamat { get; set; }

        [StringLength(12)]
        public string no_telp { get; set; }

        public bool? status_bayar { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Data_UI> Data_UI { get; set; }
    }
}

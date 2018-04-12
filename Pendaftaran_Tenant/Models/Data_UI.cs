namespace Pendaftaran_Tenant
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Data_UI
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Data_UI()
        {
            Data_carausel = new HashSet<Data_carausel>();
        }

        [Key]
        public int id_ui { get; set; }

        public int id_penyewa { get; set; }

        [Column(TypeName = "image")]
        public byte[] logo { get; set; }

        [StringLength(7)]
        public string warna_bg { get; set; }

        [StringLength(7)]
        public string warna_navbar { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Data_carausel> Data_carausel { get; set; }

        public virtual Penyewa Penyewa { get; set; }
    }
}

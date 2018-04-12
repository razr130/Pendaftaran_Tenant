namespace Pendaftaran_Tenant
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Data_carausel
    {
        [Key]
        public int id_carausel { get; set; }

        public int id_ui { get; set; }

        [Column(TypeName = "image")]
        public byte[] gambar { get; set; }

        public virtual Data_UI Data_UI { get; set; }
    }
}

namespace Pendaftaran_Tenant
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class PendaftaranTenant : DbContext
    {
        public PendaftaranTenant()
            : base("name=PendaftaranTenant")
        {
        }

        public virtual DbSet<Data_carausel> Data_carausel { get; set; }
        public virtual DbSet<Data_UI> Data_UI { get; set; }
        public virtual DbSet<Penyewa> Penyewas { get; set; }
        public virtual DbSet<sysdiagram> sysdiagrams { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Data_UI>()
                .Property(e => e.warna_bg)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Data_UI>()
                .Property(e => e.warna_navbar)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Data_UI>()
                .HasMany(e => e.Data_carausel)
                .WithRequired(e => e.Data_UI)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Penyewa>()
                .Property(e => e.nama_perusahaan)
                .IsUnicode(false);

            modelBuilder.Entity<Penyewa>()
                .Property(e => e.email)
                .IsUnicode(false);

            modelBuilder.Entity<Penyewa>()
                .Property(e => e.password)
                .IsUnicode(false);

            modelBuilder.Entity<Penyewa>()
                .Property(e => e.alamat)
                .IsUnicode(false);

            modelBuilder.Entity<Penyewa>()
                .Property(e => e.no_telp)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Penyewa>()
                .HasMany(e => e.Data_UI)
                .WithRequired(e => e.Penyewa)
                .WillCascadeOnDelete(false);
        }
    }
}

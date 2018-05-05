namespace E_Commerce_MultiTenant.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class ECommerce : DbContext
    {
        public ECommerce()
            : base("name=ECommerce")
        {
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}

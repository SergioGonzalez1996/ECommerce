using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace ECommerce.Models
{
    public class ECommerceContext : DbContext
    {
        public ECommerceContext() : base("DefaultConnection")
        {
            
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
        }


        public DbSet<ECommerce.Models.Department> Departments { get; set; }

        public DbSet<ECommerce.Models.City> Cities { get; set; }

        public DbSet<ECommerce.Models.Company> Companies { get; set; }

        public DbSet<ECommerce.Models.User> Users { get; set; }

        public System.Data.Entity.DbSet<ECommerce.Models.Category> Categories { get; set; }

        public System.Data.Entity.DbSet<ECommerce.Models.Tax> Taxes { get; set; }

        public System.Data.Entity.DbSet<ECommerce.Models.Product> Products { get; set; }
    }
}
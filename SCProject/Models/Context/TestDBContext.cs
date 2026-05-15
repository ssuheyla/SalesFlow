using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace SCProject.Models.Context
{
    public class TestDBContext : DbContext
    {
        public TestDBContext(DbContextOptions<TestDBContext> options) : base(options)
        {

        }

        public DbSet<Category> Category { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<Purchase> Purchase { get; set; }
        public DbSet<Sales> Sales { get; set; }
        public DbSet<Stock> Stock { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly()); ///çalışmış olduğum assembly (katmanı) tara Configuration için

            //  modelBuilder.UseOpenIddict();  // string türünde kimlik          

        }

    }
}

using CoreAdvanceConcepts.Models;
using Microsoft.EntityFrameworkCore;

namespace CoreAdvanceConcepts.DataContext
{
    public class EmployeeDbContext : DbContext
    {
        public EmployeeDbContext(DbContextOptions<EmployeeDbContext> options): base(options)
        {   
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                .Property(x => x.FlagDeleted)
                .HasDefaultValue(false);
        }
        public DbSet<Employee> Employees { get; set; }
    }
}

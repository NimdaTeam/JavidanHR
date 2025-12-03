using HrSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Infrastructure.Context
{
    public class HrSystemContext:DbContext
    {
        public HrSystemContext(DbContextOptions<HrSystemContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasIndex(e => e.EmployeeCode)
                    .IsUnique();

                entity.HasIndex(e => e.NationalCode)
                    .IsUnique();

                entity.Property(e => e.BaseSalary)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Benefits)
                    .HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<Employee>(entity => entity.HasQueryFilter(e => !e.IsDeleted));

            base.OnModelCreating(modelBuilder);
        }
    }
}

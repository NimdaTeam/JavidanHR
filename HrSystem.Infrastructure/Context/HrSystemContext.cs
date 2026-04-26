using HrSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HrSystem.Infrastructure.Context
{
    public class HrSystemContext : DbContext
    {
        public HrSystemContext(DbContextOptions<HrSystemContext> options) : base(options) { }

        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<EmployeeMilitaryService> EmployeeMilitaryServices { get; set; } = null!;
        public DbSet<EmployeeEducation> EmployeeEducations { get; set; } = null!;
        public DbSet<EmployeeMaritalInfo> EmployeeMaritalInfos { get; set; } = null!;
        public DbSet<EmployeeAddress> EmployeeAddresses { get; set; } = null!;
        public DbSet<EmployeeFamilyMember> EmployeeFamilyMembers { get; set; } = null!;
        public DbSet<EmployeeTraining> EmployeeTrainings { get; set; } = null!;
        public DbSet<EmployeeWorkExperience> EmployeeWorkExperiences { get; set; } = null!;
        public DbSet<EmployeeLoan> EmployeeLoans { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ================================
            // 1. Employee - جدول اصلی
            // ================================
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("Employees");

                // ایندکس‌های یونیک فقط برای رکوردهای فعال
                entity.HasIndex(e => new { e.EmployeeCode, e.IsDeleted })
                      .IsUnique();

                entity.HasIndex(e => new { e.NationalCode, e.IsDeleted })
                      .IsUnique();

                // ایندکس‌های پرکاربرد برای جستجو و فیلتر
                entity.HasIndex(e => e.MobilePhone);
                entity.HasIndex(e => e.IsProfileCompletedByEmployee);
                entity.HasIndex(e => e.IsApprovedByAdmin);
                entity.HasIndex(e => e.Department);
                entity.HasIndex(e => e.Position);

                entity.Property(e => e.BaseSalary).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Benefits).HasColumnType("decimal(18,2)");

                // Soft Delete فقط برای Employee (مثل قبل)
                entity.HasQueryFilter(e => !e.IsDeleted);

                // روابط One-to-One با Cascade Delete
                entity.HasOne(e => e.MilitaryService)
                      .WithOne(m => m.Employee)
                      .HasForeignKey<EmployeeMilitaryService>(m => m.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Education)
                      .WithOne(m => m.Employee)
                      .HasForeignKey<EmployeeEducation>(m => m.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.MaritalInfo)
                      .WithOne(m => m.Employee)
                      .HasForeignKey<EmployeeMaritalInfo>(m => m.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.AddressInfo)
                      .WithOne(m => m.Employee)
                      .HasForeignKey<EmployeeAddress>(m => m.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ================================
            // بقیه جداول — Soft Delete هم دارن
            // ================================
            var softDeleteEntityTypes = modelBuilder.Model.GetEntityTypes()
                .Where(t => t.ClrType.GetProperty("IsDeleted") != null);

            foreach (var entityType in softDeleteEntityTypes)
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, "IsDeleted");
                var falseConstant = Expression.Constant(false);
                var equalExpression = Expression.Equal(property, falseConstant);
                var lambda = Expression.Lambda(equalExpression, parameter);

                entityType.SetQueryFilter(lambda);
            }

            // ================================
            // تنظیمات جداول فرزند
            // ================================
            modelBuilder.Entity<EmployeeMilitaryService>(entity =>
            {
                entity.ToTable("EmployeeMilitaryServices");
                entity.HasIndex(e => e.EmployeeId).IsUnique();
                entity.Property(e => e.Status).HasConversion<int>();
            });

            modelBuilder.Entity<EmployeeEducation>(entity =>
            {
                entity.ToTable("EmployeeEducations");
                entity.HasIndex(e => e.EmployeeId).IsUnique();
                entity.Property(e => e.Level).HasConversion<int>();
            });

            modelBuilder.Entity<EmployeeMaritalInfo>(entity =>
            {
                entity.ToTable("EmployeeMaritalInfos");
                entity.HasIndex(e => e.EmployeeId).IsUnique();
                entity.Property(e => e.MaritalStatus).HasConversion<int>();
            });

            modelBuilder.Entity<EmployeeAddress>(entity =>
            {
                entity.ToTable("EmployeeAddresses");
                entity.HasIndex(e => e.EmployeeId).IsUnique();
                entity.Property(e => e.HousingStatus).HasConversion<int>();
            });

            modelBuilder.Entity<EmployeeFamilyMember>(entity =>
            {
                entity.ToTable("EmployeeFamilyMembers");
                entity.HasIndex(e => e.EmployeeId);
                entity.HasIndex(e => e.Relation);
            });

            modelBuilder.Entity<EmployeeTraining>(entity =>
            {
                entity.ToTable("EmployeeTrainings");
                entity.HasIndex(e => e.EmployeeId);
                entity.HasIndex(e => e.CourseName);
            });

            modelBuilder.Entity<EmployeeWorkExperience>(entity =>
            {
                entity.ToTable("EmployeeWorkExperiences");
                entity.HasIndex(e => e.EmployeeId);
                entity.HasIndex(e => e.Organization);
                entity.HasIndex(e => new { e.StartDate, e.EndDate });
                entity.Property(e => e.Type).HasConversion<int>();
            });

            modelBuilder.Entity<EmployeeLoan>(entity =>
            {
                entity.ToTable("EmployeeLoans");
                entity.HasIndex(e => e.EmployeeId);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
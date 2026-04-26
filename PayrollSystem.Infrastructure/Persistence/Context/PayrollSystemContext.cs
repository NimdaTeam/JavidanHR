using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayrollSystem.Domain.Entities.CalculationLog;
using PayrollSystem.Domain.Entities.Contract;
using PayrollSystem.Domain.Entities.Contract.ContractPayItem;
using PayrollSystem.Domain.Entities.PayItem;
using PayrollSystem.Domain.Entities.PaySlip;
using PayrollSystem.Domain.Entities.PaySlip.PaySlipItem;
using PayrollSystem.Domain.Entities.Workshop;

namespace PayrollSystem.Infrastructure.Persistence.Context
{
    public class PayrollSystemContext : DbContext
    {
        public PayrollSystemContext(DbContextOptions<PayrollSystemContext> options) : base(options) { }

        public DbSet<Workshop> Workshops { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ContractPayItem> ContractPayItems { get; set; }
        public DbSet<PayItem> PayItems { get; set; }
        public DbSet<PayItemFormula> PayItemFormulas { get; set; }
        public DbSet<PaySlip> PaySlips { get; set; }
        public DbSet<PaySlipItem> PaySlipItems { get; set; }
        public DbSet<CalculationLog> CalculationLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ---------------------- Workshop ----------------------
            modelBuilder.Entity<Workshop>(entity =>
            {
                entity.HasKey(w => w.Id);
                entity.Property(w => w.Code).HasMaxLength(10).IsRequired();
                entity.Property(w => w.Name).IsRequired();
                entity.Property(w => w.EmployerName).IsRequired();
                entity.Property(w => w.Address).IsRequired();
                entity.Property(w => w.EmployeeInsuranceRate).HasDefaultValue(7);
                entity.Property(w => w.EmployerInsuranceRate).HasDefaultValue(20);
                entity.Property(w => w.UnEmploymentInsuranceRate).HasDefaultValue(3);
                entity.HasIndex(w => w.Code).IsUnique();
            });

            // ---------------------- Contract ----------------------
            modelBuilder.Entity<Contract>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.EmployeeId).IsRequired();
                entity.Property(c => c.WorkshopId).IsRequired();
                entity.Property(c => c.ValidFromDate).IsRequired();
                entity.Property(c => c.Status).HasConversion<int>();

                // رابطه با Workshop (یک به چند)
                entity.HasOne<Workshop>()
                      .WithMany()
                      .HasForeignKey(c => c.WorkshopId)
                      .OnDelete(DeleteBehavior.Restrict); // جلوگیری از حذف کارگاه دارای قرارداد

                // رابطه با ContractPayItem (یک به چند)
                entity.HasMany(c => c.PayItems)
                      .WithOne()
                      .HasForeignKey(cpi => cpi.ContractId)
                      .OnDelete(DeleteBehavior.Cascade);

                // رابطه با PaySlip (یک به چند)
                entity.HasMany<PaySlip>()
                      .WithOne()
                      .HasForeignKey(ps => ps.ContractId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(c => c.EmployeeId);
                entity.HasIndex(c => new { c.EmployeeId, c.ValidFromDate });
            });

            // ---------------------- ContractPayItem ----------------------
            modelBuilder.Entity<ContractPayItem>(entity =>
            {
                entity.HasKey(cpi => cpi.Id);
                entity.Property(cpi => cpi.ContractId).IsRequired();
                entity.Property(cpi => cpi.PayItemId).IsRequired();
                entity.Property(cpi => cpi.Value).HasPrecision(18, 2);

                // رابطه با PayItem (ارجاعی، بدون FK در دیتابیس در صورت عدم دسترسی به HrSystem)
                // در صورت وجود PayItem در همان دیتابیس می‌توان FK تعریف کرد
                entity.HasIndex(cpi => cpi.PayItemId);
                entity.HasIndex(cpi => new { cpi.ContractId, cpi.PayItemId }).IsUnique();
            });

            // ---------------------- PayItem ----------------------
            modelBuilder.Entity<PayItem>(entity =>
            {
                entity.HasKey(pi => pi.Id);
                entity.Property(pi => pi.Name).HasMaxLength(100).IsRequired();
                entity.Property(pi => pi.SystemCode).HasMaxLength(50).IsRequired();
                entity.Property(pi => pi.Type).HasConversion<int>();
                entity.Property(pi => pi.DataType).HasConversion<int>();
                entity.Property(pi => pi.SortOrder).HasDefaultValue(0);
                entity.HasIndex(pi => pi.SystemCode).IsUnique();

                // رابطه با PayItemFormula
                entity.HasMany(pi => pi.Formulas)
                      .WithOne()
                      .HasForeignKey(f => f.PayItemId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Metadata.FindNavigation(nameof(PayItem.Formulas))
                    ?.SetPropertyAccessMode(PropertyAccessMode.Field);
            });

            // ---------------------- PayItemFormula ----------------------
            modelBuilder.Entity<PayItemFormula>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.Property(f => f.Formula).HasMaxLength(500).IsRequired();
                entity.Property(f => f.Version).IsRequired();
                entity.Property(f => f.ValidFromDate).IsRequired();
                entity.Property(f => f.IsActive).IsRequired();
                entity.HasIndex(f => new { f.PayItemId, f.IsActive });
                entity.HasIndex(f => new { f.PayItemId, f.ValidFromDate, f.ValidToDate });
            });

            // ---------------------- PaySlip ----------------------
            modelBuilder.Entity<PaySlip>(entity =>
            {
                entity.HasKey(ps => ps.Id);
                entity.Property(ps => ps.EmployeeId).IsRequired();
                entity.Property(ps => ps.ContractId).IsRequired();
                entity.Property(ps => ps.Year).IsRequired();
                entity.Property(ps => ps.Month).IsRequired();
                entity.Property(ps => ps.Status).HasConversion<int>();
                entity.Property(ps => ps.TotalEarnings).HasPrecision(18, 2);
                entity.Property(ps => ps.TotalDeductions).HasPrecision(18, 2);
                entity.Property(ps => ps.NetPay).HasPrecision(18, 2);
                entity.HasIndex(ps => new { ps.EmployeeId, ps.Year, ps.Month }).IsUnique();

                // رابطه با PaySlipItem
                entity.HasMany(ps => ps.Items)
                      .WithOne()
                      .HasForeignKey(item => item.PaySlipId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ---------------------- PaySlipItem ----------------------
            modelBuilder.Entity<PaySlipItem>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.PaySlipId).IsRequired();
                entity.Property(item => item.PayItemId).IsRequired();
                entity.Property(item => item.CalculatedValue).HasPrecision(18, 2);
                entity.Property(item => item.ManualOverrideValue).HasPrecision(18, 2);
                entity.Property(item => item.FinalValue).HasPrecision(18, 2);
                entity.HasIndex(item => new { item.PaySlipId, item.PayItemId }).IsUnique();
            });

            // ---------------------- CalculationLog ----------------------
            modelBuilder.Entity<CalculationLog>(entity =>
            {
                entity.HasKey(log => log.Id);
                entity.Property(log => log.PaySlipId).IsRequired();
                entity.Property(log => log.PayItemId).IsRequired();
                entity.Property(log => log.InputValuesJson).HasMaxLength(4000).IsRequired();
                entity.Property(log => log.FormulaUsed).HasMaxLength(500).IsRequired();
                entity.Property(log => log.ResultValue).HasPrecision(18, 2);
                entity.Property(log => log.CalculatedAt).IsRequired();
                entity.HasIndex(log => log.PaySlipId);
                entity.HasIndex(log => log.PayItemId);
            });

           

            // ---------------------- Query Filters -------------------------
            modelBuilder.Entity<Workshop>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Contract>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<PayItem>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<PaySlip>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<CalculationLog>().HasQueryFilter(e => !e.IsDeleted);
        }
    }
}
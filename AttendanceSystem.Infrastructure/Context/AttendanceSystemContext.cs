using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AttendanceSystem.Domain.Entities;

namespace AttendanceSystem.Infrastructure.Context
{
    public class AttendanceSystemContext : DbContext
    {
        public AttendanceSystemContext(DbContextOptions<AttendanceSystemContext> options) : base(options) { }

        public DbSet<ManualAttendanceRequest> ManualAttendanceRequests { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ManualAttendanceRequest>(entity =>
            {
                entity.ToTable("ManualAttendanceRequests");

                // ایندکس‌های یونیک فقط برای رکوردهای فعال
                entity.HasIndex(e => new { e.AttendanceTimes, e.IsDeleted })
                      .IsUnique();

                entity.HasIndex(e => new { e.Username, e.IsDeleted });

                entity.HasIndex(e => new { e.UserId, e.IsDeleted });
            });

            modelBuilder.Entity<LeaveRequest>(entity =>
            {
                entity.ToTable("LeaveRequests");

                // ایندکس‌های یونیک فقط برای رکوردهای فعال
                entity.HasIndex(e => new { e.UserId,e.StartDate,e.EndDate, e.IsDeleted })
                    .IsUnique();

                entity.HasIndex(e => new { e.Username, e.IsDeleted });

                entity.HasIndex(e => new { e.UserId, e.IsDeleted });
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
            base.OnModelCreating(modelBuilder);
        }
    }
}

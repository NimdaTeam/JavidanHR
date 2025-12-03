using AuthenticationSystem.Domain.Role;
using AuthenticationSystem.Domain.RolePermission;
using AuthenticationSystem.Domain.User;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationSystem.Infrastructure
{
    public class AuthenticationSystemContext :DbContext
    {
        public DbSet<Users> Users { get; set; }

        public DbSet<Roles> Roles { get; set; }

        public DbSet<RolePermissions> RolePermissions { get; set; }

        public DbSet<OtpCodes> OtpCodes { get; set; }

        public DbSet<UserRoles> UserRoles { get; set; }

        public DbSet<UserLoginHistory> UserLoginHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Users
            modelBuilder.Entity<Users>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();

            // Roles
            modelBuilder.Entity<Roles>()
                .HasIndex(u => u.Name)
                .IsUnique();

            // OtpCode
            modelBuilder.Entity<OtpCodes>()
                .HasIndex(o => new { o.PhoneNumber, o.CreationDate });
            modelBuilder.Entity<OtpCodes>()
                .HasIndex(o => o.ExpireDate);


            // UserLoginHistory
            modelBuilder.Entity<UserLoginHistory>()
                .HasIndex(h => h.UserId);
            modelBuilder.Entity<UserLoginHistory>()
                .HasIndex(h => h.LoginAt);
            modelBuilder.Entity<UserLoginHistory>()
                .HasIndex(h => h.IpAddress);
            modelBuilder.Entity<UserLoginHistory>()
                .HasIndex(h => new { h.UserId, h.IsSuccessful });



            modelBuilder.Entity<Users>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<RolePermissions>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Roles>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<UserRoles>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<OtpCodes>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<UserLoginHistory>().HasQueryFilter(u => !u.IsDeleted);
        }

        public AuthenticationSystemContext(DbContextOptions<AuthenticationSystemContext> options): base(options) { }
    }
}

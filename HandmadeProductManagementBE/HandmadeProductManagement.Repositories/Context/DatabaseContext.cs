using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Repositories.Entity;

namespace HandmadeProductManagement.Repositories.Context
{
    public class DatabaseContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid, ApplicationUserClaims, ApplicationUserRoles, ApplicationUserLogins, ApplicationRoleClaims, ApplicationUserTokens>
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        // user
        public virtual DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
        public virtual DbSet<ApplicationRole> ApplicationRoles => Set<ApplicationRole>();
        public virtual DbSet<ApplicationUserClaims> ApplicationUserClaims => Set<ApplicationUserClaims>();
        public virtual DbSet<ApplicationUserRoles> ApplicationUserRoles => Set<ApplicationUserRoles>();
        public virtual DbSet<ApplicationUserLogins> ApplicationUserLogins => Set<ApplicationUserLogins>();
        public virtual DbSet<ApplicationRoleClaims> ApplicationRoleClaims => Set<ApplicationRoleClaims>();
        public virtual DbSet<ApplicationUserTokens> ApplicationUserTokens => Set<ApplicationUserTokens>();

        public virtual DbSet<UserInfo> UserInfos => Set<UserInfo>();
        public virtual DbSet<Shop> Shops => Set<Shop>();
        public virtual DbSet<Order> Orders => Set<Order>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration for Shop entity
            modelBuilder.Entity<Shop>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Rating).HasDefaultValue(0);
                //entity.HasOne(e => e.User)
                //      .WithOne(u => u.Shop)
                //      .HasForeignKey<Shop>(e => e.UserId)
                //      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuration for Order entity
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalPrice).IsRequired();
                entity.Property(e => e.OrderDate).IsRequired();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(200);
                entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(15);
                entity.Property(e => e.Note).HasMaxLength(500);
                //entity.HasOne(e => e.User)
                //      .WithMany(u => u.Orders)
                //      .HasForeignKey(e => e.UserId)
                //      .OnDelete(DeleteBehavior.Cascade);
                //entity.HasOne(e => e.CancelReason)
                //      .WithMany(cr => cr.Orders)
                //      .HasForeignKey(e => e.CancelReasonId)
                //      .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }   
}

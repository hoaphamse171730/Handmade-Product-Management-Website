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

        public DbSet<Variation> Variations => Set<Variation>();
        public DbSet<VariationOption> VariationOptions => Set<VariationOption>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Payment Entity Configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.OrderId)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.ExpirationDate)
                      .IsRequired();

                entity.Property(e => e.TotalAmount)
                      .IsRequired();

                entity.HasOne(e => e.Order)
                      .WithMany()
                      .HasForeignKey(e => e.OrderId);

                // One-to-one relationship with PaymentDetail
                entity.HasOne<PaymentDetail>()
                      .WithOne(pd => pd.Payment)
                      .HasForeignKey<PaymentDetail>(pd => pd.PaymentId);
            });

            // PaymentDetail Entity Configuration
            modelBuilder.Entity<PaymentDetail>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.PaymentId)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.Status)
                      .IsRequired()
                      .HasMaxLength(15);

                entity.Property(e => e.Amount)
                      .IsRequired();

                entity.Property(e => e.Method)
                      .IsRequired()
                      .HasMaxLength(30);

                entity.Property(e => e.ExternalTransaction)
                      .HasMaxLength(100);
            });
        }

    }

}
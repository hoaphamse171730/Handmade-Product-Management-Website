using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Repositories.Entity;

namespace HandmadeProductManagement.Repositories.Context
{
    public class DatabaseContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid, ApplicationUserClaims,
        ApplicationUserRoles, ApplicationUserLogins, ApplicationRoleClaims, ApplicationUserTokens>
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

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

            modelBuilder.Entity<Variation>(entity =>
            {
                entity.ToTable("Variation");
                
                entity.HasOne(v => v.Category)
                    .WithMany(c => c.Variations)
                    .HasForeignKey(v => v.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(v => v.Name)
                    .HasColumnType("text")
                    .HasMaxLength(150)
                    .IsRequired();
                
            });


            modelBuilder.Entity<VariationOption>(entity =>
            {
                entity.ToTable("VariationOption");

                entity.Property(vo => vo.Value)
                    .HasColumnType("text")
                    .HasMaxLength(150)
                    .IsRequired();
                
                entity.HasOne(vo => vo.Variation)
                    .WithMany(v => v.VariationOptions)
                    .HasForeignKey(v => v.VariationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
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
        public virtual DbSet<Promotion> Promotions => Set<Promotion>();
        public virtual DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
        public DbSet<Variation> Variations => Set<Variation>();
        public DbSet<VariationOption> VariationOptions => Set<VariationOption>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Promotion
            modelBuilder.Entity<Promotion>()  
                .HasKey(p => p.PromotionId);
            modelBuilder.Entity<Promotion>()  
                .Property(p => p.PromotionName)  
                .IsRequired() 
                .HasMaxLength(255);
            modelBuilder.Entity<Promotion>()  
                .Property(p => p.Description)  
                .HasMaxLength(255);
            modelBuilder.Entity<Promotion>()  
                .HasMany(p => p.Categories) 
                .WithOne(c => c.Promotion) 
                .HasForeignKey(c => c.PromotionId);
            
            // OrderDetail  
            modelBuilder.Entity<OrderDetail>()  
                .HasKey(od => od.OrderDetailId); 
            modelBuilder.Entity<OrderDetail>()  
                .HasOne(od => od.Order)  
                .WithMany(o => o.OrderDetails)  
                .HasForeignKey(od => od.OrderId) 
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<OrderDetail>()  
                .HasOne(od => od.Product)  
                .WithMany(p => p.OrderDetails)  
                .HasForeignKey(od => od.ProductId) 
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
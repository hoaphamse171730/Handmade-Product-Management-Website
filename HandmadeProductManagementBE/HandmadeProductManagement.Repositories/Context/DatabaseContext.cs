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


            //Quan he giua cart va user
            modelBuilder.Entity<Cart>()
                .HasOne(c=>c.User)
                .WithOne()
                .HasForeignKey<Cart>(c=>c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            //Quan he giua cart va cartItem
            modelBuilder.Entity<Cart>()
                .HasMany(c=>c.CartItems)
                .WithOne()
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);


            // Quan hệ giữa CartItem và ProductItem 
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.ProductItem)  
                .WithMany(pi => pi.CartItem)  
                .HasForeignKey(ci => ci.ProductItemId)  
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ giữa ProductConfiguration và ProductItem (1-N)
            modelBuilder.Entity<ProductConfiguration>()
                .HasOne(pc => pc.ProductItem)  
                .WithMany(pi => pi.ProductConfiguration) 
                .HasForeignKey(pc => pc.ProductItemId)  
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ giữa ProductConfiguration và VariationOption (1-N)
            modelBuilder.Entity<ProductConfiguration>()
                .HasOne(pc => pc.VariationOption)  
                .WithMany(vo => vo.ProductConfiguration)  
                .HasForeignKey(pc => pc.VariationOptionId)  
                .OnDelete(DeleteBehavior.Restrict);

        }




    }

}
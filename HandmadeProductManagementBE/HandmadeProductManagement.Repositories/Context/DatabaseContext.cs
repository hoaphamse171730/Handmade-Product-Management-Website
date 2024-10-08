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
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<Reply> Replies => Set<Reply>();
        public DbSet<Shop> Shops => Set<Shop>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<CancelReason> CancelReasons => Set<CancelReason>();
        public DbSet<StatusChange> StatusChanges => Set<StatusChange>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration for Shop entity relationships and properties
            modelBuilder.Entity<Shop>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Rating).IsRequired().HasColumnType("decimal(2, 1)").HasDefaultValue(0);
                entity.Property(e => e.UserId).IsRequired();
                entity.HasOne(e => e.User)
                    .WithOne(u => u.Shop)
                    .HasForeignKey<Shop>(e => e.UserId);
            });

            // Configuration for Order entity relationships and properties
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalPrice).IsRequired().HasColumnType("decimal(18, 2)");
                entity.Property(e => e.OrderDate).IsRequired();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Address).IsRequired().HasMaxLength(200);
                entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(15);
                entity.Property(e => e.Note).HasMaxLength(500);
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Orders)
                    .HasForeignKey(e => e.UserId);
                entity.HasOne(e => e.CancelReason)
                    .WithMany(cr => cr.Orders)
                    .HasForeignKey(e => e.CancelReasonId);
            });

            //Quan he giua cart va user
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithOne(u => u.Cart)
                .HasForeignKey<Cart>(e => e.UserId);

            //Quan he giua cart va cartItem
            modelBuilder.Entity<Cart>()
                .HasMany(c => c.CartItems)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .HasConstraintName("FK_Cart_CartItem_2645B050")
                .OnDelete(DeleteBehavior.NoAction);


            // Quan hệ giữa CartItem và ProductItem 
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.ProductItem)
                .WithMany(pi => pi.CartItem)
                .HasForeignKey(ci => ci.ProductItemId)
                .OnDelete(DeleteBehavior.NoAction)
                ;


            //Primary Key cua ProductConfiguration
            modelBuilder.Entity<ProductConfiguration>().HasKey(e => e.ProductItemId);
            modelBuilder.Entity<ProductConfiguration>().HasKey(e => e.VariationOptionId);


            // Quan hệ giữa ProductConfiguration và ProductItem (1-N)
            modelBuilder.Entity<ProductConfiguration>()
                .HasOne(pc => pc.ProductItem)
                .WithMany(pi => pi.ProductConfiguration)
                .HasForeignKey(pc => pc.ProductItemId);

            // Quan hệ giữa ProductConfiguration và VariationOption (1-N)
            modelBuilder.Entity<ProductConfiguration>()
                .HasOne(pc => pc.VariationOption)
                .WithMany(vo => vo.ProductConfiguration)
                .HasForeignKey(pc => pc.VariationOptionId);

            // ProductItem Configuration
            modelBuilder.Entity<ProductItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductId)
                      .IsRequired();

                entity.HasOne(e => e.Product)
                    .WithMany(p => p.ProductItems)
                    .HasForeignKey(e => e.ProductId);

                entity.Property(e => e.QuantityInStock)
                      .IsRequired();

                entity.Property(e => e.Price)
                      .IsRequired();
            });

            // Category Configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name)
                      .IsRequired();

                entity.Property(e => e.Description)
                      .HasMaxLength(500);

                entity.Property(e => e.PromotionId)
                      .IsRequired(false);

                entity.HasMany(e => e.Variations)
                      .WithOne(v => v.Category)
                      .HasForeignKey(v => v.CategoryId);

                entity.HasOne(c => c.Promotion)
                      .WithMany(p => p.Categories)
                      .HasForeignKey(c => c.PromotionId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // Variation Configuration
            modelBuilder.Entity<Variation>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(v => v.Category)
                    .WithMany(c => c.Variations)
                    .HasForeignKey(v => v.CategoryId);

                entity.HasMany(vc => vc.VariationOptions)
                      .WithOne(v => v.Variation)
                      .HasForeignKey(v => v.VariationId);

                entity.Property(v => v.Name)
                    .HasMaxLength(150)
                    .IsRequired();

            });

            // Variation Option Configuration
            modelBuilder.Entity<VariationOption>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(vo => vo.Value)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.HasOne(vo => vo.Variation)
                    .WithMany(v => v.VariationOptions)
                    .HasForeignKey(v => v.VariationId);
            });

            // Promotion
            modelBuilder.Entity<Promotion>()
                .HasKey(p => p.Id);
            modelBuilder.Entity<Promotion>()
                .Property(p => p.Description)
                .HasMaxLength(500);
            modelBuilder.Entity<Promotion>()
                .HasMany(p => p.Categories)
                .WithOne(c => c.Promotion)
                .HasForeignKey(c => c.PromotionId)
                .OnDelete(DeleteBehavior.NoAction)
                ;

            // OrderDetail  
            modelBuilder.Entity<OrderDetail>()
                .HasKey(od => od.Id);
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.ProductItem)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.ProductItemId)
                .OnDelete(DeleteBehavior.NoAction);
            //....

            // Configurations for StatusChange
            modelBuilder.Entity<StatusChange>(entity =>
            {
                // Primary key
                entity.HasKey(e => e.Id);

                // Attributes
                entity.Property(e => e.Status)
                      .IsRequired()
                      .HasMaxLength(30);

                entity.Property(e => e.ChangeTime)
                      .IsRequired();

                // Many-to-one relationship with Order
                entity.HasOne(e => e.Order)
                    .WithMany(o => o.StatusChanges)
                    .HasForeignKey(e => e.OrderId);

            });

            // Configurations for CancelReason
            modelBuilder.Entity<CancelReason>(entity =>
            {
                // Primary key
                entity.HasKey(e => e.Id);
                // Attribute
                entity.Property(e => e.Description)
                      .HasMaxLength(150);
                entity.Property(e => e.RefundRate)
                      .IsRequired();
                // One-to-many relationship with Order
                entity.HasMany(cr => cr.Orders)
                    .WithOne(o => o.CancelReason)
                    .HasForeignKey(o => o.CancelReasonId);
            });
            // Payment Entity Configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderId)
                      .IsRequired();
                entity.Property(e => e.ExpirationDate)
                      .IsRequired();
                entity.Property(e => e.TotalAmount)
                      .IsRequired()
                      .HasColumnType("decimal(18, 2)");
                entity.Property(e => e.Status)
                      .IsRequired()
                      .HasMaxLength(20);
                entity.HasOne(e => e.Order)
                      .WithOne(o => o.Payment)
                      .HasForeignKey<Payment>(e => e.OrderId);
                entity.HasMany(e => e.PaymentDetails)
                      .WithOne(pd => pd.Payment)
                      .HasForeignKey(pd => pd.PaymentId);
            });

            // PaymentDetail Entity Configuration
            modelBuilder.Entity<PaymentDetail>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.PaymentId)
                      .IsRequired();

                entity.Property(e => e.Status)
                      .IsRequired()
                      .HasMaxLength(15);

                entity.Property(e => e.Method)
                      .IsRequired()
                      .HasMaxLength(30);

                entity.Property(e => e.ExternalTransaction)
                      .HasMaxLength(100);
            });

            // Configuration for Review entity
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired(false).HasMaxLength(1000);
                entity.Property(e => e.Rating).IsRequired(false);
                entity.Property(e => e.Date).IsRequired(false).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.ProductId).IsRequired();
                entity.Property(e => e.UserId).IsRequired();

                entity.HasOne(e => e.Product)
                    .WithMany(p => p.Reviews)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuration for Reply entity
            modelBuilder.Entity<Reply>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired(false).HasMaxLength(1000);
                entity.Property(e => e.Date).IsRequired(false).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.ReviewId).IsRequired();
                entity.Property(e => e.ShopId).IsRequired();

                entity.HasOne(e => e.Review)
                    .WithOne(r => r.Reply)
                    .HasForeignKey<Reply>(e => e.ReviewId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Shop)
                    .WithMany()
                    .HasForeignKey(e => e.ShopId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuration for Product entity
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.CategoryId).IsRequired();
                entity.Property(e => e.ShopId).IsRequired();
                entity.Property(e => e.Rating).HasColumnType("decimal(2, 1)").HasDefaultValue(0);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SoldCount).IsRequired().HasDefaultValue(0);

                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Shop)
                    .WithMany(s => s.Products)
                    .HasForeignKey(e => e.ShopId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

    }
}
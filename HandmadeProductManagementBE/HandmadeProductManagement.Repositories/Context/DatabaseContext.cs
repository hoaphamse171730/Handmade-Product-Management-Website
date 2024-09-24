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
        public virtual DbSet<Promotion> Promotions => Set<Promotion>();
        public virtual DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
        public DbSet<Variation> Variations => Set<Variation>();
        public DbSet<VariationOption> VariationOptions => Set<VariationOption>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<Reply> Replies => Set<Reply>();
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

            // ProductItem Configuration
            modelBuilder.Entity<ProductItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductId)
                      .IsRequired();

                entity.HasOne(e => e.Product)
                      .WithMany(p => p.ProductItems)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

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

                entity.HasMany(e => e.Variations)
                      .WithOne(v => v.Category)
                      .HasForeignKey(v => v.CategoryId)
                      .OnDelete(DeleteBehavior.Cascade);

            });

            // Variation Configuration
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

            // Variation Option Configuration
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
            
            // Promotion
            modelBuilder.Entity<Promotion>()  
                .HasKey(p => p.PromotionId);
            modelBuilder.Entity<Promotion>()  
                .Property(p => p.PromotionName)  
                .IsRequired() 
                .HasMaxLength(255);
            modelBuilder.Entity<Promotion>()  
                .Property(p => p.Description)  
                .HasMaxLength(500);
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
            //....

            // Configurations for StatusChange
            modelBuilder.Entity<StatusChange>(entity =>
            {
                // Primary key
                entity.HasKey(e => e.Id);

                // Attributes
                entity.Property(e => e.Status)
                      .IsRequired()
                      .HasMaxLength(15); 

                entity.Property(e => e.ChangeTime)
                      .IsRequired();

                // Many-to-one relationship with Order
                entity.HasOne(e => e.Order)
                      .WithMany(o => o.StatusChanges) 
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
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
                      .WithOne(o => o.CancelReason)  // Each Order has one CancelReason
                      .HasForeignKey(o => o.CancelReasonId)
                      .OnDelete(DeleteBehavior.Cascade);
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

            // Configure Review entity
            modelBuilder.Entity<Review>(entity =>
            {
                entity.Property(r => r.Content)
                      .HasColumnType("text") 
                      .IsRequired(false);

                entity.Property(r => r.Rating)
                      .IsRequired();

                entity.Property(r => r.Date)
                      .HasDefaultValueSql("GETDATE()");

                entity.HasOne(r => r.Product)
                      .WithMany(p => p.Reviews)
                      .HasForeignKey(r => r.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                // One-to-many: Review belongs to ApplicationUser
                entity.HasOne(r => r.User)
                      .WithMany(u => u.Reviews)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // One-to-one: Review has one Reply
                entity.HasOne(r => r.Reply)
                      .WithOne(re => re.Review)
                      .HasForeignKey<Reply>(re => re.ReviewId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Reply entity
            modelBuilder.Entity<Reply>(entity =>
            {
                entity.Property(rp => rp.Content)
                      .HasColumnType("text")
                      .IsRequired(false);

                entity.Property(rp => rp.Date)
                      .HasDefaultValueSql("GETDATE()");

                entity.HasOne(rp => rp.Shop)
                      .WithMany()
                      .HasForeignKey(rp => rp.ShopId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

    }
}
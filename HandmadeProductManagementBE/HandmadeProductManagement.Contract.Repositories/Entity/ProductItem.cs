using HandmadeProductManagement.Core.Base;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class ProductItem : BaseEntity
    {
        // Foreign Key Property

        public int QuantityInStock { get; set; }
        public int Price { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public Product Product { get; set; }
        public virtual ICollection<CartItem> CartItem { get; set; }
        public virtual ICollection<ProductConfiguration> ProductConfiguration { get; set; } 
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}

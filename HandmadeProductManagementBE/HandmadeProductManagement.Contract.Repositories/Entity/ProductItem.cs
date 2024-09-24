using HandmadeProductManagement.Core.Base;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class ProductItem : BaseEntity
    {
        // Foreign Key Property
        public string ProductId { get; set; } = string.Empty;
        public Product Product { get; set; } = new Product();

        public int QuantityInStock { get; set; }
        public int Price { get; set; }

        public ICollection<CartItem> CartItem { get; set; } = new List<CartItem>();

        public ICollection<ProductConfiguration> ProductConfiguration { get; set; } = new List<ProductConfiguration>();
    }
}

using HandmadeProductManagement.Core.Base;
using System.Text.Json.Serialization;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string CategoryId { get; set; } = string.Empty;
        public string ShopId { get; set; } = string.Empty ;
        public decimal Rating { get; set; } 
        public string Status { get; set; } = string.Empty;  
        public int SoldCount { get; set; }
        [JsonIgnore]
        public ICollection<ProductImage> ProductImages { get; set; } = [];
        [JsonIgnore]
        public ICollection<ProductItem> ProductItems { get; set; } = [];
        [JsonIgnore]
        public Category? Category { get; set; }
        [JsonIgnore]
        public Shop? Shop { get; set; }
        [JsonIgnore]
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = [];
        [JsonIgnore]
        public virtual ICollection<Review> Reviews { get; set; } = [];
    }
}

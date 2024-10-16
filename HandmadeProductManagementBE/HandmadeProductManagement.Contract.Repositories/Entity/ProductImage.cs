using HandmadeProductManagement.Core.Base;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class ProductImage : BaseEntity
    {
        public string Url { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public Product? Product { get; set; }
    }
}

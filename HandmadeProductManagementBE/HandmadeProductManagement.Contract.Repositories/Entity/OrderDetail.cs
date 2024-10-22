using HandmadeProductManagement.Core.Base;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class OrderDetail : BaseEntity
    {
        public string ProductItemId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public int ProductQuantity { get; set; }
        public decimal DiscountPrice { get; set; }

        public virtual Order? Order { get; set; }
        public virtual ProductItem? ProductItem { get; set; }

    }
}
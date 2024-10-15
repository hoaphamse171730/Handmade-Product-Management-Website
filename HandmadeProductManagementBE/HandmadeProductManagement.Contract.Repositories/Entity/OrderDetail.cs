using HandmadeProductManagement.Core.Base;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class OrderDetail : BaseEntity
    {
        public string ProductItemId { get; set; }
        public string OrderId { get; set; }
        public int ProductQuantity { get; set; }
        public decimal DiscountPrice { get; set; }

        public virtual Order? Order { get; set; }
        public virtual ProductItem? ProductItem { get; set; }

    }
}
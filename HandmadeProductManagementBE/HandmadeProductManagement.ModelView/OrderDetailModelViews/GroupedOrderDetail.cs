using HandmadeProductManagement.Contract.Repositories.Entity;

namespace HandmadeProductManagement.ModelViews.OrderDetailModelViews
{
    public class GroupedOrderDetail
    {
        public string ShopId { get; set; } = string.Empty;
        public CartItem CartItem { get; set; }
        public ProductItem ProductItem { get; set; }
        public decimal DiscountPrice { get; set; }
    }
}

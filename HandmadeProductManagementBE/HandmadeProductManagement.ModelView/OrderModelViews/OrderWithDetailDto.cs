using HandmadeProductManagement.ModelViews.OrderDetailModelViews;

namespace HandmadeProductManagement.ModelViews.OrderModelViews
{
    public class OrderWithDetailDto : OrderResponseModel
    {
        public string CustomerId { get; set; }
        public string? ShopId { get; set; }
        public string? ShopName { get; set; }
        public List<OrderInDetailDto> OrderDetails { get; set; } = [];
    }
}

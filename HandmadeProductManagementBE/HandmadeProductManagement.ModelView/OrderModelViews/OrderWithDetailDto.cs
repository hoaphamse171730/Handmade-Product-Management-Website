using HandmadeProductManagement.ModelViews.OrderDetailModelViews;

namespace HandmadeProductManagement.ModelViews.OrderModelViews
{
    public class OrderWithDetailDto : OrderResponseModel
    {
        public string? ShopName { get; set; }
        public List<OrderInDetailDto> OrderDetails { get; set; } = [];
    }
}

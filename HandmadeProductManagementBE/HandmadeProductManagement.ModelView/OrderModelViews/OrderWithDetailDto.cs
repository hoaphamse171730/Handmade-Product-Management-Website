using HandmadeProductManagement.ModelViews.OrderDetailModelViews;

namespace HandmadeProductManagement.ModelViews.OrderModelViews
{
    public class OrderWithDetailDto : OrderResponseModel
    {
        public List<OrderInDetailDto> OrderDetails { get; set; } = [];
    }
}

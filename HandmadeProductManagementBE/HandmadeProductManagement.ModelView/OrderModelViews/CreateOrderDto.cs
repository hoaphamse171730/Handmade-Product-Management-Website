using HandmadeProductManagement.ModelViews.OrderDetailModelViews;

namespace HandmadeProductManagement.ModelViews.OrderModelViews
{
    public class CreateOrderDto
    {
        public string Address { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Note { get; set; }
    }
}

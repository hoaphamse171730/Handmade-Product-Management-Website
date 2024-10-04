using HandmadeProductManagement.ModelViews.OrderDetailModelViews;

namespace HandmadeProductManagement.ModelViews.OrderModelViews
{
    public class CreateOrderDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string? CancelReasonId { get; set; }
        public List<OrderDetailForCreationDto> OrderDetails { get; set; } = new List<OrderDetailForCreationDto>();
    }
}

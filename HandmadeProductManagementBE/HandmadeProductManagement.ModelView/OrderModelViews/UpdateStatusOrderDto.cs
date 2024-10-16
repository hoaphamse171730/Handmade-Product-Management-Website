namespace HandmadeProductManagement.ModelViews.OrderModelViews
{
    public class UpdateStatusOrderDto
    {
        public string OrderId { get; set; } = string.Empty;
        public string Status {get; set; } = string.Empty;
        public string? CancelReasonId {get; set; }
    }
}

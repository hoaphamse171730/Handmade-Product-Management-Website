namespace HandmadeProductManagement.ModelViews.OrderModelViews
{
    public class OrderByUserDto
    {
        public string Id { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? Phone { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string? CancelReasonId { get; set; }
    }
}

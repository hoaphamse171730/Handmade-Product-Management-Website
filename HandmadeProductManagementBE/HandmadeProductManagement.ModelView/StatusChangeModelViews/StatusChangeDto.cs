namespace HandmadeProductManagement.ModelViews.StatusChangeModelViews
{
    public class StatusChangeDto
    {
        public required string Id { get; set; }
        public required DateTime ChangeTime { get; set; }
        public required string Status { get; set; }
        public required string OrderId { get; set; }
    }
}

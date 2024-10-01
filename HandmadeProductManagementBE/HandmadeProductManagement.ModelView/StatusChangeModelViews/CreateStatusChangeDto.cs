namespace HandmadeProductManagement.ModelViews.StatusChangeModelViews
{
    public class CreateStatusChangeDto
    {
        public required DateTime ChangeTime { get; set; }
        public required string Status { get; set; }
        public required string OrderId { get; set; }
    }
}

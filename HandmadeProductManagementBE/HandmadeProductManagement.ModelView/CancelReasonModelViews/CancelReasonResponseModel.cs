namespace HandmadeProductManagement.ModelViews.CancelReasonModelViews
{
    public class CancelReasonResponseModel
    {
        public string Id { get; set; } = string.Empty;
        public required string Description { get; set; }
        public required float RefundRate { get; set; }
    }
}

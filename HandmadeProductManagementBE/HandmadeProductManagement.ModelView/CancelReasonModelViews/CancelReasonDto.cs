namespace HandmadeProductManagement.ModelViews.CancelReasonModelViews
{
    public class CancelReasonDto
    {
        public string Id { get; set; } = string.Empty;
        public required string Description { get; set; }
        public required decimal RefundRate { get; set; }
    }
}

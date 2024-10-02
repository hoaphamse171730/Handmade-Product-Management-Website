namespace HandmadeProductManagement.ModelViews.CancelReasonModelViews
{
    public class CreateCancelReasonDto
    {
        public required string Description { get; set; }
        public required decimal RefundRate { get; set; }
    }
}

namespace HandmadeProductManagement.ModelViews.PaymentDetailModelViews
{
    public class CreatePaymentDetailDto
    {
        public string PaymentId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string? ExternalTransaction { get; set; }
    }
}

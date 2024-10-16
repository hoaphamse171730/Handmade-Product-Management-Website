using HandmadeProductManagement.ModelViews.PaymentDetailModelViews;

namespace HandmadeProductManagement.ModelViews.PaymentModelViews
{
    public class PaymentResponseModel
    {
        public string Id { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public DateTime? ExpirationDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<PaymentDetailResponseModel> PaymentDetails { get; set; } = new List<PaymentDetailResponseModel>();

    }
}

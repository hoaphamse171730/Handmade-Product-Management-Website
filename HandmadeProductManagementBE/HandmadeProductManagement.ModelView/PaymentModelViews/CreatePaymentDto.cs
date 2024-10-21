namespace HandmadeProductManagement.ModelViews.PaymentModelViews
{
    public class CreatePaymentDto
    {
        public string OrderId { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }
}

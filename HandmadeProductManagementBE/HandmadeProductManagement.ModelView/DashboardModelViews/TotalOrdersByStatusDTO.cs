namespace HandmadeProductManagement.ModelViews.DashboardModelViews
{
    public class TotalOrdersByStatusDTO
    {
        public int Pending { get; set; }
        public int Cancelled { get; set; }
        public int AwaitingPayment { get; set; }
        public int PaymentFailed { get; set; }
        public int Processing { get; set; }
        public int Delivering { get; set; }
        public int Shipped { get; set; }
        public int DeliveryFailed { get; set; }
        public int OnHold { get; set; }
        public int DeliveringRetry { get; set; }
        public int RefundRequested { get; set; }
        public int RefundApproved { get; set; }
        public int RefundDenied { get; set; }
        public int Returning { get; set; }
        public int ReturnFailed { get; set; }
        public int Returned { get; set; }
        public int Refunded { get; set; }
        public int RefundCancelled { get; set; }
        public int Closed { get; set; }
    }
}

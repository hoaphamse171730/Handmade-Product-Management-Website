using HandmadeProductManagement.Core.Base;


namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class PaymentDetail : BaseEntity
    {
        public string PaymentId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string? ExternalTransaction { get; set; }

        public Payment? Payment { get; set; }
    }
}
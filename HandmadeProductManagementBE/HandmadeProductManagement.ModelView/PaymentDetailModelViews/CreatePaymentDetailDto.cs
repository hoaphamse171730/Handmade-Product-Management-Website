using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.ModelViews.PaymentDetailModelViews
{
    public class CreatePaymentDetailDto
    {
        public string PaymentId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string? ExternalTransaction { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}

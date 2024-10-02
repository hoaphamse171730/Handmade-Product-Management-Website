using HandmadeProductManagement.ModelViews.PaymentModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IPaymentService
    {
        Task<bool> CreatePaymentAsync(CreatePaymentDto createPaymentDto);
        Task<bool> UpdatePaymentStatusAsync(string paymentId, string status);
        Task<PaymentResponseModel> GetPaymentByOrderIdAsync(string orderId);
        Task CheckAndExpirePaymentsAsync();
    }
}

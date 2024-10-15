using HandmadeProductManagement.ModelViews.PaymentModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IPaymentService
    {
        Task<bool> CreatePaymentAsync(string userId, string orderId);
        Task<bool> UpdatePaymentStatusAsync(string paymentId, string status, string userId);
        Task<PaymentResponseModel> GetPaymentByOrderIdAsync(string orderId);
        Task CheckAndExpirePaymentsAsync();
    }
}

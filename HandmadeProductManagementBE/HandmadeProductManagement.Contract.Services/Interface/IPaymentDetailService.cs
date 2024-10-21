using HandmadeProductManagement.ModelViews.PaymentDetailModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IPaymentDetailService
    {
        Task<bool> CreatePaymentDetailAsync(string userId, CreatePaymentDetailDto createPaymentDetailDto);
    }
}

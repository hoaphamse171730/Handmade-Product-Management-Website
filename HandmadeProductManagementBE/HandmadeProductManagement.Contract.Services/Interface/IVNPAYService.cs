using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.ModelViews.VariationModelViews;
using HandmadeProductManagement.ModelViews.VNPayModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IVNPayService
    {
        Task<string> GetTransactionStatusVNPay(string orderId, Guid userId, String urlReturn);

        Task<VNPAYResponse> VNPAYPayment(VNPAYRequest request);

    }
}

using HandmadeProductManagement.ModelViews.PaymentDetailModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IPaymentDetailService
    {
        Task<PaymentDetailResponseModel> CreatePaymentDetailAsync(CreatePaymentDetailDto createPaymentDetailDto);
        Task<PaymentDetailResponseModel> GetPaymentDetailByPaymentIdAsync(string paymentId);
        Task<PaymentDetailResponseModel> GetPaymentDetailByIdAsync(string id);
    }
}

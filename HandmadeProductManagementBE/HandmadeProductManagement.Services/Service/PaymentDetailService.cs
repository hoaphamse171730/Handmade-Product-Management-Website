using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.PaymentDetailModelViews;
using HandmadeProductManagement.ModelViews.PaymentModelViews;
using HandmadeProductManagement.Repositories.Entity;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagement.Services.Service
{
    public class PaymentDetailService : IPaymentDetailService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaymentDetailService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PaymentDetailResponseModel> CreatePaymentDetailAsync(CreatePaymentDetailDto createPaymentDetailDto)
        {
            var userRepository = _unitOfWork.GetRepository<ApplicationUser>();
            var userExists = await userRepository.Entities.AnyAsync(u => u.Id == createPaymentDetailDto.UserId);
            if (!userExists)
            {
                throw new BaseException.ErrorException(404, "user_not_found", "User not found.");
            }

            var paymentRepository = _unitOfWork.GetRepository<Payment>();
            var payment = await paymentRepository.Entities.FirstOrDefaultAsync(p => p.Id == createPaymentDetailDto.PaymentId);

            if (payment == null)
            {
                throw new BaseException.ErrorException(404, "payment_not_found", "Payment not found.");
            }

            if (payment.ExpirationDate < DateTime.UtcNow)
            {
                throw new BaseException.BadRequestException("payment_expired", "Payment has expired.");
            }

            if (!decimal.TryParse(createPaymentDetailDto.Amount.ToString(), out decimal amount) || amount <= 0)
            {
                throw new BaseException.BadRequestException("invalid_amount", "Amount must be a positive number.");
            }

            var paymentDetailRepository = _unitOfWork.GetRepository<PaymentDetail>();

            var paymentDetail = new PaymentDetail
            {
                PaymentId = createPaymentDetailDto.PaymentId,
                Status = createPaymentDetailDto.Status,
                Amount = createPaymentDetailDto.Amount,
                Method = createPaymentDetailDto.Method,
                ExternalTransaction = createPaymentDetailDto.ExternalTransaction
            };

            payment.CreatedBy = createPaymentDetailDto.UserId.ToString();
            payment.CreatedTime = DateTime.UtcNow;
            payment.LastUpdatedBy = createPaymentDetailDto.UserId.ToString();
            payment.LastUpdatedTime = DateTime.UtcNow;
            await paymentDetailRepository.InsertAsync(paymentDetail);
            await _unitOfWork.SaveAsync();

            if (createPaymentDetailDto.Status == "Success")
            {
                payment.Status = "Completed";
                payment.LastUpdatedTime = DateTime.UtcNow;
                paymentRepository.Update(payment);
                await _unitOfWork.SaveAsync();
            }

            return new PaymentDetailResponseModel
            {
                Id = paymentDetail.Id,
                PaymentId = paymentDetail.PaymentId,
                Status = paymentDetail.Status,
                Amount = paymentDetail.Amount,
                Method = paymentDetail.Method,
                ExternalTransaction = paymentDetail.ExternalTransaction
            };
        }

        public async Task<PaymentDetailResponseModel> GetPaymentDetailByPaymentIdAsync(string paymentId)
        {
            var paymentRepository = _unitOfWork.GetRepository<Payment>();
            var paymentExists = await paymentRepository.Entities.AnyAsync(p => p.Id == paymentId);

            if (!paymentExists)
            {
                throw new BaseException.ErrorException(404, "payment_not_found", "Payment not found.");
            }

            var paymentDetailRepository = _unitOfWork.GetRepository<PaymentDetail>();
            var paymentDetail = await paymentDetailRepository.Entities.FirstOrDefaultAsync(pd => pd.PaymentId == paymentId);

            if (paymentDetail == null)
            {
                throw new BaseException.ErrorException(404, "payment_detail_not_found", "Payment detail not found.");
            }

            return new PaymentDetailResponseModel
            {
                Id = paymentDetail.Id,
                PaymentId = paymentDetail.PaymentId,
                Status = paymentDetail.Status,
                Amount = paymentDetail.Amount,
                Method = paymentDetail.Method,
                ExternalTransaction = paymentDetail.ExternalTransaction
            };
        }

        public async Task<PaymentDetailResponseModel> GetPaymentDetailByIdAsync(string id)
        {
            var paymentDetailRepository = _unitOfWork.GetRepository<PaymentDetail>();
            var paymentDetail = await paymentDetailRepository.Entities.FirstOrDefaultAsync(pd => pd.Id == id);

            if (paymentDetail == null)
            {
                throw new BaseException.ErrorException(404, "payment_detail_not_found", "Payment detail not found.");
            }

            return new PaymentDetailResponseModel
            {
                Id = paymentDetail.Id,
                PaymentId = paymentDetail.PaymentId,
                Status = paymentDetail.Status,
                Amount = paymentDetail.Amount,
                Method = paymentDetail.Method,
                ExternalTransaction = paymentDetail.ExternalTransaction
            };
        }
    }
}
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.PaymentDetailModelViews;
using HandmadeProductManagement.Repositories.Entity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace HandmadeProductManagement.Services.Service
{
    public class PaymentDetailService : IPaymentDetailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentService _paymentService;

        public PaymentDetailService(IUnitOfWork unitOfWork, IPaymentService paymentService)
        {
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
        }

        public async Task<bool> CreatePaymentDetailAsync(string userId, CreatePaymentDetailDto createPaymentDetailDto)
        {
            ValidatePaymentDetail(createPaymentDetailDto);
            var userRepository = _unitOfWork.GetRepository<ApplicationUser>();
            var userExists = await userRepository.Entities.AnyAsync(u => u.Id.ToString() == userId && !u.DeletedTime.HasValue);
            if (!userExists)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageUserNotFound);
            }

            var paymentRepository = _unitOfWork.GetRepository<Payment>();
            var payment = await paymentRepository.Entities
                        .FirstOrDefaultAsync(p => p.Id == createPaymentDetailDto.PaymentId && !p.DeletedTime.HasValue)
                        ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessagePaymentNotFound);

            var invalidStatuses = new[] { Constants.PaymentStatusCompleted, Constants.PaymentStatusExpired, Constants.PaymentStatusRefunded };
            if (invalidStatuses.Contains(payment.Status))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                    string.Format(Constants.ErrorMessageInvalidPaymentStatus, payment.Status));
            }

            var paymentDetailRepository = _unitOfWork.GetRepository<PaymentDetail>();

            var paymentDetail = new PaymentDetail
            {
                PaymentId = createPaymentDetailDto.PaymentId,
                Status = createPaymentDetailDto.Status,
                Method = createPaymentDetailDto.Method,
                ExternalTransaction = createPaymentDetailDto.ExternalTransaction
            };

            paymentDetail.CreatedBy = userId;
            paymentDetail.LastUpdatedBy = userId;
            await paymentDetailRepository.InsertAsync(paymentDetail);
            await _unitOfWork.SaveAsync();

            if (createPaymentDetailDto.Status == Constants.PaymentDetailStatusSuccess)
            {
                await _paymentService.UpdatePaymentStatusAsync(createPaymentDetailDto.PaymentId, Constants.PaymentStatusCompleted, userId);
            }

            return true;
        }

        private void ValidatePaymentDetail(CreatePaymentDetailDto createPaymentDetailDto)
        {
            if (string.IsNullOrWhiteSpace(createPaymentDetailDto.PaymentId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPaymentId);
            }

            if (!Guid.TryParse(createPaymentDetailDto.PaymentId, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            if (string.IsNullOrWhiteSpace(createPaymentDetailDto.Status))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessagePleaseInputStatus);
            }

            if (createPaymentDetailDto.Status != Constants.PaymentDetailStatusSuccess && createPaymentDetailDto.Status != Constants.PaymentDetailStatusFailed)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidStatusFormat);
            }

            if (string.IsNullOrWhiteSpace(createPaymentDetailDto.Method))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessagePleaseInputMethod);
            }

            if (Regex.IsMatch(createPaymentDetailDto.Method, @"[^a-zA-Z\s]"))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidMethodFormat);
            }

            if (!string.IsNullOrWhiteSpace(createPaymentDetailDto.ExternalTransaction) && !Regex.IsMatch(createPaymentDetailDto.ExternalTransaction, @"^[a-zA-Z0-9]+$"))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidExternalTransactionFormat);
            }
        }
    }
}
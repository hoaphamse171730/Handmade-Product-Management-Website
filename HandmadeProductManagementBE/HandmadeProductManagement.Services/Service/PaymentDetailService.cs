﻿using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.PaymentDetailModelViews;
using HandmadeProductManagement.ModelViews.PaymentModelViews;
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
                throw new BaseException.NotFoundException("user_not_found", "User not found.");
            }

            var paymentRepository = _unitOfWork.GetRepository<Payment>();
            var payment = await paymentRepository.Entities
                        .FirstOrDefaultAsync(p => p.Id == createPaymentDetailDto.PaymentId && !p.DeletedTime.HasValue);
            if (payment == null)
            {
                throw new BaseException.NotFoundException("payment_not_found", "Payment not found.");
            }

            var invalidStatuses = new[] { "Completed", "Expired", "Refunded"};
            if (invalidStatuses.Contains(payment.Status))
            {
                throw new BaseException.BadRequestException("invalid_payment_status", $"Cannot create payment detail for payment with status '{payment.Status}'.");
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

            if (createPaymentDetailDto.Status == "Success")
            {
                await _paymentService.UpdatePaymentStatusAsync(createPaymentDetailDto.PaymentId, "Completed", userId);
            }

            return true;
        }
        private void ValidatePaymentDetail(CreatePaymentDetailDto createPaymentDetailDto)
        {
            if (string.IsNullOrWhiteSpace(createPaymentDetailDto.PaymentId))
            {
                throw new BaseException.BadRequestException("invalid_payment_id", "Please input payment id.");
            }

            if (!Guid.TryParse(createPaymentDetailDto.PaymentId, out _))
            {
                throw new BaseException.BadRequestException("invalid_payment_id_format", "Payment ID format is invalid. Example: 123e4567-e89b-12d3-a456-426614174000.");
            }

            if (string.IsNullOrWhiteSpace(createPaymentDetailDto.Status))
            {
                throw new BaseException.BadRequestException("invalid_status", "Please input status.");
            }

            if (createPaymentDetailDto.Status != "Success" && createPaymentDetailDto.Status != "Failed")
            {
                throw new BaseException.BadRequestException("invalid_status_format", "Status must be Success or Failed.");
            }

            if (string.IsNullOrWhiteSpace(createPaymentDetailDto.Method))
            {
                throw new BaseException.BadRequestException("invalid_method", "Please input method.");
            }

            if (Regex.IsMatch(createPaymentDetailDto.Method, @"[^a-zA-Z\s]"))
            {
                throw new BaseException.BadRequestException("invalid_method_format", "Method cannot contain numbers or special characters.");
            }

            if (!string.IsNullOrWhiteSpace(createPaymentDetailDto.ExternalTransaction) && !Regex.IsMatch(createPaymentDetailDto.ExternalTransaction, @"^[a-zA-Z0-9]+$"))
            {
                throw new BaseException.BadRequestException("invalid_external_transaction_format", "External transaction can only contain alphanumeric characters.");
            }
        }
    }
}
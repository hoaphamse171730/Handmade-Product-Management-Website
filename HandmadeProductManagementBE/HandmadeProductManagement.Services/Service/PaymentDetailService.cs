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

        public PaymentDetailService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> CreatePaymentDetailAsync(CreatePaymentDetailDto createPaymentDetailDto)
        {
            ValidatePaymentDetail(createPaymentDetailDto);

            var userRepository = _unitOfWork.GetRepository<ApplicationUser>();
            var userExists = await userRepository.Entities.AnyAsync(u => u.Id.ToString() == createPaymentDetailDto.UserId && !u.DeletedTime.HasValue && u.DeletedBy == null);
            if (!userExists)
            {
                throw new BaseException.NotFoundException("user_not_found", "User not found.");
            }

            var paymentRepository = _unitOfWork.GetRepository<Payment>();
            var payment = await paymentRepository.Entities
                        .FirstOrDefaultAsync(p => p.Id == createPaymentDetailDto.PaymentId && !p.DeletedTime.HasValue && p.DeletedBy == null);
            if (payment == null)
            {
                throw new BaseException.NotFoundException("payment_not_found", "Payment not found.");
            }

            var invalidStatuses = new[] { "Completed", "Expired", "Refunded", "Failed", "Closed" };
            if (invalidStatuses.Contains(payment.Status))
            {
                throw new BaseException.BadRequestException("invalid_payment_status", $"Cannot create payment detail for payment with status '{payment.Status}'.");
            }

            var paymentDetailRepository = _unitOfWork.GetRepository<PaymentDetail>();

            var paymentDetail = new PaymentDetail
            {
                PaymentId = createPaymentDetailDto.PaymentId,
                Status = createPaymentDetailDto.Status,
                Amount = payment.TotalAmount,
                Method = createPaymentDetailDto.Method,
                ExternalTransaction = createPaymentDetailDto.ExternalTransaction
            };

            await paymentDetailRepository.InsertAsync(paymentDetail);
            await _unitOfWork.SaveAsync();

            if (createPaymentDetailDto.Status == "Success")
            {
                payment.Status = "Completed";
                payment.LastUpdatedTime = DateTime.UtcNow;
                paymentRepository.Update(payment);

                var orderRepository = _unitOfWork.GetRepository<Order>();
                var order = await orderRepository.Entities
                                .FirstOrDefaultAsync(o => o.Id == payment.OrderId && !o.DeletedTime.HasValue && o.DeletedBy == null);
                if (order != null)
                {
                    order.Status = "Processing";
                    order.LastUpdatedTime = DateTime.UtcNow;
                    orderRepository.Update(order);
                }

                await _unitOfWork.SaveAsync();
            }

            return true;
        }

        public async Task<PaymentDetailResponseModel> GetPaymentDetailByPaymentIdAsync(string paymentId)
        {
            if (string.IsNullOrWhiteSpace(paymentId))
            {
                throw new BaseException.BadRequestException("invalid_payment_id", "Please input payment id.");
            }

            if (!Guid.TryParse(paymentId, out _))
            {
                throw new BaseException.BadRequestException("invalid_payment_id_format", "Payment ID format is invalid. Example: 123e4567-e89b-12d3-a456-426614174000.");
            }

            var paymentRepository = _unitOfWork.GetRepository<Payment>();
            var paymentExists = await paymentRepository.Entities
                        .AnyAsync(p => p.Id == paymentId && !p.DeletedTime.HasValue && p.DeletedBy == null);

            if (!paymentExists)
            {
                throw new BaseException.NotFoundException("payment_not_found", "Payment not found.");
            }

            var paymentDetailRepository = _unitOfWork.GetRepository<PaymentDetail>();
            var paymentDetail = await paymentDetailRepository.Entities
                        .FirstOrDefaultAsync(pd => pd.PaymentId == paymentId && !pd.DeletedTime.HasValue && pd.DeletedBy == null);
            if (paymentDetail == null)
            {
                throw new BaseException.NotFoundException("payment_detail_not_found", "Payment detail not found.");
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
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new BaseException.BadRequestException("invalid_id", "Please input id.");
            }

            if (!Guid.TryParse(id, out _))
            {
                throw new BaseException.BadRequestException("invalid_id_format", "ID format is invalid. Example: 123e4567-e89b-12d3-a456-426614174000.");
            }

            var paymentDetailRepository = _unitOfWork.GetRepository<PaymentDetail>();
            var paymentDetail = await paymentDetailRepository.Entities
                        .FirstOrDefaultAsync(pd => pd.Id == id && !pd.DeletedTime.HasValue && pd.DeletedBy == null);
            if (paymentDetail == null)
            {
                throw new BaseException.NotFoundException("payment_detail_not_found", "Payment detail not found.");
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

            if (string.IsNullOrWhiteSpace(createPaymentDetailDto.UserId))
            {
                throw new BaseException.BadRequestException("invalid_user_id", "Please input user id.");
            }

            if (!Guid.TryParse(createPaymentDetailDto.UserId, out _))
            {
                throw new BaseException.BadRequestException("invalid_user_id_format", "User ID format is invalid. Example: 123e4567-e89b-12d3-a456-426614174000.");
            }

            if (string.IsNullOrWhiteSpace(createPaymentDetailDto.Status))
            {
                throw new BaseException.BadRequestException("invalid_status", "Please input status.");
            }

            if (Regex.IsMatch(createPaymentDetailDto.Status, @"[^a-zA-Z\s]"))
            {
                throw new BaseException.BadRequestException("invalid_status_format", "Status cannot contain numbers or special characters.");
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
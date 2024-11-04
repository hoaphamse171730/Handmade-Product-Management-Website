﻿using Google.Apis.Storage.v1.Data;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.CancelReasonModelViews;
using HandmadeProductManagement.ModelViews.OrderModelViews;
using HandmadeProductManagement.ModelViews.PaymentDetailModelViews;
using HandmadeProductManagement.ModelViews.PaymentModelViews;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace HandmadeProductManagement.Services.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Lazy<IOrderService> _orderService;
        private readonly ICancelReasonService _cancelReasonService;

        public PaymentService(
            IUnitOfWork unitOfWork,
            Lazy<IOrderService> orderService,
            ICancelReasonService cancelReasonService)
        {
            _unitOfWork = unitOfWork;
            _orderService = orderService;
            _cancelReasonService = cancelReasonService;
        }

        private IOrderService OrderService => _orderService.Value;


        public async Task<bool> CreatePaymentOnlineAsync(string userId, string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageEmptyId);
            }

            if (!Guid.TryParse(orderId, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            var orderRepository = _unitOfWork.GetRepository<Order>();
            var order = await orderRepository.Entities
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.DeletedTime.HasValue) ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageOrderNotFound);

            if (order.Status != Constants.OrderStatusAwaitingPayment)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidOrderStatus);
            }

            if (order.UserId.ToString() != userId)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageUserNotOwner);
            }

            var paymentRepository = _unitOfWork.GetRepository<Payment>();
            var existingPayment = await paymentRepository.Entities
                .AnyAsync(p => p.OrderId == orderId && !p.DeletedTime.HasValue);
            if (existingPayment)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessagePaymentAlreadyExists);
            }

            var expirationDate = DateTime.UtcNow.AddDays(1);
            expirationDate = new DateTime(expirationDate.Year,
                                           expirationDate.Month,
                                           expirationDate.Day,
                                           expirationDate.Hour, 0, 0);

            var payment = new Payment
            {
                OrderId = orderId,
                TotalAmount = order.TotalPrice,
                Status = Constants.PaymentStatusPending,
                ExpirationDate = expirationDate,
                Method = Constants.PaymentMethodOnline
            };

            payment.CreatedBy = userId;
            payment.LastUpdatedBy = userId;

            await paymentRepository.InsertAsync(payment);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> CreatePaymentOfflineAsync(string userId, string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageEmptyId);
            }

            if (!Guid.TryParse(orderId, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            var orderRepository = _unitOfWork.GetRepository<Order>();
            var order = await orderRepository.Entities
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.DeletedTime.HasValue)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageOrderNotFound);

            if (order.UserId.ToString() != userId)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageUserNotOwner);
            }

            var paymentRepository = _unitOfWork.GetRepository<Payment>();
            var existingPayment = await paymentRepository.Entities
                .AnyAsync(p => p.OrderId == orderId && !p.DeletedTime.HasValue);
            if (existingPayment)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessagePaymentAlreadyExists);
            }

            var expirationDate = DateTime.UtcNow.AddDays(50);
            expirationDate = new DateTime(expirationDate.Year,
                                           expirationDate.Month,
                                           expirationDate.Day,
                                           expirationDate.Hour, 0, 0);

            var payment = new Payment
            {
                OrderId = orderId,
                TotalAmount = order.TotalPrice,
                Status = Constants.PaymentStatusPending,
                ExpirationDate = expirationDate,
                Method = Constants.PaymentMethodOffline
            };

            payment.CreatedBy = userId;
            payment.LastUpdatedBy = userId;

            await paymentRepository.InsertAsync(payment);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> UpdatePaymentStatusAsync(string paymentId, string newStatus, string userId)
        {
            ValidatePaymentStatus(paymentId, newStatus);

            var paymentRepository = _unitOfWork.GetRepository<Payment>();
            var payment = await paymentRepository.Entities
                .FirstOrDefaultAsync(p => p.Id == paymentId && !p.DeletedTime.HasValue)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessagePaymentNotFound);

            // Validate Status Flow
            var validStatusTransitions = new Dictionary<string, List<string>>
            {
                { Constants.PaymentStatusPending, new List<string> { Constants.PaymentStatusCompleted, Constants.PaymentStatusExpired, Constants.PaymentStatusFailed } },
                { Constants.PaymentStatusCompleted, new List<string> { Constants.PaymentStatusRefunded } }
            };

            var allValidStatuses = validStatusTransitions.Keys
                .Concat(validStatusTransitions.Values.SelectMany(v => v))
                .Distinct()
                .ToList();

            if (!allValidStatuses.Contains(newStatus))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), string.Format(Constants.ErrorMessageInvalidStatus, newStatus));
            }

            if (!validStatusTransitions.ContainsKey(payment.Status) ||
                !validStatusTransitions[payment.Status].Contains(newStatus))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                    string.Format(Constants.ErrorMessageInvalidStatusTransition, payment.Status, newStatus));
            }

            if (newStatus == Constants.PaymentStatusCompleted && payment.Method == Constants.PaymentMethodOnline)
            {
                var dto = new UpdateStatusOrderDto
                {
                    OrderId = payment.OrderId,
                    Status = Constants.OrderStatusProcessing
                };
                await OrderService.UpdateOrderStatusAsync(userId, dto);
            }

            // Will update when the CancelReason table has data
            if (newStatus == Constants.PaymentStatusExpired)
            {
                var cancelReasons = await _cancelReasonService.GetAll();
                var cancelReason = cancelReasons.FirstOrDefault(cr => cr.Description != null &&
                                                                    cr.Description.Contains(Constants.PaymentDescriptionFailed));
                if (cancelReason == null)
                {
                    var crDto = new CancelReasonForCreationDto
                    {
                        Description = Constants.PaymentDescriptionFailed,
                        RefundRate = 0
                    };
                    await _cancelReasonService.Create(crDto, Constants.RoleSystem);
                    cancelReasons = await _cancelReasonService.GetAll();
                    cancelReason = cancelReasons.FirstOrDefault(cr => cr.Description != null &&
                                                                cr.Description.Contains(Constants.PaymentDescriptionFailed));
                }

                if (cancelReason != null)
                {
                    var dto = new UpdateStatusOrderDto
                    {
                        OrderId = payment.OrderId,
                        Status = Constants.OrderStatusCanceled,
                        CancelReasonId = cancelReason.Id,
                    };
                    await OrderService.UpdateOrderStatusAsync(userId, dto);
                }
            }
            payment.Status = newStatus;
            payment.LastUpdatedTime = DateTime.UtcNow;
            payment.LastUpdatedBy = userId;

            paymentRepository.Update(payment);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<PaymentResponseModel> GetPaymentByOrderIdAsync(string orderId, string userId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageEmptyId);
            }

            if (!Guid.TryParse(orderId, out Guid parsedOrderId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            var orderRepository = _unitOfWork.GetRepository<Order>();
            var order = await orderRepository.Entities
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.DeletedTime.HasValue);

            if (order == null)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageOrderNotFound);
            }

            // Check if the user is the buyer
            if (order.UserId.ToString() != userId)
            {
                // If not the buyer, check if the user is the seller associated with any item in the order
                var orderDetailRepository = _unitOfWork.GetRepository<OrderDetail>();

                // Filter for order details with a valid ProductItem and check for a match on CreatedBy
                var isSeller = await orderDetailRepository.Entities
                    .Where(od => od.OrderId == orderId && !od.DeletedTime.HasValue && od.ProductItem != null)
                    .Select(od => od.ProductItem!.CreatedBy)
                    .AnyAsync(createdBy => createdBy.ToString() == userId);

                if (!isSeller)
                {
                    throw new BaseException.ForbiddenException(StatusCodeHelper.Forbidden.ToString(), Constants.ErrorMessageForbidden);
                }
            }

            var paymentRepository = _unitOfWork.GetRepository<Payment>();
            var payment = await paymentRepository.Entities
                .Include(p => p.PaymentDetails)
                .FirstOrDefaultAsync(p => p.OrderId == orderId && !p.DeletedTime.HasValue)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessagePaymentNotFound);

            var paymentDetails = payment.PaymentDetails.Select(pd => new PaymentDetailResponseModel
            {
                Id = pd.Id,
                PaymentId = pd.PaymentId,
                Status = pd.Status,
                Method = pd.Method,
                ExternalTransaction = pd.ExternalTransaction
            }).ToList();

            return new PaymentResponseModel
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                TotalAmount = payment.TotalAmount,
                Status = payment.Status,
                ExpirationDate = payment.ExpirationDate,
                Method = payment.Method,
                PaymentDetails = paymentDetails
            };
        }

        public async Task CheckAndExpirePaymentsAsync()
        {
            var paymentRepository = _unitOfWork.GetRepository<Payment>();
            var today = DateTime.UtcNow.Date;
            var expiredPayments = await paymentRepository.Entities
                .Where(p => p.ExpirationDate.HasValue &&
                    p.ExpirationDate.Value.Date == today &&
                    p.Status != Constants.PaymentStatusExpired &&
                    p.Status != Constants.PaymentStatusCompleted &&
                    p.Method == Constants.PaymentMethodOnline &&
                    !p.DeletedTime.HasValue)
                .ToListAsync();

            foreach (var payment in expiredPayments)
            {
                await UpdatePaymentStatusAsync(payment.Id.ToString(), Constants.PaymentStatusExpired, Constants.RoleSystem);
            }

            await _unitOfWork.SaveAsync();
        }

        private void ValidatePaymentStatus(string paymentId, string status)
        {
            if (string.IsNullOrWhiteSpace(paymentId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPaymentId);
            }

            if (!Guid.TryParse(paymentId, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            if (string.IsNullOrWhiteSpace(status) || !Regex.IsMatch(status, @"^[a-zA-Z]+$"))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidStatus);
            }
        }
    }
}
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.CancelReasonModelViews;
using HandmadeProductManagement.ModelViews.OrderModelViews;
using HandmadeProductManagement.ModelViews.PaymentDetailModelViews;
using HandmadeProductManagement.ModelViews.PaymentModelViews;
using HandmadeProductManagement.Repositories.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
                throw new BaseException.BadRequestException("invalid_order_id", "Please input order id.");
            }

            if (!Guid.TryParse(orderId, out _))
            {
                throw new BaseException.BadRequestException("invalid_order_id_format", "Order ID format is invalid. Example: 123e4567-e89b-12d3-a456-426614174000.");
            }

            var orderRepository = _unitOfWork.GetRepository<Order>();
            var order = await orderRepository.Entities
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.DeletedTime.HasValue);
            if (order == null)
            {
                throw new BaseException.NotFoundException("order_not_found", "Order not found.");
            }

            if (order.Status != "Awaiting Payment")
            {
                throw new BaseException.BadRequestException("invalid_order_status", "Order status must be 'Awaiting Payment'.");
            }

            if (order.UserId.ToString() != userId)
            {
                throw new BaseException.BadRequestException("user_not_owner", "User does not own the order.");
            }

            var paymentRepository = _unitOfWork.GetRepository<Payment>();
            var existingPayment = await paymentRepository.Entities
                .AnyAsync(p => p.OrderId == orderId && !p.DeletedTime.HasValue);
            if (existingPayment)
            {
                throw new BaseException.BadRequestException("payment_already_exists", "A payment for this order already exists.");
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
                Status = "Pending",
                ExpirationDate = expirationDate,
                Method = "Online"
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
                throw new BaseException.BadRequestException("invalid_order_id", "Please input order id.");
            }

            if (!Guid.TryParse(orderId, out _))
            {
                throw new BaseException.BadRequestException("invalid_order_id_format", "Order ID format is invalid. Example: 123e4567-e89b-12d3-a456-426614174000.");
            }

            var orderRepository = _unitOfWork.GetRepository<Order>();
            var order = await orderRepository.Entities
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.DeletedTime.HasValue);
            if (order == null)
            {
                throw new BaseException.NotFoundException("order_not_found", "Order not found.");
            }

            if (order.Status != "Pending")
            {
                throw new BaseException.BadRequestException("invalid_order_status", "Order status must be 'Pending'.");
            }

            if (order.UserId.ToString() != userId)
            {
                throw new BaseException.BadRequestException("user_not_owner", "User does not own the order.");
            }

            var paymentRepository = _unitOfWork.GetRepository<Payment>();
            var existingPayment = await paymentRepository.Entities
                .AnyAsync(p => p.OrderId == orderId && !p.DeletedTime.HasValue);
            if (existingPayment)
            {
                throw new BaseException.BadRequestException("payment_already_exists", "A payment for this order already exists.");
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
                Status = "Pending",
                ExpirationDate = expirationDate,
                Method = "Offline"
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
                .FirstOrDefaultAsync(p => p.Id == paymentId && !p.DeletedTime.HasValue);

            if (payment == null)
            {
                throw new BaseException.NotFoundException("payment_not_found", "Payment not found.");
            }

            // Validate Status Flow
            var validStatusTransitions = new Dictionary<string, List<string>>
            {
                { "Pending", new List<string> { "Completed", "Expired" } },
                { "Completed", new List<string> { "Refunded" } }
            };

            var allValidStatuses = validStatusTransitions.Keys
                .Concat(validStatusTransitions.Values.SelectMany(v => v))
                .Distinct()
                .ToList();

            if (!allValidStatuses.Contains(newStatus))
            {
                throw new BaseException.BadRequestException("invalid_status", $"Status {newStatus} is not a valid status.");
            }

            if (!validStatusTransitions.ContainsKey(payment.Status) ||
                !validStatusTransitions[payment.Status].Contains(newStatus))
            {
                throw new BaseException.BadRequestException("invalid_status_transition",
                    $"Cannot transition from {payment.Status} to {newStatus}.");
            }

            if (newStatus == "Completed" && payment.Method == "Online")
            {
                var dto = new UpdateStatusOrderDto
                {
                    OrderId = payment.OrderId,
                    Status = "Processing"
                };
                await OrderService.UpdateOrderStatusAsync(userId,dto);
            }

            //will update when the CancelReason table has data
                if (newStatus == "Expired")
                {
                    var cancelReasons = await _cancelReasonService.GetAll();
                    var cancelReason = cancelReasons.FirstOrDefault(cr => cr.Description != null && 
                                                                cr.Description.Contains("Payment failed"));
                    if (cancelReason == null)
                    {
                        var crDto = new CancelReasonForCreationDto
                        {
                            Description = "Payment failed",
                            RefundRate = 0
                        };
                        await _cancelReasonService.Create(crDto, "System");
                        cancelReasons = await _cancelReasonService.GetAll();
                        cancelReason = cancelReasons.FirstOrDefault(cr => cr.Description != null &&
                                                                    cr.Description.Contains("Payment failed"));
                    }

                    var dto = new UpdateStatusOrderDto
                    {
                        OrderId = payment.OrderId,
                        Status = "Canceled",
                        CancelReasonId = cancelReason.Id,
                    };
                    await OrderService.UpdateOrderStatusAsync(userId, dto);
                }

            payment.Status = newStatus;
            payment.LastUpdatedTime = DateTime.UtcNow;
            payment.LastUpdatedBy = userId;

            paymentRepository.Update(payment);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<PaymentResponseModel> GetPaymentByOrderIdAsync(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                throw new BaseException.BadRequestException("invalid_order_id", "Order ID is required.");
            }

            if (!Guid.TryParse(orderId, out Guid parsedOrderId))
            {
                throw new BaseException.BadRequestException("invalid_order_id_format", "Order ID format is invalid. Example: 123e4567-e89b-12d3-a456-426614174000.");
            }

            var orderRepository = _unitOfWork.GetRepository<Order>();
            var orderExists = await orderRepository.Entities
                .AnyAsync(o => o.Id == orderId && !o.DeletedTime.HasValue);
            if (!orderExists)
            {
                throw new BaseException.NotFoundException("order_not_found", "Order not found.");
            }

            var paymentRepository = _unitOfWork.GetRepository<Payment>();
            var payment = await paymentRepository.Entities
                .Include(p => p.PaymentDetails)
                .FirstOrDefaultAsync(p => p.OrderId == orderId && !p.DeletedTime.HasValue);

            if (payment == null)
            {
                throw new BaseException.NotFoundException("payment_not_found", "Payment not found.");
            }

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
                    p.Status != "Expired" && 
                    p.Status != "Completed" && 
                    p.Method == "Online" && 
                    !p.DeletedTime.HasValue)
                .ToListAsync();

            foreach (var payment in expiredPayments)
            {
                await UpdatePaymentStatusAsync(payment.Id.ToString(), "Expired", "System");
            }

            await _unitOfWork.SaveAsync();
        }

        private void ValidatePaymentStatus(string paymentId, string status)
        {
            if (string.IsNullOrWhiteSpace(paymentId))
            {
                throw new BaseException.BadRequestException("invalid_payment_id", "Payment ID is required.");
            }

            if (!Guid.TryParse(paymentId, out _))
            {
                throw new BaseException.BadRequestException("invalid_payment_id_format", "Payment ID format is invalid. Example: 123e4567-e89b-12d3-a456-426614174000.");
            }

            if (string.IsNullOrWhiteSpace(status) || !Regex.IsMatch(status, @"^[a-zA-Z]+$"))
            {
                throw new BaseException.BadRequestException("invalid_status", "Status is invalid or empty.");
            }
        }
    }
}
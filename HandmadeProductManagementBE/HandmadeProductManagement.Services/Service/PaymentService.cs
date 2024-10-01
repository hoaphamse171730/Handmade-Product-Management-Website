using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
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

        public PaymentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PaymentResponseModel> CreatePaymentAsync(CreatePaymentDto createPaymentDto)
        {
            ValidatePayment(createPaymentDto);

            var userRepository = _unitOfWork.GetRepository<ApplicationUser>();
            var userExists = await userRepository.Entities
                        .AnyAsync(u => u.Id.ToString() == createPaymentDto.UserId && !u.DeletedTime.HasValue && u.DeletedBy == null);
            if (!userExists)
            {
                throw new BaseException.ErrorException(404, "user_not_found", "User not found.");
            }

            var orderRepository = _unitOfWork.GetRepository<Order>();
            var order = await orderRepository.Entities.FirstOrDefaultAsync(o => o.Id == createPaymentDto.OrderId);
            if (order == null)
            {
                throw new BaseException.ErrorException(404, "order_not_found", "Order not found.");
            }

            if (order.Status != "Awaiting Payment")
            {
                throw new BaseException.BadRequestException("invalid_order_status", "Order status must be 'Awaiting Payment'.");
            }

            if (order.UserId.ToString() != createPaymentDto.UserId)
            {
                throw new BaseException.BadRequestException("user_not_owner", "User does not own the order.");
            }

            var paymentRepository = _unitOfWork.GetRepository<Payment>();

            var payment = new Payment
            {
                Id = Guid.NewGuid().ToString(),
                OrderId = createPaymentDto.OrderId,
                TotalAmount = createPaymentDto.TotalAmount,
                Status = "Pending",
                ExpirationDate = DateTime.UtcNow.AddDays(1)
            };

            payment.CreatedBy = createPaymentDto.UserId;
            payment.CreatedTime = DateTime.UtcNow;
            payment.LastUpdatedBy = createPaymentDto.UserId;
            payment.LastUpdatedTime = DateTime.UtcNow;

            await paymentRepository.InsertAsync(payment);
            await _unitOfWork.SaveAsync();

            return new PaymentResponseModel
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                TotalAmount = payment.TotalAmount,
                Status = payment.Status,
                ExpirationDate = payment.ExpirationDate,
            };
        }

        public async Task<PaymentResponseModel> UpdatePaymentStatusAsync(string paymentId, string status)
        {
            ValidatePaymentStatus(paymentId, status);

            var paymentRepository = _unitOfWork.GetRepository<Payment>();
            var payment = await paymentRepository.Entities
                        .FirstOrDefaultAsync(p => p.Id == paymentId && !p.DeletedTime.HasValue && p.DeletedBy == null);

            if (payment == null)
            {
                throw new BaseException.ErrorException(404, "payment_not_found", "Payment not found.");
            }

            payment.Status = status;
            payment.LastUpdatedTime = DateTime.UtcNow;

            paymentRepository.Update(payment);
            await _unitOfWork.SaveAsync();

            return new PaymentResponseModel
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                TotalAmount = payment.TotalAmount,
                Status = payment.Status,
                ExpirationDate = payment.ExpirationDate,
            };
        }

        public async Task<PaymentResponseModel> GetPaymentByOrderIdAsync(string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
            {
                throw new BaseException.BadRequestException("invalid_order_id", "Order ID is required.");
            }

            if (!Guid.TryParse(orderId, out Guid parsedOrderId))
            {
                throw new BaseException.BadRequestException("invalid_order_id_format", "Order ID format is invalid. Example: 123e4567-e89b-12d3-a456-426614174000.");
            }

            var orderRepository = _unitOfWork.GetRepository<Order>();
            var orderExists = await orderRepository.Entities
                        .AnyAsync(o => o.Id == orderId && !o.DeletedTime.HasValue && o.DeletedBy == null);
            if (!orderExists)
            {
                throw new BaseException.ErrorException(404, "order_not_found", "Order not found.");
            }

            var paymentRepository = _unitOfWork.GetRepository<Payment>();
            var payment = await paymentRepository.Entities
                .Include(p => p.PaymentDetails)
                .FirstOrDefaultAsync(p => p.OrderId == orderId && !p.DeletedTime.HasValue && p.DeletedBy == null);

            if (payment == null)
            {
                throw new BaseException.ErrorException(404, "payment_not_found", "Payment not found.");
            }

            return new PaymentResponseModel
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                TotalAmount = payment.TotalAmount,
                Status = payment.Status,
                ExpirationDate = payment.ExpirationDate,
            };
        }

        public async Task CheckAndExpirePaymentsAsync()
        {
            var paymentRepository = _unitOfWork.GetRepository<Payment>();
            var orderRepository = _unitOfWork.GetRepository<Order>();
            var today = DateTime.UtcNow.Date;
            var expiredPayments = await paymentRepository.Entities
                .Where(p => p.ExpirationDate.Date == today && p.Status != "Expired" && p.Status != "Completed"
                    && !p.DeletedTime.HasValue && p.DeletedBy == null)
                .ToListAsync();

            foreach (var payment in expiredPayments)
            {
                payment.Status = "Expired";
                payment.LastUpdatedTime = DateTime.UtcNow;
                paymentRepository.Update(payment);

                var order = await orderRepository.Entities
                                .FirstOrDefaultAsync(o => o.Id == payment.OrderId && !o.DeletedTime.HasValue && o.DeletedBy == null);
                if (order != null)
                {
                    order.Status = "Payment Failed";
                    order.LastUpdatedTime = DateTime.UtcNow;
                    orderRepository.Update(order);
                }
            }

            await _unitOfWork.SaveAsync();
        }

        private void ValidatePayment(CreatePaymentDto createPaymentDto)
        {
            if (string.IsNullOrEmpty(createPaymentDto.OrderId))
            {
                throw new BaseException.BadRequestException("invalid_order_id", "Please input order id.");
            }

            if (!Guid.TryParse(createPaymentDto.OrderId, out _))
            {
                throw new BaseException.BadRequestException("invalid_order_id_format", "Order ID format is invalid. Example: 123e4567-e89b-12d3-a456-426614174000.");
            }

            if (string.IsNullOrEmpty(createPaymentDto.UserId))
            {
                throw new BaseException.BadRequestException("invalid_user_id", "Please input user id.");
            }

            if (!Guid.TryParse(createPaymentDto.UserId, out _))
            {
                throw new BaseException.BadRequestException("invalid_user_id_format", "User ID format is invalid. Example: 123e4567-e89b-12d3-a456-426614174000.");
            }

            if (createPaymentDto.TotalAmount <= 0)
            {
                throw new BaseException.BadRequestException("invalid_amount", "Total amount must be a positive number.");
            }
        }

        private void ValidatePaymentStatus(string paymentId, string status)
        {
            if (string.IsNullOrEmpty(paymentId))
            {
                throw new BaseException.BadRequestException("invalid_payment_id", "Payment ID is required.");
            }

            if (!Guid.TryParse(paymentId, out _))
            {
                throw new BaseException.BadRequestException("invalid_payment_id_format", "Payment ID format is invalid. Example: 123e4567-e89b-12d3-a456-426614174000.");
            }

            if (string.IsNullOrEmpty(status) || !Regex.IsMatch(status, @"^[a-zA-Z]+$"))
            {
                throw new BaseException.BadRequestException("invalid_status", "Status is invalid or empty.");
            }
        }
    }
}
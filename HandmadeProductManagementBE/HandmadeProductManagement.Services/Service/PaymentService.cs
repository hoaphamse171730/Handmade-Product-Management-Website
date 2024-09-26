using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.PaymentDetailModelViews;
using HandmadeProductManagement.ModelViews.PaymentModelViews;
using HandmadeProductManagement.Repositories.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var userRepository = _unitOfWork.GetRepository<ApplicationUser>();
            var userExists = await userRepository.Entities.AnyAsync(u => u.Id == createPaymentDto.UserId);
            if (!userExists)
            {
                throw new BaseException.ErrorException(404, "user_not_found", "User not found.");
            }

            var orderRepository = _unitOfWork.GetRepository<Order>();
            var orderExists = await orderRepository.Entities.AnyAsync(o => o.Id == createPaymentDto.OrderId);
            if (!orderExists)
            {
                throw new BaseException.ErrorException(404, "order_not_found", "Order not found.");
            }

            if (!decimal.TryParse(createPaymentDto.TotalAmount.ToString(), out decimal totalAmount) || totalAmount <= 0)
            {
                throw new BaseException.BadRequestException("invalid_amount", "Total amount must be a positive number.");
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

            payment.CreatedBy = createPaymentDto.UserId.ToString();
            payment.CreatedTime = DateTime.UtcNow;
            payment.LastUpdatedBy = createPaymentDto.UserId.ToString();
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
            var paymentRepository = _unitOfWork.GetRepository<Payment>();
            var payment = await paymentRepository.Entities.FirstOrDefaultAsync(p => p.Id == paymentId);

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
            var orderRepository = _unitOfWork.GetRepository<Order>();
            var orderExists = await orderRepository.Entities.AnyAsync(o => o.Id == orderId);
            if (!orderExists)
            {
                throw new BaseException.ErrorException(404, "order_not_found", "Order not found.");
            }

            var paymentRepository = _unitOfWork.GetRepository<Payment>();
            var payment = await paymentRepository.Entities
                .Include(p => p.PaymentDetails)
                .FirstOrDefaultAsync(p => p.OrderId == orderId);

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
            var today = DateTime.UtcNow.Date;
            var expiredPayments = await paymentRepository.Entities
                .Where(p => p.ExpirationDate.Date == today && p.Status != "Expired" && p.Status != "Completed")
                .ToListAsync();

            foreach (var payment in expiredPayments)
            {
                payment.Status = "Expired";
                payment.LastUpdatedTime = DateTime.UtcNow;
                paymentRepository.Update(payment);
            }

            await _unitOfWork.SaveAsync();
        }
    }
}

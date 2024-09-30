using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.OrderModelViews;
using HandmadeProductManagement.Repositories.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HandmadeProductManagement.Services.Service
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<OrderResponseModel> CreateOrderAsync(CreateOrderDto createOrder)
        {
            ValidateOrder(createOrder);

            var userRepository = _unitOfWork.GetRepository<ApplicationUser>();
            var userExists = await userRepository.Entities.AnyAsync(u => u.Id == createOrder.UserId);
            if (!userExists)
            {
                throw new BaseException.ErrorException(404, "user_not_found", "User not found.");
            }

            var repository = _unitOfWork.GetRepository<Order>();

            var order = new Order
            {
                Id = Guid.NewGuid().ToString(),
                TotalPrice = createOrder.TotalPrice,
                OrderDate = DateTime.UtcNow,
                Status = createOrder.Status,
                UserId = createOrder.UserId,
                Address = createOrder.Address,
                CustomerName = createOrder.CustomerName,
                Phone = createOrder.Phone,
                Note = createOrder.Note,
                CancelReasonId = createOrder.CancelReasonId
            };

            order.CreatedBy = order.UserId.ToString();
            order.CreatedTime = DateTime.UtcNow;
            order.LastUpdatedBy = order.UserId.ToString();
            order.LastUpdatedTime = DateTime.UtcNow;

            await repository.InsertAsync(order);
            await _unitOfWork.SaveAsync();

            return new OrderResponseModel
            {
                Id = order.Id,
                TotalPrice = order.TotalPrice,
                OrderDate = order.OrderDate,
                Status = order.Status,
                UserId = order.UserId,
                Address = order.Address,
                CustomerName = order.CustomerName,
                Phone = order.Phone,
                Note = order.Note,
                CancelReasonId = order.CancelReasonId
            };
        }

        public async Task<bool> DeleteOrderAsync(string orderId)
        {
            var repository = _unitOfWork.GetRepository<Order>();
            var order = await repository.Entities.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                return false;
            }

            order.DeletedBy = order.UserId.ToString();
            order.DeletedTime = DateTime.UtcNow;

            repository.Update(order);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<IList<OrderResponseModel>> GetAllOrdersAsync()
        {
            IQueryable<Order> query = _unitOfWork.GetRepository<Order>().Entities
                .Where(order => order.DeletedBy == null);
            var result = await query.Select(order => new OrderResponseModel
            {
                Id = order.Id,
                TotalPrice = order.TotalPrice,
                OrderDate = order.OrderDate,
                Status = order.Status,
                UserId = order.UserId,
                Address = order.Address,
                CustomerName = order.CustomerName,
                Phone = order.Phone,
                Note = order.Note,
                CancelReasonId = order.CancelReasonId
            }).ToListAsync();

            return result;
        }

        public async Task<OrderResponseModel> GetOrderByIdAsync(string orderId)
        {
            var repository = _unitOfWork.GetRepository<Order>();
            var order = await repository.Entities
                .FirstOrDefaultAsync(o => o.Id == orderId && o.DeletedBy == null);

            if (order == null)
            {
                throw new BaseException.ErrorException(
                    404, "order_not_found", "Order not found.");
            }

            return new OrderResponseModel
            {
                Id = order.Id,
                TotalPrice = order.TotalPrice,
                OrderDate = order.OrderDate,
                Status = order.Status,
                UserId = order.UserId,
                Address = order.Address,
                CustomerName = order.CustomerName,
                Phone = order.Phone,
                Note = order.Note,
                CancelReasonId = order.CancelReasonId
            };
        }

        public async Task<OrderResponseModel> UpdateOrderAsync(string orderId, CreateOrderDto order)
        {
            if (string.IsNullOrWhiteSpace(orderId) || !Guid.TryParse(orderId, out _))
            {
                throw new BaseException.BadRequestException("invalid_order_id", "Order ID is not in the correct format. Ex: 123e4567-e89b-12d3-a456-426614174000.");
            }

            ValidateOrder(order);

            var repository = _unitOfWork.GetRepository<Order>();
            var existingOrder = await repository.Entities
                .FirstOrDefaultAsync(o => o.Id == orderId && o.DeletedBy == null);

            if (existingOrder == null)
            {
                throw new BaseException.ErrorException(
                    404, "order_not_found", "Order not found.");
            }

            existingOrder.TotalPrice = order.TotalPrice;
            existingOrder.Status = order.Status;
            existingOrder.Address = order.Address;
            existingOrder.CustomerName = order.CustomerName;
            existingOrder.Phone = order.Phone;
            existingOrder.Note = order.Note;
            existingOrder.CancelReasonId = order.CancelReasonId;
            existingOrder.LastUpdatedBy = order.UserId.ToString();
            existingOrder.LastUpdatedTime = DateTime.UtcNow;

            repository.Update(existingOrder);
            await _unitOfWork.SaveAsync();

            return new OrderResponseModel
            {
                Id = existingOrder.Id,
                TotalPrice = existingOrder.TotalPrice,
                OrderDate = existingOrder.OrderDate,
                Status = existingOrder.Status,
                UserId = existingOrder.UserId,
                Address = existingOrder.Address,
                CustomerName = existingOrder.CustomerName,
                Phone = existingOrder.Phone,
                Note = existingOrder.Note,
                CancelReasonId = existingOrder.CancelReasonId
            };
        }

        public async Task<IList<OrderResponseModel>> GetOrderByUserIdAsync(Guid userId)
        {
            var userRepository = _unitOfWork.GetRepository<ApplicationUser>();
            var userExists = await userRepository.Entities.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                throw new BaseException.ErrorException(404, "user_not_found", "User not found.");
            }

            var repository = _unitOfWork.GetRepository<Order>();
            var orders = await repository.Entities
                .Where(o => o.UserId == userId && o.DeletedBy == null)
                .Select(order => new OrderResponseModel
                {
                    Id = order.Id,
                    TotalPrice = order.TotalPrice,
                    OrderDate = order.OrderDate,
                    Status = order.Status,
                    UserId = order.UserId,
                    Address = order.Address,
                    CustomerName = order.CustomerName,
                    Phone = order.Phone,
                    Note = order.Note,
                    CancelReasonId = order.CancelReasonId
                }).ToListAsync();

            return orders;
        }

        public async Task<OrderResponseModel> UpdateOrderStatusAsync(string orderId, string status)
        {
            var repository = _unitOfWork.GetRepository<Order>();
            var existingOrder = await repository.Entities
                .FirstOrDefaultAsync(o => o.Id == orderId && o.DeletedBy == null);

            if (existingOrder == null)
            {
                throw new BaseException.ErrorException(
                    404, "order_not_found", "Order not found.");
            }

            existingOrder.Status = status;
            existingOrder.LastUpdatedBy = existingOrder.UserId.ToString();
            existingOrder.LastUpdatedTime = DateTime.UtcNow;

            repository.Update(existingOrder);
            await _unitOfWork.SaveAsync();

            return new OrderResponseModel
            {
                Id = existingOrder.Id,
                TotalPrice = existingOrder.TotalPrice,
                OrderDate = existingOrder.OrderDate,
                Status = existingOrder.Status,
                UserId = existingOrder.UserId,
                Address = existingOrder.Address,
                CustomerName = existingOrder.CustomerName,
                Phone = existingOrder.Phone,
                Note = existingOrder.Note,
                CancelReasonId = existingOrder.CancelReasonId
            };
        }

        private void ValidateOrder(CreateOrderDto order)
        {
            if (order.TotalPrice < 0)
            {
                throw new BaseException.BadRequestException("invalid_total_price", "Total price cannot be negative.");
            }

            if (string.IsNullOrWhiteSpace(order.UserId.ToString()) || !Guid.TryParse(order.UserId.ToString(), out _))
            {
                throw new BaseException.BadRequestException("invalid_user_id", "User ID is not in the correct format. Ex: 123e4567-e89b-12d3-a456-426614174000.");
            }

            if (string.IsNullOrWhiteSpace(order.Address) || !IsValidText(order.Address))
            {
                throw new BaseException.BadRequestException("invalid_address", "Address contains invalid characters.");
            }

            if (string.IsNullOrWhiteSpace(order.CustomerName) || !IsValidText(order.CustomerName))
            {
                throw new BaseException.BadRequestException("invalid_customer_name", "Customer name contains invalid characters.");
            }

            if (string.IsNullOrWhiteSpace(order.Phone) || !Regex.IsMatch(order.Phone, @"^\d+$"))
            {
                throw new BaseException.BadRequestException("invalid_phone", "Phone number contains invalid characters.");
            }

            if (string.IsNullOrWhiteSpace(order.Status) || !IsValidText(order.Status))
            {
                throw new BaseException.BadRequestException("invalid_status", "Status contains invalid characters.");
            }
        }

        private bool IsValidText(string text)
        {
            return Regex.IsMatch(text, @"^[a-zA-Z0-9\s]+$");
        }
    }
}

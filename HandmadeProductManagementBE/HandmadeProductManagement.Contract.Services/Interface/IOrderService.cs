﻿using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.ModelViews.OrderModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IOrderService
    {
        Task<IList<OrderResponseModel>> GetAllOrdersAsync();
        Task<OrderResponseModel> GetOrderByIdAsync(string orderId);
        Task<bool> CreateOrderAsync(CreateOrderDto createOrder);
        Task<bool> UpdateOrderStatusAsync(string orderId, string status);
        Task<IList<OrderResponseModel>> GetOrderByUserIdAsync(Guid userId);
    }
}

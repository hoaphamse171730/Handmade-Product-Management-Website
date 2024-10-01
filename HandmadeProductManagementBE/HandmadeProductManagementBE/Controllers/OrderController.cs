using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.OrderModelViews;
using HandmadeProductManagement.ModelViews.PromotionModelViews;
using HandmadeProductManagement.Services.Service;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HandmadeProductManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            var response = new BaseResponse<IList<OrderResponseModel>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Orders retrieved successfully",
                Data = orders
            };
            return Ok(response);
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(string orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            var response = new BaseResponse<OrderResponseModel>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Order retrieved successfully",
                Data = order
            };
            return Ok(response);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetOrderByUserId(Guid userId)
        {
            var orders = await _orderService.GetOrderByUserIdAsync(userId);
            var response = new BaseResponse<IList<OrderResponseModel>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Orders retrieved successfully",
                Data = orders
            };
            return Ok(response);
        }

        [HttpPatch("{orderId}/status")]
        public async Task<IActionResult> UpdateOrderStatus(string orderId, [FromBody] string status)
        {
            var updatedOrder = await _orderService.UpdateOrderStatusAsync(orderId, status);
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Order status updated successfully",
                Data = updatedOrder
            };
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto createOrder)
        {
            var order = await _orderService.CreateOrderAsync(createOrder);
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Order created successfully",
                Data = order
            };
            return Ok(response);
        }

        [HttpPut("{orderId}")]
        public async Task<IActionResult> UpdateOrder(string orderId, [FromBody] CreateOrderDto order)
        {
            var updatedOrder = await _orderService.UpdateOrderAsync(orderId, order);
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Order updated successfully",
                Data = updatedOrder
            };
            return Ok(response);
        }
    }
}
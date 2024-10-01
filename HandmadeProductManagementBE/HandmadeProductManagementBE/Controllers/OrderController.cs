using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.OrderModelViews;
using HandmadeProductManagement.ModelViews.PromotionModelViews;
using HandmadeProductManagement.Services.Service;
using Microsoft.AspNetCore.Mvc;

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
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                return Ok(BaseResponse<IList<OrderResponseModel>>.OkResponse(orders));
            }
            catch (BaseException.ErrorException ex)
            {
                return StatusCode(ex.StatusCode, new { ex.ErrorDetail.ErrorCode, ex.ErrorDetail.ErrorMessage });
            }
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(string orderId)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                return Ok(BaseResponse<OrderResponseModel>.OkResponse(order));
            }
            catch (BaseException.ErrorException ex)
            {
                return StatusCode(ex.StatusCode, new { ex.ErrorDetail.ErrorCode, ex.ErrorDetail.ErrorMessage });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetOrderByUserId(Guid userId)
        {
            try
            {
                var orders = await _orderService.GetOrderByUserIdAsync(userId);
                return Ok(BaseResponse<IList<OrderResponseModel>>.OkResponse(orders));
            }
            catch (BaseException.ErrorException ex)
            {
                return StatusCode(ex.StatusCode, new { ex.ErrorDetail.ErrorCode, ex.ErrorDetail.ErrorMessage });
            }
        }

        [HttpPatch("{orderId}/status")]
        public async Task<IActionResult> UpdateOrderStatus(string orderId, [FromBody] string status)
        {
            try
            {
                var updatedOrder = await _orderService.UpdateOrderStatusAsync(orderId, status);
                return Ok(BaseResponse<OrderResponseModel>.OkResponse(updatedOrder));
            }
            catch (BaseException.ErrorException ex)
            {
                return StatusCode(ex.StatusCode, new { ex.ErrorDetail.ErrorCode, ex.ErrorDetail.ErrorMessage });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto createOrder)
        {
            try
            {
                var order = await _orderService.CreateOrderAsync(createOrder);
                return Ok(BaseResponse<OrderResponseModel>.OkResponse(order));
            }
            catch (BaseException.ErrorException ex)
            {
                return StatusCode(ex.StatusCode, new { ex.ErrorDetail.ErrorCode, ex.ErrorDetail.ErrorMessage });
            }
        }

        [HttpPut("{orderId}")]
        public async Task<IActionResult> UpdateOrder(string orderId, [FromBody] CreateOrderDto order)
        {
            try
            {
                var updatedOrder = await _orderService.UpdateOrderAsync(orderId, order);
                return Ok(BaseResponse<OrderResponseModel>.OkResponse(updatedOrder));
            }
            catch (BaseException.ErrorException ex)
            {
                return StatusCode(ex.StatusCode, new { ex.ErrorDetail.ErrorCode, ex.ErrorDetail.ErrorMessage });
            }
        }
    }
}

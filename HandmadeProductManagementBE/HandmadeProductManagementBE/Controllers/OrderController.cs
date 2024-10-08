using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.OrderModelViews;
using Microsoft.AspNetCore.Authorization;
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

        [Authorize(Roles = "Customer")]
        [HttpGet("user")]
        public async Task<IActionResult> GetOrderByUserId()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var orders = await _orderService.GetOrderByUserIdAsync(Guid.Parse(userId));
            var response = new BaseResponse<IList<OrderResponseModel>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Orders retrieved successfully",
                Data = orders
            };
            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/user/{userId}")]
        public async Task<IActionResult> GetOrderByUserIdForAdmin(Guid userId)
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

        [HttpPatch("status")]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateStatusOrderDto dto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var updatedOrder = await _orderService.UpdateOrderStatusAsync(dto, userId);
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Order status updated successfully",
                Data = updatedOrder
            };
            return Ok(response);
        }

        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto createOrder)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var order = await _orderService.CreateOrderAsync(userId, createOrder);
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
        public async Task<IActionResult> UpdateOrder(string orderId, [FromBody] UpdateOrderDto order)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var updatedOrder = await _orderService.UpdateOrderAsync(userId, orderId, order);
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Order updated successfully",
                Data = updatedOrder
            };
            return Ok(response);
        }

        private IActionResult HandleErrorResponse(BaseException.ErrorException ex)
        {
            var response = new BaseResponse<object>
            {
                Code = ex.ErrorDetail.ErrorCode,
                StatusCode = (StatusCodeHelper)ex.StatusCode,
                Message = ex.ErrorDetail.ErrorMessage?.ToString(),
                Data = null
            };
            return StatusCode(ex.StatusCode, response);
        }
    }
}

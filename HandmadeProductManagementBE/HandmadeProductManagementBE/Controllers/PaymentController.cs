using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.PaymentModelViews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HandmadeProductManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [Authorize]
        [HttpPost("online/{orderId}")]
        public async Task<IActionResult> CreatePaymentOnline(string orderId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var createdPayment = await _paymentService.CreatePaymentOnlineAsync(userId, orderId);
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Payment created successfully",
                Data = createdPayment
            };
            return Ok(response);
        }

        [Authorize]
        [HttpPost("offline/{orderId}")]
        public async Task<IActionResult> CreatePaymentOffline(string orderId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var createdPayment = await _paymentService.CreatePaymentOfflineAsync(userId, orderId);
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Payment created successfully",
                Data = createdPayment
            };
            return Ok(response);
        }

        [Authorize]
        [HttpPut("{paymentId}/status")]
        public async Task<IActionResult> UpdatePaymentStatus(string paymentId, [FromBody] string status)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var updatedPayment = await _paymentService.UpdatePaymentStatusAsync(paymentId, status, userId);
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Payment status updated successfully",
                Data = updatedPayment
            };
            return Ok(response);
        }

        [Authorize]
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetPaymentByOrderId(string orderId)
        {
            var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
            var response = new BaseResponse<PaymentResponseModel>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Payment retrieved successfully",
                Data = payment
            };
            return Ok(response);
        }
    }
}
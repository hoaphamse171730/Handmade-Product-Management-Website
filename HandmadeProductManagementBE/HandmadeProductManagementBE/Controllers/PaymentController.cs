using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.PaymentModelViews;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

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

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto createPaymentDto)
        {
            try
            {
                var createdPayment = await _paymentService.CreatePaymentAsync(createPaymentDto);
                return Ok(BaseResponse<PaymentResponseModel>.OkResponse(createdPayment));
            }
            catch (BaseException.BadRequestException ex)
            {
                return BadRequest(new { ex.ErrorDetail.ErrorCode, ex.ErrorDetail.ErrorMessage });
            }
            catch (BaseException.ErrorException ex)
            {
                return StatusCode(ex.StatusCode, new { ex.ErrorDetail.ErrorCode, ex.ErrorDetail.ErrorMessage });
            }
        }

        [HttpPut("{paymentId}/status")]
        public async Task<IActionResult> UpdatePaymentStatus(string paymentId, [FromBody] string status)
        {
            try
            {
                var updatedPayment = await _paymentService.UpdatePaymentStatusAsync(paymentId, status);
                return Ok(BaseResponse<PaymentResponseModel>.OkResponse(updatedPayment));
            }
            catch (BaseException.ErrorException ex)
            {
                return StatusCode(ex.StatusCode, new { ex.ErrorDetail.ErrorCode, ex.ErrorDetail.ErrorMessage });
            }
        }

        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetPaymentByOrderId(string orderId)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
                return Ok(BaseResponse<PaymentResponseModel>.OkResponse(payment));
            }
            catch (BaseException.ErrorException ex)
            {
                return StatusCode(ex.StatusCode, new { ex.ErrorDetail.ErrorCode, ex.ErrorDetail.ErrorMessage });
            }
        }
    }
}
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
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
                var response = new BaseResponse<bool>
                {
                    Code = "Success",
                    StatusCode = StatusCodeHelper.OK,
                    Message = "Payment created successfully",
                    Data = createdPayment
                };
                return Ok(response);
            }
            catch (BaseException.BadRequestException ex)
            {
                var response = new BaseResponse<object>
                {
                    Code = ex.ErrorDetail.ErrorCode,
                    StatusCode = StatusCodeHelper.BadRequest,
                    Message = ex.ErrorDetail.ErrorMessage?.ToString(),
                    Data = null
                };
                return BadRequest(response);
            }
            catch (BaseException.ErrorException ex)
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

        [HttpPut("{paymentId}/status")]
        public async Task<IActionResult> UpdatePaymentStatus(string paymentId, [FromBody] string status)
        {
            try
            {
                var updatedPayment = await _paymentService.UpdatePaymentStatusAsync(paymentId, status);
                var response = new BaseResponse<bool>
                {
                    Code = "Success",
                    StatusCode = StatusCodeHelper.OK,
                    Message = "Payment status updated successfully",
                    Data = updatedPayment
                };
                return Ok(response);
            }
            catch (BaseException.ErrorException ex)
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

        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetPaymentByOrderId(string orderId)
        {
            try
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
            catch (BaseException.ErrorException ex)
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
}
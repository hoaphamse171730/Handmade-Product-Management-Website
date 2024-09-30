using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.PaymentDetailModelViews;
using Microsoft.AspNetCore.Mvc;

namespace HandmadeProductManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentDetailController : ControllerBase
    {
        private readonly IPaymentDetailService _paymentDetailService;

        public PaymentDetailController(IPaymentDetailService paymentDetailService)
        {
            _paymentDetailService = paymentDetailService;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentDetail([FromBody] CreatePaymentDetailDto createPaymentDetailDto)
        {
            try
            {
                var createdPaymentDetail = await _paymentDetailService.CreatePaymentDetailAsync(createPaymentDetailDto);
                return Ok(BaseResponse<PaymentDetailResponseModel>.OkResponse(createdPaymentDetail));
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

        [HttpGet("payment/{paymentId}")]
        public async Task<IActionResult> GetPaymentDetailByPaymentId(string paymentId)
        {
            try
            {
                var paymentDetail = await _paymentDetailService.GetPaymentDetailByPaymentIdAsync(paymentId);
                return Ok(BaseResponse<PaymentDetailResponseModel>.OkResponse(paymentDetail));
            }
            catch (BaseException.ErrorException ex)
            {
                return StatusCode(ex.StatusCode, new { ex.ErrorDetail.ErrorCode, ex.ErrorDetail.ErrorMessage });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentDetailById(string id)
        {
            try
            {
                var paymentDetail = await _paymentDetailService.GetPaymentDetailByIdAsync(id);
                return Ok(BaseResponse<PaymentDetailResponseModel>.OkResponse(paymentDetail));
            }
            catch (BaseException.ErrorException ex)
            {
                return StatusCode(ex.StatusCode, new { ex.ErrorDetail.ErrorCode, ex.ErrorDetail.ErrorMessage });
            }
        }
    }
}